using System.Collections.Generic;
using SFA.DAS.Payments.AnonymiserTool.Dto;

namespace SFA.DAS.Payments.AnonymiserTool
{
    class ReadOptimisedProviderData
    {
        public long Ukprn { get; set; }
        public Dictionary<long, List<ChangedLearner>> OptimisedLearners { get; set; } = new Dictionary<long, List<ChangedLearner>>();
    }
}