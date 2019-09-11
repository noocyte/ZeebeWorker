using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace SendMailWorker
{
    public class ZeebeWorker : IHostedService
    {
        private readonly IZeebeClient _client;
        private readonly ILogger<ZeebeWorker> _logger;
        private IJobWorker _worker;
        private CancellationTokenSource _shutdown = new CancellationTokenSource();

        public ZeebeWorker(IZeebeClient client, ILogger<ZeebeWorker> logger)
        {
            this._client = client;
            this._logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _worker = _client.NewWorker()
             .JobType("foo")
             .Handler(async (c, j) => await Handler(c, j))
             .MaxJobsActive(5)
             .Name(Environment.MachineName)
             .PollInterval(TimeSpan.FromSeconds(1))
             .Timeout(TimeSpan.FromMinutes(60))
             .Open();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _shutdown.Cancel();
            await Task.Delay(500);
        }


        private async Task Handler(IJobClient client, IJob job)
        {
            if (!_shutdown.IsCancellationRequested)
            {
                var address = job.VariablesAsDictionary["address"].ToString();
                _logger.LogInformation($"Sending mail to: {address}");

                await Task.Delay(200, _shutdown.Token);
                await client.NewCompleteJobCommand(job.Key).Send();
            }
        }
    }
}
