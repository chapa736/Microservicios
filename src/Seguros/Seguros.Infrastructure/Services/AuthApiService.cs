using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Seguros.Core.Interfaces.Application;

namespace Seguros.Infrastructure.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _authApiUrl;

        public AuthApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _authApiUrl = configuration["AuthApiUrl"];
        }

        public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_authApiUrl}/api/Auth/register", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        }
    }
}
