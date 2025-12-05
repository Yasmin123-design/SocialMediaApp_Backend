using ChatService.Dtos;
using ChatService.Models;

namespace ChatService.Services.ChatServ
{
    public interface IChatService
    {
        Task<List<ThreadDto>> GetUserThreadsAsync(string currentUserId);
        Task<MessageThreadDto> StartConversationAsync(string userId, string otherUserId);
        Task<ChatMessageDto> SendMessageAsync(int threadId, string senderId, string text);
        Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int threadId, string userId);
    }

}
