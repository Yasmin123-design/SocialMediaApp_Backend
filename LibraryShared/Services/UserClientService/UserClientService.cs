using LibraryShared.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace LibraryShared.Services.UserClientService
{
    public class UserClientService : IUserClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public UserClientService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            string cacheKey = $"user_{userId}";

            if (_cache.TryGetValue(cacheKey, out UserDto cachedUser))
            {
                Console.WriteLine($"[CACHE] User {userId} retrieved from cache ✅");
                return cachedUser;
            }

            Console.WriteLine($"[HTTP] Fetching user {userId} from UserService 🌐");

            var response = await _httpClient.GetAsync($"/api/auth/{userId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));

            return user;
        }
        public async Task<bool> CheckUserExistsAsync(string userId)
        {
            string cacheKey = $"user_exists_{userId}";

            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                Console.WriteLine($"[CACHE] User {userId} found in cache ✅");
                return cachedResult;
            }

            Console.WriteLine($"[HTTP] Checking user {userId} existence 🌐");

            var response = await _httpClient.GetAsync($"/api/auth/{userId}");
            bool exists = response.IsSuccessStatusCode;

            _cache.Set(cacheKey, exists, TimeSpan.FromMinutes(10));

            return exists;
        }
    }
}
