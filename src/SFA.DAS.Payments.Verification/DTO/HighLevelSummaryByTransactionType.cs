namespace SFA.DAS.Payments.Verification.DTO
{
    class HighLevelSummaryByTransactionType
    {
        public string Heading { get; set; }
        public int TransactionType { get; set; }
        public decimal V1PaymentsAmount { get; set; }
        public decimal V2PaymentsAmount { get; set; }
        public decimal PaymentDifference => V1PaymentsAmount - V2PaymentsAmount;
        public decimal PaymentsV2Percentage => (V2PaymentsAmount / V1PaymentsAmount) * 100;
        public decimal V1RequiredPaymentsAmount { get; set; }
        public decimal V2RequiredPaymentsAmount { get; set; }
        public decimal RequiredPaymentsDifference => V1RequiredPaymentsAmount - V2RequiredPaymentsAmount;
        public int NumberOfV1Payments { get; set; }
        public int NumberOfV2Payments { get; set; }
        public int DifferenceInNumberOfPayments => NumberOfV2Payments - NumberOfV1Payments;
        public int NumberOfV1RequiredPayments { get; set; }
        public int NumberOfV2RequiredPayments { get; set; }
        public int DifferenceInNumberOfRequiredPayments => NumberOfV2RequiredPayments - NumberOfV1RequiredPayments;
        public decimal AbsoluteSumOfV1OnlyPayments { get; set; }
        public decimal AbsoluteSumOfV2OnlyPayments { get; set; }
        public decimal AbsoluteSumOfV1OnlyRequiredPayments { get; set; }
        public decimal AbsoluteSumOfV2OnlyRequiredPayments { get; set; }
        public int NumberOfV1Learners { get; set; }
        public int NumberOfV2Learners { get; set; }
        public int DifferenceInLearners => NumberOfV2Learners - NumberOfV1Learners;
        public decimal V1EarningsAmount { get; set; }
        public decimal V2EarningsAmount { get; set; }
        public decimal EarningsDifference => V2EarningsAmount - V1EarningsAmount;
    }
}
