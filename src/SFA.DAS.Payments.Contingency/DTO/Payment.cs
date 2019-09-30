
namespace SFA.DAS.Payments.Contingency.DTO
{
    internal class Payment 
    {
        public long Uln { get; set; }
        public long Ukprn { get; set; }

        public byte ContractType { get; set; }
        public string FundingLineType { get; set; }
        public decimal Amount { get; set; }
        public int TransactionType { get; set; }
    }
}
