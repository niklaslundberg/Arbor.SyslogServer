using System.Threading;
using System.Threading.Tasks;
using Arbor.SyslogServer.Application;
using Microsoft.AspNetCore.Hosting;

namespace Arbor.SyslogServer
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                using (App app = App.Create(cancellationTokenSource, args))
                {
                    await app.RunAsync(args);

                    await app.WebHost.WaitForShutdownAsync(cancellationTokenSource.Token);
                }
            }

            return 0;
        }
    }
}
