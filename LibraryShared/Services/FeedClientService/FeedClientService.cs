using LibraryShared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
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
        public async Task<bool> CreatePostFromImageAsync(string userId, int imageId, string mediaUrl,string? content, CancellationToken ct = default)
        {
            string cacheKey = $"created_post_{userId}_{imageId}";

            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                return cachedResult;

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(userId), "UserId");
            formData.Add(new StringContent(imageId.ToString()), "ImageId");
            formData.Add(new StringContent("image"), "PostType");
            formData.Add(new StringContent(mediaUrl), "MediaUrl");
            formData.Add(new StringContent(content?? ""), "Content");

            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/feed")
            {
                Content = formData
            };

            if (!string.IsNullOrEmpty(token))
            {
                if (!token.StartsWith("Bearer "))
                    token = "Bearer " + token;

                request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
            }

            var response = await _httpClient.SendAsync(request, ct);

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
