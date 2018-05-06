using System;

namespace Arbor.SyslogServer.Areas.Syslog
{
    public class Message
    {
        public override string ToString()
        {
            return $"{nameof(Facility)}: {Facility}, {nameof(Severity)}: {Severity}, {nameof(Datestamp)}: {Datestamp}, {nameof(Hostname)}: {Hostname}, {nameof(Content)}: {Content}, {nameof(RemoteIP)}: {RemoteIP}, {nameof(LocalDate)}: {LocalDate}";
        }

        public FacilityType Facility { get; set; }
        public SeverityType Severity { get; set; }
        public DateTime Datestamp { get; set; }
        public string Hostname { get; set; }
        public string Content { get; set; }
        public string RemoteIP{ get; set; }
        public DateTime LocalDate { get; set; }
    }
}