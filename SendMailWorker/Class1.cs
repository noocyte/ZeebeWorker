﻿////
////    Copyright (c) 2018 camunda services GmbH (info@camunda.com)
////
////    Licensed under the Apache License, Version 2.0 (the "License");
////    you may not use this file except in compliance with the License.
////    You may obtain a copy of the License at
////
////        http://www.apache.org/licenses/LICENSE-2.0
////
////    Unless required by applicable law or agreed to in writing, software
////    distributed under the License is distributed on an "AS IS" BASIS,
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
////    See the License for the specific language governing permissions and
////    limitations under the License.

//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Zeebe.Client;
//using Zeebe.Client.Api.Responses;
//using Zeebe.Client.Api.Worker;

//namespace Client.Examples
//{
//    internal class Program
//    {
//        private static readonly string ZeebeUrl = "localhost:26500";
//        private static readonly string WorkflowInstanceVariables = "{\"a\":\"123\"}";
//        private static readonly string JobType = "foo";
//        private static readonly string WorkerName = Environment.MachineName;
//        private static readonly long WORK_COUNT = 10L;

//        public static async Task Main(string[] args)
//        {
//            // create zeebe client
//            var client = ZeebeClient.Builder().UseGatewayAddress(ZeebeUrl).UsePlainText().Build();

//            //var top = await client.

//            ////await client.NewPublishMessageCommand().MessageName("csharp").CorrelationKey("wow").Variables("{\"realValue\":2}").Send();

//            //// deploy
//            //var deployResponse = await client.NewDeployCommand().AddResourceFile(@"c:\Users\jany\Downloads\order.yaml").Send();

//            //// create workflow instance
//            //var workflowKey = deployResponse.Workflows[0].WorkflowKey;

//            var workflowInstance = await client
//                .NewCreateWorkflowInstanceCommand()
//                .BpmnProcessId("bar")
//                .LatestVersion()
//                .Variables(WorkflowInstanceVariables)
//                .Send();

//            await client.NewSetVariablesCommand(workflowInstance.WorkflowInstanceKey).Variables("{\"wow\":\"this\"}").Local().Send();

//            for (var i = 0; i < WORK_COUNT; i++)
//            {
//                await client
//                    .NewCreateWorkflowInstanceCommand()
//                    .BpmnProcessId("bar")
//                    .LatestVersion()
//                    .Variables(WorkflowInstanceVariables)
//                    .Send();
//            }

//            // open job worker
//            using (var signal = new EventWaitHandle(false, EventResetMode.AutoReset))
//            {
//                client.NewWorker()
//                      .JobType(JobType)
//                      .Handler(HandleJob)
//                      .MaxJobsActive(5)
//                      .Name(WorkerName)
//                      .AutoCompletion()
//                      .PollInterval(TimeSpan.FromSeconds(1))
//                      .Timeout(TimeSpan.FromSeconds(10))
//                      .Open();

//                // blocks main thread, so that worker can run
//                signal.WaitOne();
//            }
//        }
//        private static int _c = 0;
//        private static void HandleJob(IJobClient jobClient, IJob job)
//        {
//            // business logic
//            _c++;
//            var jobKey = job.Key;
//            Console.WriteLine("Counter" + _c);

//            //if (jobKey % 3 == 0)
//            //{
//            //    jobClient.NewCompleteJobCommand(jobKey).Variables("{\"foo\":2}").Send();
//            //}
//            //else if (jobKey % 2 == 0)
//            //{
//            //    jobClient.NewFailCommand(jobKey).Retries(job.Retries - 1).ErrorMessage("Example fail").Send();
//            //}
//            //else
//            //{
//            //    // auto completion
//            //}
//        }
//    }
//}