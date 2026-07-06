using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MangaPublishingSystem.Application.DTOs.AI;
using MangaPublishingSystem.Application.IServices.AI;
using MangaPublishingSystem.Application.IServices;
using System;

namespace MangaPublishingSystem.Infrastructure.Services.ExternalAiClients
{
    public class FastApiVisionClient : IAiVisionClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FastApiVisionClient> _logger;
        private readonly IStorageService _storageService;

        public FastApiVisionClient(HttpClient httpClient, ILogger<FastApiVisionClient> logger, IStorageService storageService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _storageService = storageService;
        }

        public async Task<AiSegmentationResultDto> SegmentMangaPanelsAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestPayload = new { imageUrl = imageUrl };
                var response = await _httpClient.PostAsJsonAsync("api/v1/vision/segment", requestPayload, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AiSegmentationResultDto>(cancellationToken: cancellationToken);
                    return result ?? new AiSegmentationResultDto { Success = false };
                }

                _logger.LogWarning("Goi FastAPI that bai. StatusCode: {StatusCode}", response.StatusCode);
                return new AiSegmentationResultDto { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi ket noi toi dich vu FastAPI Vision.");
                return new AiSegmentationResultDto { Success = false };
            }
        }

        public async Task<AiColorizationResultDto> SegmentAndDrawMangaAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestPayload = new { imageUrl = imageUrl };
                var response = await _httpClient.PostAsJsonAsync("api/v1/vision/segment/draw", requestPayload, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var imageStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    
                    string fileName = $"segmented_{Guid.NewGuid():N}.jpg";
                    string uploadedUrl = await _storageService.UploadFileAsync(imageStream, fileName, "image/jpeg");

                    return new AiColorizationResultDto 
                    { 
                        Success = true, 
                        ColorizedImageUrl = uploadedUrl 
                    };
                }

                _logger.LogWarning("Goi FastAPI SegmentDraw that bai. StatusCode: {StatusCode}", response.StatusCode);
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new AiColorizationResultDto { Success = false, ErrorMessage = errorContent };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi ket noi toi dich vu FastAPI Vision SegmentDraw.");
                return new AiColorizationResultDto { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<AiColorizationResultDto> ColorizeMangaAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestPayload = new { imageUrl = imageUrl };
                var response = await _httpClient.PostAsJsonAsync("api/v1/vision/colorize", requestPayload, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var imageStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    
                    // Upload the colorized image back to MinIO via IStorageService
                    string fileName = $"colorized_{Guid.NewGuid():N}.png";
                    string uploadedUrl = await _storageService.UploadFileAsync(imageStream, fileName, "image/png");

                    return new AiColorizationResultDto 
                    { 
                        Success = true, 
                        ColorizedImageUrl = uploadedUrl 
                    };
                }

                _logger.LogWarning("Goi FastAPI Colorize that bai. StatusCode: {StatusCode}", response.StatusCode);
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new AiColorizationResultDto { Success = false, ErrorMessage = errorContent };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi ket noi toi dich vu FastAPI Vision Colorization.");
                return new AiColorizationResultDto { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}
