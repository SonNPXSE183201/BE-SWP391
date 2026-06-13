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
