namespace SFA.DAS.Payments.Verification.DTO
{
    struct RequiredPayment : IContainLearnerDetails
    {
        public long LearnerUln { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public long Ukprn { get; set; }
        public int ContractType { get; set; }
        public int TransactionType { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public decimal Amount { get; set; }
        public int CollectionPeriod { get; set; }
        public int AcademicYear { get; set; }
        public int DeliveryPeriod { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public string LearningAimReference { get; set; }
        public int LearningAimProgrammeType { get; set; }
        public int LearningAimStandardCode { get; set; }
        public int LearningAimFrameworkCode { get; set; }
        public int LearningAimPathwayCode { get; set; }
        public string LearningAimFundingLineType { get; set; }
        public long AccountId { get; set; }
    }
}
