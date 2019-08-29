using System;

namespace SFA.DAS.Payments.Contingency.DTO
{
    public class Datalock : IEquatable<Datalock>
    {
        public long Ukprn { get; set; }
        public long Uln { get; set; }
        public string LearnRefNumber { get; set; }
        public int AimSeqNumber { get; set; }

        public bool Equals(Datalock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Ukprn == other.Ukprn &&
                   string.Equals(LearnRefNumber, other.LearnRefNumber, StringComparison.OrdinalIgnoreCase) &&
                   AimSeqNumber == other.AimSeqNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Datalock)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Ukprn.GetHashCode();
                hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(LearnRefNumber);
                hashCode = (hashCode * 397) ^ AimSeqNumber;
                return hashCode;
            }
        }

        public static bool operator ==(Datalock left, Datalock right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Datalock left, Datalock right)
        {
            return !Equals(left, right);
        }
    }
}
