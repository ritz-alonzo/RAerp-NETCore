using System;

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
    }
}
