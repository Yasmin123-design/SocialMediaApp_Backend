using ChatService.Dtos;
using ChatService.Models;
using LibraryShared.Dtos;

namespace ChatService.Helpers
{
    public static class ChatMapper
    {
        public static ChatMessageDto ToDto(this ChatMessage msg, string? senderName = null, string? senderImage = null)
        {
            return new ChatMessageDto
            {
                Id = msg.Id,
                ThreadId = msg.ThreadId,
                SenderId = msg.SenderId,
                SenderName = senderName,
                SenderImage = senderImage,
                Text = msg.Text,
                SentAt = msg.SentAt
            };
        }

        public static MessageThreadDto ToDto(this MessageThread thread)
        {
            return new MessageThreadDto
            {
                Id = thread.Id,
                Title = thread.Title,
                ParticipantIds = thread.Participants.Select(p => p.UserId).ToList(),
                LastMessageAt = thread.LastMessageAt
            };
        }

        public static ThreadDto ToUserThreadDto(
        this MessageThread thread,
        string currentUserId,
        UserDto otherUser,
        ChatMessage? lastMessage)
        {
            var otherUserId = thread.Participants
                .First(p => p.UserId != currentUserId)
                .UserId;

            return new ThreadDto
            {
                ThreadId = thread.Id,
                OtherUserId = otherUserId,
                OtherUserName = otherUser?.FullName ?? "Unknown",
                LastMessage = lastMessage?.Text,
                ImageUser = otherUser?.Image
            };
        }
    }

}


