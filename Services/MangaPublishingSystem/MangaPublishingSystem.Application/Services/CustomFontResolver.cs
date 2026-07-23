using System;
using System.IO;
using PdfSharpCore.Fonts;

namespace MangaPublishingSystem.Application.Services
{
    public class CustomFontResolver : IFontResolver
    {
        public string DefaultFontName => "Arial";

        private static readonly string[] RegularFontSearchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "arial.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "arial.ttf"),
            "/usr/share/fonts/truetype/msttcorefonts/Arial.TTF",
            "/usr/share/fonts/truetype/msttcorefonts/arial.ttf",
            "/usr/share/fonts/truetype/msttcorefonts/Arial.ttf",
            "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/usr/share/fonts/truetype/freefont/FreeSans.ttf",
            @"C:\Windows\Fonts\arial.ttf"
        };

        private static readonly string[] BoldFontSearchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "arialbd.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "arialbd.ttf"),
            "/usr/share/fonts/truetype/msttcorefonts/Arialbd.TTF",
            "/usr/share/fonts/truetype/msttcorefonts/arialbd.ttf",
            "/usr/share/fonts/truetype/msttcorefonts/Arial_Bold.ttf",
            "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
            "/usr/share/fonts/truetype/freefont/FreeSansBold.ttf",
            @"C:\Windows\Fonts\arialbd.ttf"
        };

        public byte[]? GetFont(string faceName)
        {
            if (faceName.Contains("Bold", StringComparison.OrdinalIgnoreCase) || faceName.EndsWith("#Bold", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var path in BoldFontSearchPaths)
                {
                    if (File.Exists(path))
                    {
                        return File.ReadAllBytes(path);
                    }
                }
            }

            foreach (var path in RegularFontSearchPaths)
            {
                if (File.Exists(path))
                {
                    return File.ReadAllBytes(path);
                }
            }

            return null;
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string faceName = "Arial";
            if (isBold) faceName += "#Bold";
            return new FontResolverInfo(faceName);
        }
    }
}
