using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Verification.Earnings.DTO;

namespace SFA.DAS.Payments.Verification.Earnings.Extensions
{
    public static class PaymentExtensions
    {
        public static List<ComparisonClass> ToComparisonClass(this IEnumerable<Payment> payments)
        {
            return payments.SelectMany(x => x.ToComparisonClass()).ToList();
        }

        public static List<ComparisonClass> ToComparisonClass(this Payment payment)
        {
            var result = new List<ComparisonClass>();
            result.Add(new ComparisonClass
            {
                Amount = payment.Amount,
                ContractType = payment.ContractType,
                DeliveryPeriod = payment.DeliveryPeriod,
                FundingLineType = payment.LearningAimFundingLineType,
                TransactionType = payment.TransactionType,
                Ukprn = payment.Ukprn,
                Uln = payment.LearnerUln,
            });
    
            return result;
        }
    }
}
