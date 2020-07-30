using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace ExecutePlan
{
    public class ExecutePlanWorker : IHostedService
    {
        private readonly IZeebeClient _client;
        private readonly ILogger<ExecutePlanWorker> _logger;
        private IJobWorker _executePlanWorker;

        public ExecutePlanWorker(IZeebeClient client, ILogger<ExecutePlanWorker> logger)
        {
            _client = client;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executePlanWorker = _client.NewWorker()
                .JobType("execute-plan")
                .Handler((c, j) => Handler(c, j).Wait())
                .MaxJobsActive(5)
                .Name(Environment.MachineName)
                .PollInterval(TimeSpan.FromSeconds(1))
                .Timeout(TimeSpan.FromMinutes(60))
                .Open();

            return Task.CompletedTask;
        }

        private async Task Handler(IJobClient client, IJob job)
        {
            _logger.LogInformation("Executing new plan");
            await Task.Delay(TimeSpan.FromSeconds(10));

            await client.NewCompleteJobCommand(job.Key).Send();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
