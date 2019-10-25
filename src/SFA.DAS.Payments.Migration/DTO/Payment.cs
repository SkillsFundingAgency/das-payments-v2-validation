using System;

namespace SFA.DAS.Payments.Migration.DTO
{
    internal class Payment 
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public Guid EarningEventId { get; set; } = Guid.Empty;
        public Guid FundingSourceEventId { get; set; } = Guid.Empty;
        public DateTimeOffset EventTime { get; set; } = DateTime.Now;
        public long JobId { get; set; } = 0;



        public long LearnerUln { get; set; }
        public long? ApprenticeshipId { get; set; }
        public long? AccountId { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long Ukprn { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public string PriceEpisodeIdentifier { get; set; }


        public int LearningAimStandardCode { get; set; }
        public int LearningAimProgrammeType { get; set; }
        public int LearningAimFrameworkCode { get; set; }
        public int LearningAimPathwayCode { get; set; }



        public byte ContractType { get; set; }
        public string LearningAimReference { get; set; }
        public byte CollectionPeriod { get; set; }
        public byte TransactionType { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        
        public string LearningAimFundingLineType { get; set; }
        public byte DeliveryPeriod { get; set; }
        public short AcademicYear { get; set; }
        public byte FundingSource { get; set; }
        public decimal Amount { get; set; }

        public long? TransferSenderAccountId { get; set; }
        public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;

        public DateTime EarningsStartDate { get; set; }
        public DateTime? EarningsPlannedEndDate { get; set; }
        public DateTime? EarningsActualEndDate { get; set; }
        public int EarningsCompletionStatus { get; set; }
        public decimal EarningsCompletionAmount { get; set; }
        public decimal EarningsInstalmentAmount { get; set; }
        public int EarningsNumberOfInstalments { get; set; }
        public DateTime? LearningStartDate { get; set; }

        public byte ApprenticeshipEmployerType { get; set; } = 0;

    }
}
