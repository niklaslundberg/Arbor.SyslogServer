﻿using System.Globalization;
using System.Linq;

namespace Arbor.SyslogServer.Extensions
{
    public static class StringExtensions
    {
        public static string MakeAnonymous(this string value, char separator, char replacementChar)
        {
            if (!value.HasValue())
            {
                return value;
            }

            string[] strings = value.Split(separator);

            if (strings.Length == 0)
            {
                return value;
            }

            if (strings.Length == 1)
            {
                return new string(replacementChar, strings.Length);
            }

            string result = strings[0] + separator + string.Join(separator.ToString(CultureInfo.InvariantCulture),
                                strings.Skip(1)
                                    .Where(text => text.HasValue())
                                    .Select(text => new string(replacementChar, text.Length)));
            return result;
        }

        public static string WithDefault(this string value, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return value;
        }

        public static bool AllHaveValue(params string[] values)
        {
            return values != null && values.All(value => !string.IsNullOrWhiteSpace(value));
        }

        public static bool HasValue(this string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }
    }
}
