using System;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Queueing;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Payments.FM36Tool.PeriodEnd;

namespace SFA.DAS.Payments.FM36Tool.JobContext
{
    public class TopicPublishingServiceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ISerializationService _serializationService;
        private readonly string _serviceBusConnectionString;

        public TopicPublishingServiceFactory(IConfiguration configuration, ISerializationService serializationService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _serviceBusConnectionString = configuration.GetConnectionString("DcServicebusConnectionString");
        }

        public ITopicPublishService<JobContextDto> GetPeriodEndTaskPublisher(PeriodEndTask periodEndTask)
        {
            return Get("periodendtopic", "Payments");
        }

        public ITopicPublishService<JobContextDto> GetSubmissionPublisher(short academicYear)
        {
            return Get($"ilr{academicYear}submissiontopic", "GenerateFM36Payments");
        }

        private ITopicPublishService<JobContextDto> Get(string topicName, string subscriptionName)
        {
            var config = new TopicConfiguration(_serviceBusConnectionString, topicName, subscriptionName, 10, maximumCallbackTimeSpan: TimeSpan.FromMinutes(40));

            return new TopicPublishService<JobContextDto>(config, _serializationService);
        }
    }
}