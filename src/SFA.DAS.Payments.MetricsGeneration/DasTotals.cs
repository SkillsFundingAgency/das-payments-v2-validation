namespace SFA.DAS.Payments.MetricsGeneration
{
    internal class DasTotals

    {
        public decimal RpsThisMonth { get; set; }
        public decimal PaymentPriorThisMonth { get; set; }
        public decimal ExpectedPaymentAfterMonthEnd { get; set; }
        public decimal TotalPaymentsThisMonth { get; set; }
        public decimal TotalAct1 { get; set; }
        public decimal TotalAct2 { get; set; }
        public decimal TotalYtd { get; set; }
        public decimal HeldBackCompletionPayments { get; set; }
    }
}