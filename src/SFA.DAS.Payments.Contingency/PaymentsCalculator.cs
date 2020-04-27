using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Contingency.DTO;

namespace SFA.DAS.Payments.Contingency
{
    public class PaymentsCalculator
    {
        public static List<CalculatedPayment> Generate(List<Earning> allEarnings, List<Payment> allPayments)
        {
            var results = new List<CalculatedPayment>();

            var groupedEarnings = allEarnings.ToLookup(x => new LearnerIdentifier(x));
            var groupedPayments = allPayments.ToLookup(x => new LearnerIdentifier(x));

            // Only looking at earnings as the ticket expressly says that we are not
            //  accounting for learners that are removed from the ILR
            foreach (var earning in groupedEarnings)
            {
                var earnings = groupedEarnings[earning.Key].ToList();

                if (!groupedPayments.Contains(earning.Key))
                {
                    results.Add(new CalculatedPayment(earning.Key, earnings));
                    continue;
                }

                // There is a payment, so we need to remove it
                var payments = groupedPayments[earning.Key].ToList();
                var onProgDifference = earnings.Sum(x => x.OneToThree) - 
                                       payments.Sum(x => x.OnProgPayments);
                var incentiveDifference = earning.Sum(x => x.Incentives) -
                                          payments.Sum(x => x.IncentivePayments);
                results.Add(new CalculatedPayment(earning.Key, onProgDifference, incentiveDifference));
            }

            return results;
        }
    }

    public class CalculatedPayment
    {
        public CalculatedPayment(LearnerIdentifier key, IList<Earning> earnings)
        {
            Uln = key.Uln;
            Ukprn = key.Ukprn;
            FundingLineType = key.FundingLineType;
            OnProgPayments = earnings.Sum(x => x.OneToThree);
            IncentivePayments = earnings.Sum(x => x.Incentives);
        }

        public CalculatedPayment(LearnerIdentifier key, decimal onProgAmount, decimal incentiveAmount)
        {
            Uln = key.Uln;
            Ukprn = key.Ukprn;
            FundingLineType = key.FundingLineType;
            OnProgPayments = onProgAmount;
            IncentivePayments = incentiveAmount;
            if (onProgAmount < -0.01m) Console.WriteLine($"Found negative payment for learner: {key}");
            if (incentiveAmount < -0.01m) Console.WriteLine($"Found negative payment for learner: {key}");
        }

        public long Uln { get; set; }
        public long Ukprn { get; set; }
        public string FundingLineType { get; set; }
        
        public decimal OnProgPayments { get; set; }
        public decimal IncentivePayments { get; set; }
        public decimal TotalAmount => OnProgPayments + IncentivePayments;
    }

    public class LearnerIdentifier : IEquatable<LearnerIdentifier>
    {
        public LearnerIdentifier(Payment payment)
        {
            Ukprn = payment.Ukprn;
            Uln = payment.Uln;
            FundingLineType = payment.FundingLineType;
        }

        public LearnerIdentifier(Earning earning)
        {
            Ukprn = earning.Ukprn;
            Uln = earning.Uln;
            FundingLineType = earning.FundingLineType;
        }

        public override string ToString()
        {
            return $"Ukprn: {Ukprn}, Uln: {Uln}";
        }

        public bool Equals(LearnerIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Ukprn == other.Ukprn && Uln == other.Uln && string.Equals(FundingLineType, other.FundingLineType, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is LearnerIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Ukprn.GetHashCode();
                hashCode = (hashCode * 397) ^ Uln.GetHashCode();
                hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(FundingLineType);
                return hashCode;
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

        public long Ukprn { get; set; }
        public long Uln { get; set; }
        public string FundingLineType { get; set; }
    }
}
