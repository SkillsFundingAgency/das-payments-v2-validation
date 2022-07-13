using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.FileService.Interface;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Serialization.Interfaces;
using SFA.DAS.Payments.FM36Tool.PeriodEnd;

namespace SFA.DAS.Payments.FM36Tool.JobContext
{
    public class DcHelper
    {
        private readonly IJsonSerializationService _serializationService;
        private readonly IFileService _azureFileService;
        private readonly TopicPublishingServiceFactory _topicPublishingServiceFactory;

        public DcHelper(IJsonSerializationService serializationService, IFileService azureFileService, TopicPublishingServiceFactory topicPublishingServiceFactory)
        {
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _azureFileService = azureFileService ?? throw new ArgumentNullException(nameof(azureFileService));
            _topicPublishingServiceFactory = topicPublishingServiceFactory ?? throw new ArgumentNullException(nameof(topicPublishingServiceFactory));
        }

        public async Task SendPeriodEndTask(short academicYear, byte collectionPeriod, long jobId, PeriodEndTask periodEndTask)
        {
            try
            {
                var dto = new JobContextDto
                {
                    JobId = jobId,
                    KeyValuePairs = new Dictionary<string, object>
                    {
                        { JobContextMessageKey.UkPrn, 0 },
                        { JobContextMessageKey.Filename, string.Empty },
                        { JobContextMessageKey.CollectionName, $"PE-DAS-{periodEndTask:G}{academicYear}" },
                        { JobContextMessageKey.CollectionYear, academicYear },
                        { JobContextMessageKey.ReturnPeriod, collectionPeriod },
                        { JobContextMessageKey.Username, "Period End" }
                    },
                    SubmissionDateTimeUtc = DateTime.UtcNow,
                    TopicPointer = 0,
                    Topics = new List<TopicItemDto>
                    {
                        new TopicItemDto
                        {
                            SubscriptionName = "Payments",
                            Tasks = new List<TaskItemDto>
                            {
                                new TaskItemDto
                                {
                                    SupportsParallelExecution = false,
                                    Tasks = new List<string> { periodEndTask.ToString("G") }
                                }
                            }
                        }
                    }
                };

                var publisher = _topicPublishingServiceFactory.GetPeriodEndTaskPublisher(periodEndTask);
                
                await publisher.PublishAsync(dto, new Dictionary<string, object> { { "To", "Payments" } }, "Payments");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task SubmitFm36(List<FM36Learner> learners, long ukprn, short collectionYear, byte collectionPeriod, long jobId)
        {
            var ilrSubmission = new FM36Global
            {
                UKPRN = (int)ukprn,
                Year = collectionYear.ToString(),
                Learners = learners
            };

            var json = _serializationService.Serialize(ilrSubmission);

            await SubmitFm36(json, ukprn, collectionYear, collectionPeriod, jobId);
        }

        private async Task SubmitFm36(string fm36Json, long ukprn, short academicYear, byte collectionPeriod, long jobId)
        {
            try
            {
                var container = $"ilr{academicYear}-files";
                var messagePointer = $"{ukprn}/{jobId}/FundingFm36Output.json";
                
                await using (var stream = await _azureFileService.OpenWriteStreamAsync(messagePointer, container, new CancellationToken()))
                await using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(fm36Json);
                }

                var dto = new JobContextDto
                {
                    JobId = jobId,
                    KeyValuePairs = new Dictionary<string, object>
                    {
                        {JobContextMessageKey.CollectionYear, academicYear},
                        {JobContextMessageKey.FundingFm36Output, messagePointer},
                        {JobContextMessageKey.Filename, messagePointer},
                        {JobContextMessageKey.UkPrn, ukprn},
                        {JobContextMessageKey.Container, container},
                        {JobContextMessageKey.ReturnPeriod, collectionPeriod },
                        {JobContextMessageKey.Username, "PV2-Automated" }
                    },
                    SubmissionDateTimeUtc = DateTime.UtcNow,
                    TopicPointer = 0,
                    Topics = new List<TopicItemDto>
                    {
                        new TopicItemDto
                        {
                            SubscriptionName = "GenerateFM36Payments",
                            Tasks = new List<TaskItemDto>
                            {
                                new TaskItemDto
                                {
                                    SupportsParallelExecution = false,
                                    Tasks = new List<string>()
                                }
                            }
                        }
                    }
                };

                var publisher = _topicPublishingServiceFactory.GetSubmissionPublisher(academicYear);

                await publisher.PublishAsync(dto, new Dictionary<string, object> { { "To", "GenerateFM36Payments" } }, "GenerateFM36Payments");

                await Task.Delay(500);

                dto.Topics.Add(new TopicItemDto
                {
                    SubscriptionName = "GenerateFM36Payments",
                    Tasks = new List<TaskItemDto>
                    {
                        new TaskItemDto
                        {
                            SupportsParallelExecution = false,
                            Tasks = new List<string>{"JobSuccess"}
                        }
                    }
                });

                dto.TopicPointer = 1;

                await publisher.PublishAsync(dto, new Dictionary<string, object> { { "To", "GenerateFM36Payments" } }, "GenerateFM36Payments");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
