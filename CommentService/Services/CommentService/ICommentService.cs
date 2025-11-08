using CommentService.Dtos;
using CommentService.Models;

namespace CommentService.Services.CommentService
{
    public interface ICommentService
    {
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId);
        Task<Comment?> AddCommentAsync(CommentDto dto);
        Task<bool> DeleteCommentAsync(int id, string userId);
        Task<Comment?> UpdateCommentAsync(int id, string userId, string newContent);
    }
}
