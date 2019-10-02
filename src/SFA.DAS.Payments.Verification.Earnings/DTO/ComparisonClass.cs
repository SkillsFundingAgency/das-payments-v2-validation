using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.Verification.Earnings.DTO
{
    public class ComparisonClass : IEquatable<ComparisonClass>
    {
        private sealed class ComparisonClassEqualityComparer : IEqualityComparer<ComparisonClass>
        {
            public bool Equals(ComparisonClass x, ComparisonClass y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Ukprn == y.Ukprn && x.Uln == y.Uln && string.Equals(x.FundingLineType, y.FundingLineType, StringComparison.OrdinalIgnoreCase) && x.ContractType == y.ContractType && x.DeliveryPeriod == y.DeliveryPeriod && x.TransactionType == y.TransactionType && x.Amount == y.Amount;
            }

            public int GetHashCode(ComparisonClass obj)
            {
                unchecked
                {
                    var hashCode = obj.Ukprn.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Uln.GetHashCode();
                    hashCode = (hashCode * 397) ^ (obj.FundingLineType != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FundingLineType) : 0);
                    hashCode = (hashCode * 397) ^ obj.ContractType;
                    hashCode = (hashCode * 397) ^ obj.DeliveryPeriod;
                    hashCode = (hashCode * 397) ^ obj.TransactionType;
                    hashCode = (hashCode * 397) ^ obj.Amount.GetHashCode();
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<ComparisonClass> ComparisonClassComparer { get; } = new ComparisonClassEqualityComparer();

        public bool Equals(ComparisonClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Ukprn == other.Ukprn && Uln == other.Uln && string.Equals(FundingLineType, other.FundingLineType, StringComparison.OrdinalIgnoreCase) && ContractType == other.ContractType && DeliveryPeriod == other.DeliveryPeriod && TransactionType == other.TransactionType && Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ComparisonClass) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Ukprn.GetHashCode();
                hashCode = (hashCode * 397) ^ Uln.GetHashCode();
                hashCode = (hashCode * 397) ^ (FundingLineType != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(FundingLineType) : 0);
                hashCode = (hashCode * 397) ^ ContractType;
                hashCode = (hashCode * 397) ^ DeliveryPeriod;
                hashCode = (hashCode * 397) ^ TransactionType;
                hashCode = (hashCode * 397) ^ Amount.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ComparisonClass left, ComparisonClass right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComparisonClass left, ComparisonClass right)
        {
            return !Equals(left, right);
        }

        public long Ukprn { get; set; }
        public long Uln { get; set; }
        public string FundingLineType { get; set; }
        public int ContractType { get; set; }
        public int DeliveryPeriod { get; set; }
        public int TransactionType { get; set; }
        public decimal Amount { get; set; }
    }
}
