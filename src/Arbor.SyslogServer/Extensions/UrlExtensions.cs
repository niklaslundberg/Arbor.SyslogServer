﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Arbor.SyslogServer.Extensions
{
    public static class UrlExtensions
    {
        public static string CreateQueryWithQuestionMark([NotNull] IEnumerable<KeyValuePair<string, string>> parameters)
        {
            return $"?{CreateQueryWithoutQuestionMark(parameters)}";
        }

        public static string CreateQueryWithoutQuestionMark(
            [NotNull] IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            string query =
                $"{string.Join("&", parameters.Select(parameter => $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"))}";

            return query;
        }

        public static Uri WithQueryFromParameters(
            [NotNull] this Uri uri,
            IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var builder = new UriBuilder(uri)
            {
                Query = CreateQueryWithoutQuestionMark(parameters)
            };

            return builder.Uri;
        }
    }
}
