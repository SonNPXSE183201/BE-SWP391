using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Tasks;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Infrastructure.Models;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MangaPublishingSystem.Infrastructure.Services
{
    public class ImageCompositor : IImageCompositor
    {
        private readonly HttpClient _httpClient;
        private readonly MinioSettings _settings;

        public ImageCompositor(IOptions<MinioSettings> settings)
        {
            _settings = settings.Value;
            _httpClient = new HttpClient();
        }

        public async Task<byte[]> CompositeLayersAsync(string baseLayerUrl, List<CompositeLayerDto> layers)
        {
            Image baseImage = null;
            try
            {
                baseImage = await LoadImageFromUrlAsync(baseLayerUrl);
            }
            catch (Exception)
            {
                baseImage = new Image<Rgba32>(800, 1200);
                baseImage.Mutate(ctx => ctx.BackgroundColor(Color.White));
            }

            try
            {
                foreach (var layer in layers.OrderBy(l => l.ZIndex))
                {
                    Image overlayImage = null;
                    try
                    {
                        overlayImage = await LoadImageFromUrlAsync(layer.OverlayUrl);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (layer.Width > 0 && layer.Height > 0)
                    {
                        var x = Math.Clamp(layer.X, 0, Math.Max(0, baseImage.Width - 1));
                        var y = Math.Clamp(layer.Y, 0, Math.Max(0, baseImage.Height - 1));
                        var w = Math.Clamp(layer.Width, 1, baseImage.Width - x);
                        var h = Math.Clamp(layer.Height, 1, baseImage.Height - y);

                        overlayImage.Mutate(ctx => ctx.Resize(new ResizeOptions
                        {
                            Size = new Size(w, h),
                            Mode = ResizeMode.Stretch,
                        }));
                        baseImage.Mutate(ctx => ctx.DrawImage(overlayImage, new Point(x, y), 1f));
                    }

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
            var resolved = ResolveMediaUrl(url);
            if (string.IsNullOrWhiteSpace(resolved))
            {
                throw new ArgumentException("Invalid URL");
            }

            var bytes = await _httpClient.GetByteArrayAsync(resolved);
            using (var ms = new MemoryStream(bytes))
            {
                return await Image.LoadAsync(ms);
            }
        }

        private string ResolveMediaUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;

            var scheme = _settings.Secure ? "https" : "http";
            if (url.StartsWith('/'))
            {
                return $"{scheme}://{_settings.Endpoint}{url}";
            }

            return $"{scheme}://{_settings.Endpoint}/{_settings.BucketName}/{url.TrimStart('/')}";
        }
    }
}
