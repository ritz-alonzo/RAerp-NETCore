using Microsoft.AspNetCore.Mvc.Rendering;
using System;
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
    }
}
