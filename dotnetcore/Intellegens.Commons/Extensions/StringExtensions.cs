using System;

namespace Intellegens.Commons.Extensions
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string enumValueAsString)
            where T : System.Enum
        {
            return (T)Enum.Parse(typeof(T), enumValueAsString);
        }
    }
}