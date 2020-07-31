using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Zeebe.Client;

namespace ExecutePlan
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
              .ConfigureAppConfiguration((hostingContext, config) =>
              {
                  config.AddJsonFile("appsettings.json", optional: true);
                  config.AddEnvironmentVariables();

                  if (args != null)
                  {
                      config.AddCommandLine(args);
                  }
              })
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddOptions();
                  services.Configure<ZeebeConfig>(hostContext.Configuration.GetSection("Zeebe"));
                  services.AddSingleton(provider =>
                  {
                      var config = provider.GetService<IOptions<ZeebeConfig>>();
                      var client = ZeebeClient
                      .Builder()
                      .UseGatewayAddress(config.Value.ClusterAddress)
                      .UsePlainText()
                      .Build();
                      return client;
                  });

                  services.AddHostedService<ExecutePlanWorker>();
              })
              .ConfigureLogging((hostingContext, logging) =>
              {
                  logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                  logging.AddConsole();
              })
              .UseConsoleLifetime();

            await builder.RunConsoleAsync();
        }
    }
}
