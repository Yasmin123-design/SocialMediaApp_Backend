using CommentService.Dtos;
using CommentService.Models;

namespace CommentService.Services.CommentService
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentResult>> GetCommentsByPostIdAsync(int postId);
        Task<CommentResult?> AddCommentAsync(CommentDto dto);
        Task<CommentResult?> DeleteCommentAsync(int id, string userId);
        Task<CommentResult?> UpdateCommentAsync(int id, string userId, string newContent);
        Task<int> GetNoOfCommentsOfPostAsync(int postId);

    }
}
