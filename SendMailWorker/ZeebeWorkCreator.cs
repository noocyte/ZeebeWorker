using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client;

namespace SendMailWorker
{
    public class ZeebeWorkCreator : IHostedService
    {
        private readonly IZeebeClient _client;
        private Task _backgroundTask;
        private CancellationTokenSource _shutdown =
          new CancellationTokenSource();

        public ZeebeWorkCreator(IZeebeClient client)
        {
            this._client = client;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _backgroundTask = Task.Run(BackgroundProceessing);
            return Task.CompletedTask;
        }

        private async Task BackgroundProceessing()
        {
            var i = 0;
            while (!_shutdown.IsCancellationRequested)
            {
                i++;
                var variables = new Dictionary<string, string>
                {
                    ["address"] = $"{i}_jarle@powel.com"
                };

                await _client
                     .NewCreateWorkflowInstanceCommand()
                     .BpmnProcessId("bar")
                     .LatestVersion()
                     .Variables(JsonConvert.SerializeObject(variables))
                     .Send();

                await Task.Delay(TimeSpan.FromSeconds(1), _shutdown.Token);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _shutdown.Cancel();
            return Task.CompletedTask;
        }
    }
}
