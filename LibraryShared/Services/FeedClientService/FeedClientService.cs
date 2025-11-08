using LibraryShared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace LibraryShared.Services.FeedClientService
{
    public class FeedClientService : IFeedClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public FeedClientService(HttpClient httpClient,
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<bool> CheckFeedPostExistsAsync(int postId)
        {
            string cacheKey = $"feed_post_exists_{postId}";

            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                return cachedResult;

            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/feed/{postId}");
            if (!string.IsNullOrEmpty(token))
            {
                if (!token.StartsWith("Bearer "))
                    token = "Bearer " + token;

                request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
            }

            var response = await _httpClient.SendAsync(request);

            bool exists = response.IsSuccessStatusCode;

            _cache.Set(cacheKey, exists, TimeSpan.FromMinutes(2));

            return exists;
        }
        public async Task<bool> CreatePostFromImageAsync(string userId, int imageId, string mediaUrl, CancellationToken ct = default)
        {
            string cacheKey = $"created_post_{userId}_{imageId}";

            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            var newPost = new
            {
                UserId = userId,
                ImageId = imageId,
                PostType = "image",
                MediaUrl = mediaUrl,
                Content = ""
            };

            var content = new StringContent(
                JsonSerializer.Serialize(newPost),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"/api/feed", content, ct);

            bool success = response.IsSuccessStatusCode;

            _cache.Set(cacheKey, success, TimeSpan.FromMinutes(2));

            return success;
        }

        public async Task<PostDto?> GetPostByIdAsync(int postId)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/feed/{postId}");
            if (!string.IsNullOrEmpty(token))
            {
                if (!token.StartsWith("Bearer "))
                    token = "Bearer " + token;

                request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var post = JsonSerializer.Deserialize<PostDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return post;
        }

    }
}
