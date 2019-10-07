namespace SFA.DAS.Payments.Verification.Earnings.DTO
{
    class OutputRow
    {
        public long Ukprn { get; set; }
        public decimal Payments { get; set; }
        public decimal Earnings { get; set; }
        public int Period { get; set; }
    }
}
