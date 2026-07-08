using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Presentation.Extensions
{
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        private readonly string _format = "yyyy-MM-dd HH:mm:ss";
        private static readonly TimeZoneInfo VietnamTimeZone = GetVietnamTimeZone();

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                // Windows timezone ID
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Linux/macOS/Docker fallback
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && DateTime.TryParse(reader.GetString(), out var date))
            {
                if (date.Kind == DateTimeKind.Utc)
                {
                    return date;
                }
                if (date.Kind == DateTimeKind.Local)
                {
                    return date.ToUniversalTime();
                }
                // For Unspecified, treat it as Vietnam Time (UTC+7) and convert to UTC
                return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date, DateTimeKind.Unspecified), VietnamTimeZone);
            }
            return reader.GetDateTime();
        }

        /// <summary>
        /// Chuyển DateTime sang giờ Việt Nam (UTC+7) trước khi serialize.
        /// EF Core đọc từ SQL Server trả về Kind = Unspecified (thực tế là UTC),
        /// nên cần coi Unspecified = UTC.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // EF Core trả Kind = Unspecified cho DateTime từ SQL Server → coi là UTC
            var localTime = value.Kind == DateTimeKind.Local
                ? value
                : TimeZoneInfo.ConvertTimeFromUtc(
                      DateTime.SpecifyKind(value, DateTimeKind.Utc),
                      VietnamTimeZone);

            writer.WriteStringValue(localTime.ToString(_format));
        }
    }

    /// <summary>
    /// Converter cho DateTime? — cần thiết vì JsonConverter&lt;DateTime&gt; không tự xử lý Nullable.
    /// </summary>
    public class NullableDateTimeJsonConverter : JsonConverter<DateTime?>
    {
        private readonly DateTimeJsonConverter _inner = new();

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            return _inner.Read(ref reader, typeof(DateTime), options);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            _inner.Write(writer, value.Value, options);
        }
    }
}

