using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MangaPublishingSystem.Application.DTOs.AI;
using MangaPublishingSystem.Application.IServices.AI;

namespace MangaPublishingSystem.Infrastructure.Services.ExternalAiClients
{
    public class GeminiClient : IGeminiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiClient> _logger;

        public GeminiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AiTagsResultDto> SuggestTagsAsync(string synopsis)
        {
            var result = new AiTagsResultDto { Success = false };
            try
            {
                var apiKey = _configuration["Gemini:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("Gemini API Key is missing. Returning mock tags.");
                    // Fallback to mock data if no API key is provided
                    result.Success = true;
                    result.Tags = new List<string> { "Hành động", "Phiêu lưu", "Giả tưởng" };
                    return result;
                }

                var prompt = $"Dựa vào tóm tắt truyện sau đây, hãy gợi ý cho tôi khoảng 3 đến 5 thể loại (genre/tags) phù hợp nhất bằng tiếng Việt. Chỉ trả về các thể loại ngăn cách bằng dấu phẩy, không giải thích thêm.\nTóm tắt: {synopsis}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Gemini API error: {response.StatusCode} - {errorResponse}");
                    result.ErrorMessage = "Lỗi khi gọi API Gemini.";
                    return result;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonResponse);

                var candidates = document.RootElement.GetProperty("candidates");
                if (candidates.GetArrayLength() > 0)
                {
                    var text = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        result.Tags = text.Split(',')
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrEmpty(t))
                            .ToList();
                        result.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while communicating with Gemini API.");
                result.ErrorMessage = "Đã xảy ra lỗi không xác định khi gợi ý thể loại.";
            }

            return result;
        }
    }
}
