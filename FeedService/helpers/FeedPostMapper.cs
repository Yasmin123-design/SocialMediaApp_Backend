using FeedService.Dtos;
using FeedService.Models;
using LibraryShared.Dtos;

namespace FeedService.helpers
{
    public static class FeedPostMapper
    {
        public static FeedPostWithUserDto ToFeedDto(
            FeedPost post,
            UserDto? user,
            ImageDto? image)
        {
            return new FeedPostWithUserDto
            {
                Id = post.Id,
                Content = post.Content,
                MediaUrl = post.MediaUrl,
                PostType = post.PostType,
                CreatedAt = post.CreatedAt,
                UserId = user?.Id,
                User = user,
                ImageId = post.ImageId,
                OriginalImagePath = image?.OriginalFilePath,
                FilteredImagePath = image?.FilteredFilePath
            };
        }
    }

}
