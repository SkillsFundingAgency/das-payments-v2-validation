namespace SFA.DAS.Payments.MetricsGeneration
{
    internal class DasTotals

    {
        private decimal RpsThisMonth { get; set; }
        private decimal paymentPriorThisMonth { get; set; }
        private decimal expectedPaymentAfterMonthEnd { get; set; }
        private decimal TotalPaymentsThisMonth { get; set; }
        private decimal TotalAct1 { get; set; }
        private decimal TotalAct2 { get; set; }
        private decimal TotalYtd { get; set; }
        private decimal HeldBackCompletionPayments { get; set; }
    }
}