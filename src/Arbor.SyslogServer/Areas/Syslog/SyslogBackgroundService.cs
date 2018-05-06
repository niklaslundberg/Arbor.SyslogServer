using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Arbor.SyslogServer.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;

namespace Arbor.SyslogServer.Areas.Syslog
{
    [UsedImplicitly]
    public class SyslogBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IClock _clock;
        private OnMessageReceived _handler;

        private UdpReceiveResult _receiveResult;
        private Logger _syslogLogger;

        public SyslogBackgroundService(ILogger logger, IClock clock, SerilogConfiguration serilogConfiguration)
        {
            _syslogLogger = new LoggerConfiguration().WriteTo.Seq(serilogConfiguration.SeqUrl).MinimumLevel.Debug().CreateLogger();

            _logger = logger;
            _clock = clock;
            _handler = message => _syslogLogger.Information("{HostName} {Application} {Message} {Severity} {RemoteIP}",
                message.Hostname,
                message.Facility,
                message.Content,
                message.Severity,
                message.RemoteIP);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("Starting syslog background service");

            var re = new Regex(@"^
(?<PRI>\<\d{1,3}\>)?
(?<HDR>
  (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s
  [0-3][0-9]\s
  [0-9]{2}\:[0-9]{2}\:[0-9]{2}\s
  [^ ]+?\s
)?
(?<MSG>.+)
",
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

            int port = 514;
            _logger.Information("Starting UDP client on port {Port}", port);

            using (var udp = new UdpClient(port))
            {
                while (!stoppingToken.IsCancellationRequested)

                {
                    try
                    {
                        _logger.Information("Waiting for UDP receive");
                        _receiveResult = await udp.ReceiveAsync();
                        _logger.Information("Received UDP message");
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }

                    Match m = re.Match(Encoding.ASCII.GetString(_receiveResult.Buffer));
                    if (m.Success)
                    {
                        Message msg = new Message();

                        if (m.Groups["PRI"].Success)
                        {
                            string pri = m.Groups["PRI"].Value;
                            int priority = int.Parse(pri.Substring(1, pri.Length - 2));
                            msg.Facility = (FacilityType)Math.Floor((double)priority / 8);
                            msg.Severity = (SeverityType)(priority % 8);
                        }
                        else
                        {
                            msg.Facility = FacilityType.User;
                            msg.Severity = SeverityType.Notice;
                        }

                        if (m.Groups["HDR"].Success)
                        {
                            string hdr = m.Groups["HDR"].Value.TrimEnd();
                            int idx = hdr.LastIndexOf(' ');
                            msg.Datestamp = DateTime.ParseExact(hdr.Substring(0, idx), "MMM dd HH:mm:ss", null);
                            msg.Hostname = hdr.Substring(idx + 1);
                        }
                        else
                        {
                            msg.Datestamp = _clock.UtcNow();

                            try
                            {
                                IPHostEntry he = Dns.GetHostEntry(_receiveResult.RemoteEndPoint.Address);
                                msg.Hostname = he.HostName;
                            }
                            catch (SocketException)
                            {
                                msg.Hostname = _receiveResult.RemoteEndPoint.Address.ToString();
                            }
                        }

                        msg.Content = m.Groups["MSG"].Value;
                        msg.RemoteIP = _receiveResult.RemoteEndPoint.Address.ToString();
                        msg.LocalDate = DateTime.Now;

                        _handler?.Invoke(msg);
                    }
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_syslogLogger is IDisposable disposable)
            {
                disposable.Dispose();
            } 
        }
    }
}
