using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.Contingency.DTO;

namespace SFA.DAS.Payments.Contingency.UnitTests.GivenAPaymentsCalculator
{
    [TestFixture]
    public class WhenCallingGenerate
    {
        [TestFixture]
        public class WithNoPayments
        {
            [AutoData]
            public void ThenTheResultsShouldMatchTheInputEarnings(
                List<Earning> earnings)
            {
                var actual = PaymentsCalculator.Generate(earnings, new List<Payment>());

                actual.Sum(x => x.TotalAmount).Should()
                    .Be(earnings.Sum(x => x.AllTransactions));
            }
        }

        [TestFixture]
        public class WithPayments
        {
            [AutoData]
            public void ThenTheActualAmountShouldBeEarningsLessPayments(
                List<Earning> earnings,
                List<Payment> payments,
                long ukprn, 
                long uln,
                string fundingLineType)
            {
                earnings.ForEach(x =>
                {
                    x.Ukprn = ukprn;
                    x.Uln = uln;
                    x.FundingLineType = fundingLineType;
                });

                payments.ForEach(x =>
                {
                    x.Ukprn = ukprn;
                    x.Uln = uln;
                    x.FundingLineType = fundingLineType;
                });

                var actual = PaymentsCalculator.Generate(earnings, payments);
                var totalEarnings = earnings.Sum(x => x.AllTransactions);
                var totalPayments = payments.Sum(x => x.Amount);
                var expected = totalEarnings - totalPayments;

                actual.Sum(x => x.TotalAmount).Should().Be(expected);
            }

            [AutoData]
            public void ThenTheActualAmountShouldIgnorePaymetnsWithoutEarnings(
                List<Earning> earnings,
                List<Payment> payments,
                long ukprn, 
                long uln,
                string fundingLineType)
            {
                earnings.ForEach(x =>
                {
                    x.Ukprn = ukprn;
                    x.Uln = uln;
                    x.FundingLineType = fundingLineType;
                });

                var actual = PaymentsCalculator.Generate(earnings, payments);
                var expected = earnings.Sum(x => x.AllTransactions);
                
                actual.Sum(x => x.TotalAmount).Should().Be(expected);
            }
        }
    }
}
