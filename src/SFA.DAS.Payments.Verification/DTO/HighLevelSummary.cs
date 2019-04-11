namespace SFA.DAS.Payments.Verification.DTO
{
    class HighLevelSummary
    {
        public int TransactionType { get; set; }
        public decimal V1Amount { get; set; }
        public decimal V2Amount { get; set; }
        public decimal Difference => V2Amount - V1Amount;

    }
}
