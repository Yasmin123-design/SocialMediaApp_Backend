using ChatService.Data;
using ChatService.Dtos;
using ChatService.Helpers;
using ChatService.Hubs;
using ChatService.Models;
using LibraryShared.Services.UserClientService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Services.ChatServ
{
    public class ChatService : IChatService
    {
        private readonly ChatDbContext _context;
        private readonly IUserClientService _userClient;
        private readonly IHubContext<ChatHub> _hub;

        public ChatService(ChatDbContext context, IUserClientService userClient, IHubContext<ChatHub> hub)
        {
            _context = context;
            _userClient = userClient;
            _hub = hub;
        }

        public async Task<MessageThreadDto> StartConversationAsync(string userId, string otherUserId)
        {
            if (userId == otherUserId)
                throw new Exception("You cannot chat with yourself.");

            if (!await _userClient.CheckUserExistsAsync(otherUserId))
                throw new Exception("User does not exist");

            var existing = await _context.MessageThreads
                .Include(t => t.Participants)
                .FirstOrDefaultAsync(t =>
                    t.Participants.Any(p => p.UserId == userId) &&
                    t.Participants.Any(p => p.UserId == otherUserId)
                );

            if (existing != null)
                return existing.ToDto();

            var thread = new MessageThread
            {
                Title = $"Chat between {userId} and {otherUserId}",
                Participants = new List<MessageThreadParticipant>
        {
            new MessageThreadParticipant { UserId = userId },
            new MessageThreadParticipant { UserId = otherUserId }
        }
            };

            _context.MessageThreads.Add(thread);
            await _context.SaveChangesAsync();

            return thread.ToDto();
        }
        public async Task<ChatMessageDto> SendMessageAsync(int threadId, string senderId, string text)
        {
            var thread = await _context.MessageThreads
                .Include(t => t.Participants)
                .FirstOrDefaultAsync(t => t.Id == threadId);

            if (thread == null)
                throw new Exception("Thread not found");

            if (!thread.Participants.Any(p => p.UserId == senderId))
                throw new Exception("User is not a participant");

            var msg = new ChatMessage
            {
                ThreadId = threadId,
                SenderId = senderId,
                Text = text
            };

            _context.ChatMessages.Add(msg);

            thread.LastMessageAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _hub.Clients.Group(threadId.ToString())
                .SendAsync("ReceiveMessage", msg.ToDto());

            return msg.ToDto();
        }
        public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int threadId, string userId)
        {
            if (!await _userClient.CheckUserExistsAsync(userId))
                throw new Exception("User does not exist");

            var thread = await _context.MessageThreads
                .Include(t => t.Participants)
                .FirstOrDefaultAsync(t => t.Id == threadId);

            if (thread == null)
                throw new Exception("Thread not found");

            if (!thread.Participants.Any(p => p.UserId == userId))
                throw new Exception("Not a participant");

            var messages = await _context.ChatMessages
                .Where(m => m.ThreadId == threadId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var senderIds = messages.Select(x => x.SenderId).Distinct();

            var userTasks = senderIds.Select(id => _userClient.GetUserByIdAsync(id));
            var users = await Task.WhenAll(userTasks);

            var userDict = users
                .Where(u => u != null)
                .ToDictionary(u => u!.Id, u => u);

            var result = messages.Select(msg =>
            {
                userDict.TryGetValue(msg.SenderId, out var user);

                return msg.ToDto(user?.FullName, user?.Image);
            });

            return result;
        }


        public async Task<List<ThreadDto>> GetUserThreadsAsync(string currentUserId)
        {
            var threads = await _context.MessageThreads
                .Include(t => t.Participants)
                .Include(t => t.Messages)
                .Where(t => t.Participants.Any(p => p.UserId == currentUserId))
                .ToListAsync();

            var result = new List<ThreadDto>();

            foreach (var thread in threads)
            {
                var otherUserId = thread.Participants
                    .First(p => p.UserId != currentUserId)
                    .UserId;

                var otherUser = await _userClient.GetUserByIdAsync(otherUserId);

                var lastMessage = thread.Messages
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefault();

                result.Add(thread.ToUserThreadDto(currentUserId, otherUser, lastMessage));
            }

            return result
                .ToList();
        }
    }

}
