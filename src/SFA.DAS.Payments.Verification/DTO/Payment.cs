using System;

namespace SFA.DAS.Payments.Verification.DTO
{
    struct Payment : IContainLearnerDetails
    {
        public long LearnerUln { get; set; }
        public long CommitmentId { get; set; }
        public long AccountId { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long Ukprn { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public int StandardCode { get; set; }
        public int ProgrammeType { get; set; }
        public int FrameworkCode { get; set; }
        public int PathwayCode { get; set; }
        public int ContractType { get; set; }
        public string CollectionPeriodName { get; set; }
        public int TransactionType { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public string FundingLineType { get; set; }
        public int DeliveryPeriod { get; set; }
        public int AcademicYear { get; set; }
        public int FundingSource { get; set; }
        public decimal Amount { get; set; }
    }
}
