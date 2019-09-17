using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        private IJobWorker _fooWorker;
        private IJobWorker _barWorker;
        private IJobWorker _foobarWorker;
        private IJobWorker _barfooWorker;
        private CancellationTokenSource _shutdown = new CancellationTokenSource();

        public ZeebeWorker(IZeebeClient client, ILogger<ZeebeWorker> logger)
        {
            this._client = client;
            this._logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _fooWorker = _client.NewWorker()
                .JobType("foo")
                .Handler((c, j) => Handler(c, j).Wait())
                .MaxJobsActive(5)
                .Name(Environment.MachineName)
                .PollInterval(TimeSpan.FromSeconds(1))
                .Timeout(TimeSpan.FromMinutes(60))
                .Open();

            _barWorker = _client.NewWorker()
                .JobType("bar")
                .Handler((c, j) => Handler(c, j).Wait())
                .MaxJobsActive(5)
                .Name(Environment.MachineName)
                .PollInterval(TimeSpan.FromSeconds(1))
                .Timeout(TimeSpan.FromMinutes(60))
                .Open();

            _foobarWorker = _client.NewWorker()
              .JobType("barfoo")
              .Handler((c, j) => Handler(c, j).Wait())
              .MaxJobsActive(5)
              .Name(Environment.MachineName)
              .PollInterval(TimeSpan.FromSeconds(1))
              .Timeout(TimeSpan.FromMinutes(60))
              .Open();

            _barfooWorker = _client.NewWorker()
              .JobType("foobar")
              .Handler((c, j) => Handler(c, j).Wait())
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
            _fooWorker.Dispose();
            _barWorker.Dispose();
            await Task.Delay(500);
        }


        private async Task Handler(IJobClient client, IJob job)
        {
            if (!_shutdown.IsCancellationRequested)
            {
                _logger.LogInformation($"Job name: {job.Type}");

                await Task.Delay(200, _shutdown.Token);
                var jobKey = job.Key;

                if (job.Type == "bar")
                {
                    var variables = new { foo = (jobKey % 3 == 0) ? 100 : 0 };
                    await client.NewCompleteJobCommand(jobKey).Variables(JsonConvert.SerializeObject(variables)).Send();
                }
                else
                {
                    await client.NewCompleteJobCommand(jobKey).Send();
                }
            }
        }
    }
}
