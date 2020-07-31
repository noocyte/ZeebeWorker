using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Priceforecaster
{
    public class PriceforecasterWorker : IHostedService
    {
        private readonly IZeebeClient _client;
        private readonly ILogger<PriceforecasterWorker> _logger;
        private IJobWorker _priceforecasterWorker;

        public PriceforecasterWorker(IZeebeClient client, ILogger<PriceforecasterWorker> logger)
        {
            _client = client;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _priceforecasterWorker = _client.NewWorker()
                .JobType("get-priceforecast")
                .Handler((c, j) => Handler(c, j).Wait())
                .MaxJobsActive(5)
                .Name(Environment.MachineName + "priceforecast")
                .PollInterval(TimeSpan.FromSeconds(1))
                .Timeout(TimeSpan.FromMinutes(60))
                .Open();

            return Task.CompletedTask;
        }

        private async Task Handler(IJobClient client, IJob job)
        {
            _logger.LogInformation("Getting new price forecast from Wattsight API");
            await Task.Delay(TimeSpan.FromSeconds(2));

            _logger.LogInformation("Storing new price  forecast in Mesh Db");
            await Task.Delay(TimeSpan.FromSeconds(3));

            await client.NewCompleteJobCommand(job.Key).Send();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
