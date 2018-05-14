using System;
using Arbor.KVConfiguration.Urns;
using Arbor.SyslogServer.Validation;
using JetBrains.Annotations;

namespace Arbor.SyslogServer.Logging
{
    [Urn(LoggingConstants.SerilogBaseUrn)]
    [UsedImplicitly]
    public class SerilogConfiguration : IValidationObject
    {
        public SerilogConfiguration(
            string seqUrl,
            string startupLogFilePath,
            string rollingLogFilePath,
            bool seqEnabled = false,
            bool rollingLogFilePathEnabled = false,
            bool consoleEnabled = false)
        {
            SeqUrl = seqUrl;
            StartupLogFilePath = startupLogFilePath;
            RollingLogFilePath = rollingLogFilePath;
            SeqEnabled = seqEnabled;
            RollingLogFilePathEnabled = rollingLogFilePathEnabled;
            ConsoleEnabled = consoleEnabled;
        }

        public bool SeqEnabled { get; }

        public bool RollingLogFilePathEnabled { get; }

        public bool ConsoleEnabled { get; }

        public string SeqUrl { get; }

        public string StartupLogFilePath { get; }

        public string RollingLogFilePath { get; }

        public bool IsValid =>
            string.IsNullOrWhiteSpace(SeqUrl) || Uri.TryCreate(SeqUrl, UriKind.Absolute, out Uri uri);
    }
}
