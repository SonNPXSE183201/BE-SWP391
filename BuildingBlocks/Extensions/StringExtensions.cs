using System;
using System.Globalization;
using System.Text;

namespace BuildingBlocks.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Loại bỏ dấu tiếng Việt và chuyển chuỗi về dạng chữ thường.
        /// Thích hợp dùng cho lọc dữ liệu In-Memory (IEnumerable).
        /// </summary>
        public static string ToUnsignedLowerCase(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Chuyển về chữ thường trước
            var temp = text.Trim().ToLower(CultureInfo.InvariantCulture);

            // Thay thế ký tự đặc trưng của tiếng Việt: đ/Đ -> d
            temp = temp.Replace('đ', 'd').Replace('Đ', 'd');

            // Chuẩn hóa Unicode sang dạng tách rời các ký hiệu dấu (FormD)
            var normalizedString = temp.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                // Loại bỏ các dấu kết hợp (NonSpacingMark)
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Trả về chuỗi chuẩn hóa dạng dựng sẵn (FormC)
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Kiểm tra xem chuỗi nguồn có chứa chuỗi tìm kiếm hay không (In-Memory, không dấu, không phân biệt hoa thường).
        /// </summary>
        public static bool ContainsUnsigned(this string source, string search)
        {
            if (source == null) return false;
            if (string.IsNullOrEmpty(search)) return true;

            var unsignedSource = source.ToUnsignedLowerCase();
            var unsignedSearch = search.ToUnsignedLowerCase();

            return unsignedSource.Contains(unsignedSearch);
        }
    }
}
