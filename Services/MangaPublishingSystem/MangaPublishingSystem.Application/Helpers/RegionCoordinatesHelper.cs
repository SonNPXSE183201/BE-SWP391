using System.Text.Json;

namespace MangaPublishingSystem.Application.Helpers
{
    public static class RegionCoordinatesHelper
    {
        public static (int X, int Y, int Width, int Height) Parse(string? coordinatesJson)
        {
            if (string.IsNullOrWhiteSpace(coordinatesJson))
            {
                return (0, 0, 0, 0);
            }

            try
            {
                using var doc = JsonDocument.Parse(coordinatesJson);
                var root = doc.RootElement;

                int GetInt(string primary, string fallback = "")
                {
                    if (root.TryGetProperty(primary, out var v) && v.TryGetInt32(out var n)) return n;
                    if (!string.IsNullOrEmpty(fallback) && root.TryGetProperty(fallback, out var v2) && v2.TryGetInt32(out var n2)) return n2;
                    return 0;
                }

                return (
                    GetInt("left", "x"),
                    GetInt("top", "y"),
                    GetInt("width", "w"),
                    GetInt("height", "h")
                );
            }
            catch
            {
                return (0, 0, 0, 0);
            }
        }
    }
}
