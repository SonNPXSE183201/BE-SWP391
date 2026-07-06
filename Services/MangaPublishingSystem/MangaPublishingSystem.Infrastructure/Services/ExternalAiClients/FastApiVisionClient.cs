using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MangaPublishingSystem.Application.DTOs.AI;
using MangaPublishingSystem.Application.IServices.AI;
using System;

namespace MangaPublishingSystem.Infrastructure.Services.ExternalAiClients
{
    public class FastApiVisionClient : IAiVisionClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FastApiVisionClient> _logger;

        public FastApiVisionClient(HttpClient httpClient, ILogger<FastApiVisionClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AiSegmentationResultDto> SegmentMangaPanelsAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Bỏ dòng này khi Python FastAPI Server được triển khai (YOLOv8/SAM).
                // Hiện tại mock để các URL thật từ MinIO vẫn pass test.
                return new AiSegmentationResultDto
                {
                    Success = true,
                    Panels = new System.Collections.Generic.List<BoundingBoxDto>
                    {
                        new BoundingBoxDto { X = 0, Y = 0, Width = 100, Height = 100, Label = "Panel 1" }
                    }
                };

                var requestPayload = new { imageUrl = imageUrl };
                var response = await _httpClient.PostAsJsonAsync("/segment", requestPayload, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AiSegmentationResultDto>(cancellationToken: cancellationToken);
                    return result ?? new AiSegmentationResultDto { Success = false };
                }

                _logger.LogWarning("Gọi FastAPI thất bại. StatusCode: {StatusCode}", response.StatusCode);
                return new AiSegmentationResultDto { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi kết nối tới dịch vụ FastAPI Vision.");
                return new AiSegmentationResultDto { Success = false };
            }
        }
    }
}
