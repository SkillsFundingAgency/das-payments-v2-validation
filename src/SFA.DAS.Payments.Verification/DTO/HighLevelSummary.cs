namespace SFA.DAS.Payments.Verification.DTO
{
    class HighLevelSummary
    {
        public int TransactionType { get; set; }
        public decimal V1PaymentsAmount { get; set; }
        public decimal V2PaymentsAmount { get; set; }
        public decimal PaymentDifference => V1PaymentsAmount - V2PaymentsAmount;
        public decimal V1RequiredPaymentsAmount { get; set; }
        public decimal V2RequiredPaymentsAmount { get; set; }
        public decimal RequiredPaymentsDifference => V1RequiredPaymentsAmount - V2RequiredPaymentsAmount;
    }
}
