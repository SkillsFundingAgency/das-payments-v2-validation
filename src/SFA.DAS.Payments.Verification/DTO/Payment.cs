using System;
using SFA.DAS.Payments.Verification.Constants;

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
        public int LearningAimStandardCode { get; set; }
        public int LearningAimProgrammeType { get; set; }
        public int LearningAimFrameworkCode { get; set; }
        public int LearningAimPathwayCode { get; set; }
        public int ContractType { get; set; }
        public string LearningAimReference { get; set; }
        public string CollectionPeriodName { get; set; }
        public int TransactionType { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public string LearningAimFundingLineType { get; set; }
        public int DeliveryPeriod { get; set; }
        public int AcademicYear { get; set; }
        public int FundingSource { get; set; }
        public decimal Amount { get; set; }
        public VerificationResult VerificationResult { get; set; }
    }
}
