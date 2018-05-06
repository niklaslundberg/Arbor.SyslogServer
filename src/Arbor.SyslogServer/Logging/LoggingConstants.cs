namespace Arbor.SyslogServer.Logging
{
    public static class LoggingConstants
    {
        internal const string SerilogBaseUrn = "urn:arbor:syslogserver:logging:serilog";
        
        public const string SerilogStartupLogFilePath = SerilogBaseUrn + ":startup-log-file-path";
    }
}
