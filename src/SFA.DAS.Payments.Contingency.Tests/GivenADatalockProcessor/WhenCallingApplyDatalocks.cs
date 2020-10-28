using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.Contingency.DTO;

namespace SFA.DAS.Payments.Contingency.UnitTests.GivenADatalockProcessor
{
    [TestFixture]
    public class WhenCallingApplyDatalocks
    {
        [TestFixture]
        public class AndThereAreNoApprenticeships
        {
            [AutoData]
            public void ThenThereShouldBeNoResults(List<Earning> earnings)
            {
                var apprenticeships = new List<BasicV2Apprenticeship>();
                var actual = DatalockCalculator.ApplyDatalocks(earnings, apprenticeships);
                actual.Should().BeEmpty();
            }
        }

        [TestFixture]
        public class AndThereAreMatchingApprenticeships
        {
            [AutoData]
            public void AndAllEarningsHaveAMatchingApprenticeship_ThenAllEarningsShouldBePresent(List<Earning> earnings)
            {
                var apprenticeships = new List<BasicV2Apprenticeship>(earnings.Select(x => new BasicV2Apprenticeship
                {
                    Ukprn = x.Ukprn,
                    Uln = x.Uln,
                }));
                var actual = DatalockCalculator.ApplyDatalocks(earnings, apprenticeships);
                actual.Should().HaveCount(3);
            }

            [AutoData]
            public void AndSomeEarningsHaveAMatchingApprenticeship_ThenThoseEarningsShouldBePresent(List<Earning> earnings)
            {
                var apprenticeships = new List<BasicV2Apprenticeship>
                {
                    new BasicV2Apprenticeship
                    {
                        Ukprn = earnings[0].Ukprn,
                        Uln = earnings[0].Uln,
                    }
                };
                var actual = DatalockCalculator.ApplyDatalocks(earnings, apprenticeships);
                actual.Should().HaveCount(1);
            }
        }
    }
}
