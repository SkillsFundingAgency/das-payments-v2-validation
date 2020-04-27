using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Contingency.DTO;

namespace SFA.DAS.Payments.Contingency
{
    public class DatalockCalculator
    {
        public static List<Earning> ApplyDatalocks(List<Earning> earnings, List<BasicV2Apprenticeship> apprenticeships)
        {
            // Remove all earnigns that don't have a matching commitment
            var groupedEarnings = earnings.ToLookup(x => new LearnerIdentifier(x));
            var apprenticeshipList = new HashSet<LearnerIdentifier>();
            foreach (var basicV2Apprenticeship in apprenticeships)
            {
                apprenticeshipList.Add(new LearnerIdentifier(basicV2Apprenticeship));
            }

            var results = new List<Earning>();

            foreach (var groupedEarning in groupedEarnings)
            {
                if (apprenticeshipList.Contains(groupedEarning.Key))
                {
                    results.AddRange(groupedEarning);
                }
            }

            return results;
        }

        public sealed class LearnerIdentifier : IEquatable<LearnerIdentifier>
        {
            public LearnerIdentifier(Earning earning)
            {
                Ukprn = earning.Ukprn;
                Uln = earning.Uln;
            }

            public LearnerIdentifier(BasicV2Apprenticeship apprenticeship)
            {
                Ukprn = apprenticeship.Ukprn;
                Uln = apprenticeship.Uln;
            }

            public long Uln { get; }
            public long Ukprn { get; }

            public bool Equals(LearnerIdentifier other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Uln == other.Uln && Ukprn == other.Ukprn;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((LearnerIdentifier) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Uln.GetHashCode() * 397) ^ Ukprn.GetHashCode();
                }
            }

            public static bool operator ==(LearnerIdentifier left, LearnerIdentifier right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(LearnerIdentifier left, LearnerIdentifier right)
            {
                return !Equals(left, right);
            }
        }
    }
}
