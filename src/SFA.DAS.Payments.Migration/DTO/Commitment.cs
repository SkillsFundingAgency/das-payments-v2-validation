using System;

namespace SFA.DAS.Payments.Migration.DTO
{
    class Commitment
    {
        public long ApprenticeshipId { get; set; }
        public long EventId { get; set; }
        public string VersionId { get; set; }
        public long Uln { get; set; }
        public long Ukprn { get; set; }
        public long AccountId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AgreedCost { get; set; }
        public int? StandardCode { get; set; }
        public int? ProgrammeType { get; set; }
        public int? FrameworkCode { get; set; }
        public int? PathwayCode { get; set; }
        public int PaymentStatus { get; set; }
        public string PaymentStatusDescription { get; set; }
        public int Priority { get; set; }
        public DateTime EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }
        public long? TransferSendingEmployerAccountId { get; set; }
        public DateTime? TransferApprovalDate { get; set; }
        public DateTime? PausedOnDate { get; set; }
        public DateTime? WithdrawnOnDate { get; set; }
        public string LegalEntityName { get; set; }

        public int ApprenticeshipEmployerType { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }

        public int TrainingType { get; set; }
        public string TrainingCode { get; set; }
    }
}
