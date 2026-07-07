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

                var prompt = $@"Dựa vào tóm tắt truyện sau, hãy chọn 3 đến 5 thể loại phù hợp nhất từ danh sách sau:
- Action (Hành động)
- Comedy (Hài hước)
- Romance (Lãng mạn)
- Fantasy (Kỳ ảo)
- Sci-Fi (Khoa học viễn tưởng)
- Horror (Kinh dị)
- Mystery (Bí ẩn)
- Thriller (Ly kỳ)
- Sports (Thể thao)
- Historical (Lịch sử)
- Slice of Life (Đời thường)
- Mecha (Cơ giáp)
- Isekai (Xuyên không)
- Shōnen (Nam thiếu niên)
- Shōjo (Nữ thiếu niên)
- Seinen (Nam thanh niên)
- Josei (Nữ thanh niên)
- Kodomo (Thiếu nhi)

Chỉ trả về MỘT mảng JSON chứa các từ khóa TIẾNG ANH, tuyệt đối không giải thích thêm.
Ví dụ: [""Action"", ""Romance""]
Tóm tắt: {synopsis}";

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
                    },
                    generationConfig = new
                    {
                        responseMimeType = "application/json"
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
                        try 
                        {
                            result.Tags = JsonSerializer.Deserialize<List<string>>(text) ?? new List<string>();
                            result.Success = true;
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to parse Gemini JSON output: " + text);
                            // Fallback if the model somehow returned comma separated despite the prompt
                            result.Tags = text.Split(',')
                                .Select(t => t.Trim().Trim('"').Trim('[').Trim(']'))
                                .Where(t => !string.IsNullOrEmpty(t))
                                .ToList();
                            result.Success = true;
                        }
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
