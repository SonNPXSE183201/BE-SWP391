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
                // If it is already UTC, return as-is.
                // If it is local (e.g. sent by FE in Vietnam timezone), we convert it to UTC for DB storage.
                return date.Kind == DateTimeKind.Utc 
                    ? date 
                    : TimeZoneInfo.ConvertTimeToUtc(date, VietnamTimeZone);
            }
            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Convert to Vietnam timezone (UTC+7) for presentation if it is UTC
            var localTime = value.Kind == DateTimeKind.Utc 
                ? TimeZoneInfo.ConvertTimeFromUtc(value, VietnamTimeZone) 
                : value;

            writer.WriteStringValue(localTime.ToString(_format));
        }
    }
}
