using System.Collections.Generic;
using System.Linq;
using FastMember;
using SFA.DAS.Payments.Verification.Earnings.DTO;

namespace SFA.DAS.Payments.Verification.Earnings.Extensions
{
    public static class EarningExtensions
    {
        private static readonly TypeAccessor EarningsAccessor = TypeAccessor.Create(typeof(Earning));

        public static List<ComparisonClass> ToComparisonClass(this IEnumerable<Earning> earnings)
        {
            return earnings.SelectMany(x => x.ToComparisonClass()).ToList();
        }

        public static List<ComparisonClass> ToComparisonClass(this Earning earning)
        {
            var result = new List<ComparisonClass>();
            for (var i = 1; i <= 16; i++)
            {
                var amount = (decimal) EarningsAccessor[earning, $"TransactionType{i:D2}"];
                if (amount != 0)
                {
                    result.Add(new ComparisonClass
                    {
                        Amount = amount,
                        ContractType = earning.ContractType,
                        DeliveryPeriod = earning.DeliveryPeriod,
                        FundingLineType = earning.LearningAimFundingLineType,
                        TransactionType = i,
                        Ukprn = earning.Ukprn,
                        Uln = earning.LearnerUln,
                    });
                }
            }

            return result;
        }
    }
}
