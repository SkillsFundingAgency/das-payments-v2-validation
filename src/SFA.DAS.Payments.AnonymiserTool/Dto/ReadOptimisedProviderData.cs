using System.Collections.Generic;

namespace SFA.DAS.Payments.AnonymiserTool.Dto
{
    internal class ReadOptimisedProviderData
    {
        public Dictionary<long, List<ChangedLearner>> OptimisedLearners { get; set; } = new Dictionary<long, List<ChangedLearner>>();
    }
}