
namespace SFA.DAS.Payments.Contingency.DTO
{
    public class Payment 
    {
        public long Uln { get; set; }
        public long Ukprn { get; set; }
        public byte ContractType { get; set; }
        public string FundingLineType { get; set; }
        
        public decimal OnProgPayments { get; set; }
        public decimal IncentivePayments { get; set; }
        public decimal Amount => OnProgPayments + IncentivePayments;
    }
}
