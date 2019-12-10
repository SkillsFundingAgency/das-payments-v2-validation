using System;

namespace SFA.DAS.Payments.ProviderPayments.Model.V1
{
    public enum TransferType
    {
        None = 0,
        Levy = 1,
    }

    public class LegacyAccountTransferModel
    {
        public long SendingAccountId { get; set; }
        public long ReceivingAccountId { get; set; }
        public Guid RequiredPaymentId { get; set; }
        public long CommitmentId { get; set; }
        public decimal Amount { get; set; }
        public TransferType TransferType { get; set; }
        public string CollectionPeriodName { get; set; }
        public int CollectionPeriodMonth { get; set; }
        public int CollectionPeriodYear { get; set; }
    }
}
