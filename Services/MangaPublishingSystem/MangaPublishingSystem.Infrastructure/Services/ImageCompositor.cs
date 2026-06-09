using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.IServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MangaPublishingSystem.Infrastructure.Services
{
    public class ImageCompositor : IImageCompositor
    {
        private readonly HttpClient _httpClient;

        public ImageCompositor()
        {
            _httpClient = new HttpClient();
        }

        public async Task<byte[]> CompositeLayersAsync(string baseLayerUrl, List<(string overlayUrl, int zIndex)> layers)
        {
            Image baseImage = null;
            try
            {
                baseImage = await LoadImageFromUrlAsync(baseLayerUrl);
            }
            catch (Exception)
            {
                // Fallback: Tạo một ảnh trắng nếu URL không hợp lệ hoặc lỗi mạng
                baseImage = new Image<Rgba32>(800, 1200);
                baseImage.Mutate(ctx => ctx.BackgroundColor(Color.White));
            }

            try
            {
                foreach (var layer in layers)
                {
                    Image overlayImage = null;
                    try
                    {
                        overlayImage = await LoadImageFromUrlAsync(layer.overlayUrl);
                    }
                    catch (Exception)
                    {
                        // Fallback: Tạo một layer bán trong suốt đại diện cho nét vẽ trợ lý
                        overlayImage = new Image<Rgba32>(baseImage.Width, baseImage.Height);
                        // Tô màu xanh nhẹ bán trong suốt (alpha = 80)
                        overlayImage.Mutate(ctx => ctx.BackgroundColor(Color.FromRgba(129, 140, 248, 80)));
                    }

                    // Đảm bảo kích thước khớp với ảnh gốc
                    if (overlayImage.Width != baseImage.Width || overlayImage.Height != baseImage.Height)
                    {
                        overlayImage.Mutate(ctx => ctx.Resize(baseImage.Width, baseImage.Height));
                    }

                    // Ghép đè lớp vẽ Assistant lên ảnh gốc
                    baseImage.Mutate(ctx => ctx.DrawImage(overlayImage, new Point(0, 0), 1f));
                    overlayImage.Dispose();
                }

                using (var ms = new MemoryStream())
                {
                    await baseImage.SaveAsPngAsync(ms);
                    return ms.ToArray();
                }
            }
            finally
            {
                baseImage.Dispose();
            }
        }

        private async Task<Image> LoadImageFromUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid URL");
            }

            var bytes = await _httpClient.GetByteArrayAsync(url);
            using (var ms = new MemoryStream(bytes))
            {
                return await Image.LoadAsync(ms);
            }
        }
    }
}
