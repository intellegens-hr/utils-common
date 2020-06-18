using System;

namespace Intellegens.Commons.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Simple string to enum conversion
        /// </summary>
        /// <typeparam name="T">Target enum type</typeparam>
        /// <param name="enumValueAsString">Enum string value</param>
        /// <returns></returns>
        public static T ToEnum<T>(this string enumValueAsString)
            where T : System.Enum
        {
            return (T)Enum.Parse(typeof(T), enumValueAsString);
        }
    }
}