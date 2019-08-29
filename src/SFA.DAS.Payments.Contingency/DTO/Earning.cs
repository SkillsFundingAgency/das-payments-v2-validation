using System;

namespace SFA.DAS.Payments.Contingency.DTO
{
    public class Earning
    {
        public decimal Amount { get; set; }
        public string LearnRefNumber { get; set; }
        public long Ukprn { get; set; }
        public DateTime EpisodeEffectiveTNPStartDate { get; set; }
        public long Uln { get; set; }
        public int PathwayCode { get; set; }
        public int ProgrammeType { get; set; }
        public int FrameworkCode { get; set; }
        public int StandardCode { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public string FundingLineType { get; set; }
        public int AimSeqNumber { get; set; }
        public decimal TotalPrice { get; set; }
        public bool MathsAndEnglish { get; set; }
    }
}
