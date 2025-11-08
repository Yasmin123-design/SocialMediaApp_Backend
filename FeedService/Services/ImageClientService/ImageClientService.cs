using FeedService.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FeedService.Services.ImageClientService
{
    public class ImageClientService : IImageClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageClientService(HttpClient httpClient, 
            IMemoryCache memoryCache,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _httpClient = httpClient;
            _cache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ImageDto?> GetImageByIdAsync(int imageId, string token)
        {
            if (_cache.TryGetValue($"image_{imageId}", out ImageDto cachedImage))
            {
                return cachedImage;
            }


            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/image/{imageId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var image = JsonSerializer.Deserialize<ImageDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (image != null)
            {
                _cache.Set($"image_{imageId}", image, TimeSpan.FromMinutes(5));
            }

            return image;
        }

        public async Task DeleteImageAsync(int imageId)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(token))
                throw new Exception("No authorization token found");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/image/{imageId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to delete image with id {imageId}");
        }

    }
}
