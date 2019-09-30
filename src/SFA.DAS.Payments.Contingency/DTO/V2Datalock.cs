namespace SFA.DAS.Payments.Contingency.DTO
{
    class V2Datalock
    {
        public long Ukprn { get; set; }
        public long Uln { get; set; }
        public string LearnRefNumber { get; set; }
        public string LearnAimRef { get; set; }
        public int StandardCode { get; set; }
        public int ProgrammeType { get; set; }
        public int FrameworkCode { get; set; }
        public int PathwayCode { get; set; }
        public byte DeliveryPeriod { get; set; }
        public byte TransactionType { get; set; }
    }
}


