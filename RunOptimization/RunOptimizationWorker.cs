using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace RunOptimization
{
    public class RunOptimizationWorker : IHostedService
    {
        private readonly IZeebeClient _client;
        private readonly ILogger<RunOptimizationWorker> _logger;
        private IJobWorker _runOptimizationWorker;

        public RunOptimizationWorker(IZeebeClient client, ILogger<RunOptimizationWorker> logger)
        {
            _client = client;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _runOptimizationWorker = _client.NewWorker()
                .JobType("run-optimize")
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
            _logger.LogInformation("Running new optimization based on updated price forecast");
            await Task.Delay(TimeSpan.FromSeconds(20));

            // random to see if it is better... 
            var betterPlanFound = new Random().Next(1, 100) > 75 ? 1 : 0;
            var isItBetter = betterPlanFound == 0 ? "No" : "Yes";
            var variables = new { betterPlanFound };
            _logger.LogInformation($"Is the new plan better?  {isItBetter}");

            await client.NewCompleteJobCommand(job.Key).Variables(JsonConvert.SerializeObject(variables)).Send();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
