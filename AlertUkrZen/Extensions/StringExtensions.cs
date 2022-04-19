using System;

namespace AlertUkrZen.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string firstString, string secondString)
        {
            return firstString is not null &&
                   firstString.Equals(secondString, StringComparison.OrdinalIgnoreCase);
        }
    }
}