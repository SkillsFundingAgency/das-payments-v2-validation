namespace SFA.DAS.Payments.FM36Tool.ApprenticeshipSetup
{
    public class LevyAccountModel
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public bool IsLevyPayer { get; set; }
        public decimal TransferAllowance { get; set; }
    }
}
