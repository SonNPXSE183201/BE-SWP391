using System;
using System.IO;
using PdfSharpCore.Fonts;

namespace MangaPublishingSystem.Application.Services
{
    public class CustomFontResolver : IFontResolver
    {
        public string DefaultFontName => "Arial";

        private static readonly string[] ArialRegularPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "arial.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "arial.ttf"),
            "/app/Fonts/arial.ttf",
            "/usr/share/fonts/truetype/msttcorefonts/Arial.TTF",
            "/usr/share/fonts/truetype/msttcorefonts/arial.ttf",
            "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/usr/share/fonts/truetype/freefont/FreeSans.ttf",
            "/usr/share/fonts/truetype/noto/NotoSans-Regular.ttf",
            @"C:\Windows\Fonts\arial.ttf"
        };

        private static readonly string[] ArialBoldPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "arialbd.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "arialbd.ttf"),
            "/app/Fonts/arialbd.ttf",
            "/usr/share/fonts/truetype/msttcorefonts/Arialbd.TTF",
            "/usr/share/fonts/truetype/msttcorefonts/arialbd.ttf",
            "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
            "/usr/share/fonts/truetype/freefont/FreeSansBold.ttf",
            "/usr/share/fonts/truetype/noto/NotoSans-Bold.ttf",
            @"C:\Windows\Fonts\arialbd.ttf"
        };

        private static readonly string[] TimesRegularPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "times.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "times.ttf"),
            "/app/Fonts/times.ttf",
            "/usr/share/fonts/truetype/msttcorefonts/Times_New_Roman.ttf",
            "/usr/share/fonts/truetype/msttcorefonts/times.ttf",
            "/usr/share/fonts/truetype/liberation/LiberationSerif-Regular.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSerif.ttf",
            "/usr/share/fonts/truetype/freefont/FreeSerif.ttf",
            "/usr/share/fonts/truetype/noto/NotoSerif-Regular.ttf",
            @"C:\Windows\Fonts\times.ttf"
        };

        private static readonly string[] TimesBoldPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "timesbd.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "timesbd.ttf"),
            "/app/Fonts/timesbd.ttf",
            "/usr/share/fonts/truetype/msttcorefonts/Times_New_Roman_Bold.ttf",
            "/usr/share/fonts/truetype/msttcorefonts/timesbd.ttf",
            "/usr/share/fonts/truetype/liberation/LiberationSerif-Bold.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSerif-Bold.ttf",
            "/usr/share/fonts/truetype/freefont/FreeSerifBold.ttf",
            "/usr/share/fonts/truetype/noto/NotoSerif-Bold.ttf",
            @"C:\Windows\Fonts\timesbd.ttf"
        };

        public byte[]? GetFont(string faceName)
        {
            string[] searchPaths;

            if (faceName.Contains("TimesNewRoman", StringComparison.OrdinalIgnoreCase) ||
                faceName.Contains("Times", StringComparison.OrdinalIgnoreCase))
            {
                searchPaths = faceName.Contains("Bold", StringComparison.OrdinalIgnoreCase)
                    ? TimesBoldPaths
                    : TimesRegularPaths;
            }
            else
            {
                searchPaths = faceName.Contains("Bold", StringComparison.OrdinalIgnoreCase)
                    ? ArialBoldPaths
                    : ArialRegularPaths;
            }

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    Console.WriteLine($"[CustomFontResolver] Loaded font: {path} for faceName={faceName}");
                    return File.ReadAllBytes(path);
                }
            }

            // Fallback: nếu không tìm thấy font yêu cầu, thử trả về Arial Regular
            Console.WriteLine($"[CustomFontResolver] WARNING: Font not found for faceName={faceName}. Trying Arial fallback...");
            foreach (var path in ArialRegularPaths)
            {
                if (File.Exists(path))
                {
                    return File.ReadAllBytes(path);
                }
            }

            Console.WriteLine($"[CustomFontResolver] CRITICAL: No font file found at all!");
            return null;
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string faceName;

            if (familyName.Contains("Times", StringComparison.OrdinalIgnoreCase) ||
                familyName.Contains("serif", StringComparison.OrdinalIgnoreCase))
            {
                faceName = "TimesNewRoman";
            }
            else
            {
                faceName = "Arial";
            }

            if (isBold) faceName += "#Bold";
            return new FontResolverInfo(faceName);
        }
    }
}
