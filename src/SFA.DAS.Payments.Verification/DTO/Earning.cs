

using System;
using ClosedXML.Excel;

namespace SFA.DAS.Payments.Verification.DTO
{
    struct Earning : IContainLearnerDetails
    {
        public long LearnerUln { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long Ukprn { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public DateTime EpisodeStartDate { get; set; }
        public DateTime EpisodeEffectiveTNPStartDate { get; set; }
        public int DeliveryPeriod { get; set; }
        public int LearningAimProgrammeType { get; set; }
        public int LearningAimFrameworkCode { get; set; }
        public int LearningAimPathwayCode { get; set; }
        public int LearningAimStandardCode { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public string LearningAimFundingLineType { get; set; }
        public string LearningAimReference { get; set; }
        public DateTime LearningStartDate { get; set; }
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
        public int ContractType { get; set; }
        public DateTime FirstIncentiveCensusDate { get; set; }
        public DateTime SecondIncentiveCensusDate { get; set; }
        public DateTime LearnerAdditionalPaymentsDate { get; set; }
        public decimal AgreedPrice { get; set; }
        public DateTime EndDate { get; set; }
        public int CumulativePmrs { get; set; }
        public int ExemptionCodeForCompletionHoldback { get; set; }
    }
}
