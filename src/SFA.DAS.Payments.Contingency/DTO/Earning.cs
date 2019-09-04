using System;

namespace SFA.DAS.Payments.Contingency.DTO
{
    public class Earning
    {
        public decimal OneToThree => TransactionType01 + TransactionType02 + TransactionType03;
        public decimal Incentives => TransactionType04 + TransactionType05 + TransactionType06 +
                                     TransactionType07 + TransactionType08 + TransactionType09 +
                                     TransactionType10 + TransactionType11 + TransactionType12 +
                                     TransactionType13 + TransactionType14 + TransactionType15 +
                                     TransactionType16;

        public decimal AllTransactions => OneToThree + Incentives;

        public decimal Amount { get; set; }
        public decimal TransactionType01 { get; set; }
        public decimal TransactionType02 { get; set; }
        public decimal TransactionType03 { get; set; }
        public decimal TransactionType04 { get; set; }
        public decimal TransactionType05 { get; set; }
        public decimal TransactionType06 { get; set; }
        public decimal TransactionType07 { get; set; }
        public decimal TransactionType08 { get; set; }
        public decimal TransactionType09 { get; set; }
        public decimal TransactionType10 { get; set; }
        public decimal TransactionType11 { get; set; }
        public decimal TransactionType12 { get; set; }
        public decimal TransactionType13 { get; set; }
        public decimal TransactionType14 { get; set; }
        public decimal TransactionType15 { get; set; }
        public decimal TransactionType16 { get; set; }
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
        public int ApprenticeshipContractType { get; set; }
    }
}
