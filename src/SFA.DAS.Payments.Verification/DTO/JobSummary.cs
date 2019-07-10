namespace SFA.DAS.Payments.Verification.DTO
{
    class JobSummary
    {
        public string Heading { get; set; }
        public string Periods { get; set; }
        public string Ukprns { get; set; }
        public int NumberOfV1Learners { get; set; }
        public int NumberOfV2Learners { get; set; }
    }
}
