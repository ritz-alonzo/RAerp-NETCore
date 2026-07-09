using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using RA.WebFramework.Models.Pagination;
using System;
using System.Data.Entity;
using System.Runtime.CompilerServices;

namespace RA.WebFramework.Extensions
{
    public static class FrameworkExtensions
    {
        public static bool IsNullOrEmpty(this Guid value)
        {
            return value == Guid.Empty;
        }
        public static bool IsNullOrEmpty(this Guid? value)
        {
            return !value.HasValue || value == Guid.Empty;
        }

        public static bool IsNotNullOrEmpty(this Guid value)
        {
            return value != Guid.Empty;
        }
        public static bool IsNotNullOrEmpty(this Guid? value)
        {
            return value.HasValue && value != Guid.Empty;
        }
        public static bool IsNullOrEmptyJson(this string value)
        {
            return value == "{}";
        }
        public static bool IsNotNullOrEmptyJson(this string value)
        {
            return value != "{}";
        }
        public static bool HasAny(this List<Guid> value)
        {
            return value != null && value.Count > 0;
        }
        public static bool HasAny(this List<Guid?> value)
        {
            return value != null && value?.Count > 0;
        }
        public static bool HasAny(this List<int> value)
        {
            return value != null && value.Count > 0;
        }
        public static bool IsValidUrl(this string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
        public static bool IsValidImageFile(this IFormFile file)
        {
            var allowedExt = new[] { ".jpg", ".png", ".jpeg" };
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

            return allowedExt.Contains(extension) || allowedContentTypes.Contains(extension);
        }

        public static DateTime ConvertUTCToAppSettingsDateTime(this DateTime date, string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
            {
                timeZoneId = "Singapore Standard Time"; // default to Singapore Standard Time if not provided
            }

            var utcValue = date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcValue, tz);
        }

        public static DateTime ConvertUTCToAppSettingsDateTime(this DateTime? date, string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
            {
                timeZoneId = "Singapore Standard Time"; // default to Singapore Standard Time if not provided
            }
            var utcValue = date.Value.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcValue.Value, tz);
        }

        public static DateTime ConvertUTCToLocalDateTime(this DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        public static DateTime? ConvertUTCToLocalDateTime(this DateTime? date)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(date.Value, TimeZoneInfo.Local);
        }

        public static DateTime ConvertUTCToPHDateTime(this DateTime date)
        {
            var timeZones = TimeZoneInfo.GetSystemTimeZones();
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(date, tz);
        }

        public static DateTime? ConvertUTCToPHDateTime(this DateTime? date)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(date.Value, tz);
        }

        public static DateTime ConvertToUTC(this DateTime date)
        {
            return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.Local);
        }

        public static DateTime? ConvertToUTC(this DateTime? date)
        {
            return TimeZoneInfo.ConvertTimeToUtc(date.Value, TimeZoneInfo.Local);
        }

        public static PagedResult<T> ToPagedResult<T>(
        this IEnumerable<T> query, int? pageNumber = null, int? pageSize = null, CancellationToken ct = default)
        {
            var totalCount = query.Count();

            List<T> items;
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                if (pageNumber <= 0)
                    pageNumber = 1;

                if (pageSize <= 0)
                    pageSize = 10;

                items = query
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToList();
            }
            else
            {
                items = query.ToList();
            }

            return new PagedResult<T>(
                items,
                totalCount,
                pageNumber ?? 1,
                pageSize ?? totalCount
            ); // report the "effective" page size used
        }
    }
}
