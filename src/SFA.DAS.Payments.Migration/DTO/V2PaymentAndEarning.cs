using System;

namespace SFA.DAS.Payments.Migration.DTO
{
    public class V2PaymentAndEarning
    {
        public Guid RequiredPaymentEventId { get; set; }
        public int? LearningAimSequenceNumber { get; set; }
        public decimal? AmountDue { get; set; }

        public long Id { get; set; }
        public Guid EventId { get; set; }
        public Guid EarningEventId { get; set; }
        public Guid FundingSourceEventId { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public long Ukprn { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long LearnerUln { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public decimal Amount { get; set; }
        public byte CollectionPeriod { get; set; }
        public short AcademicYear { get; set; }
        public byte DeliveryPeriod { get; set; }
        public string LearningAimReference { get; set; }
        public int LearningAimProgrammeType { get; set; }
        public int LearningAimStandardCode { get; set; }
        public int LearningAimFrameworkCode { get; set; }
        public int LearningAimPathwayCode { get; set; }
        public string LearningAimFundingLineType { get; set; }
        public int ContractType { get; set; }
        public int TransactionType { get; set; }
        public int FundingSource { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public long JobId { get; set; }
        public long? AccountId { get; set; }
        public long? TransferSenderAccountId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EarningsStartDate { get; set; }
        public DateTime? EarningsPlannedEndDate { get; set; }
        public DateTime? EarningsActualEndDate { get; set; }
        public byte? EarningsCompletionStatus { get; set; }
        public decimal? EarningsCompletionAmount { get; set; }
        public decimal? EarningsInstalmentAmount { get; set; }
        public short? EarningsNumberOfInstalments { get; set; }
        public string AgreementId { get; set; }
        public long? ApprenticeshipId { get; set; }
    }
}
