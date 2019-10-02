using System;

namespace SFA.DAS.Payments.Verification.Earnings.DTO
{
    public class Earning
    {
        public long LearnerUln { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long Ukprn { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public DateTime EpisodeStartDate { get; set; }
        public DateTime EpisodeEffectiveTnpStartDate { get; set; }
        public int DeliveryPeriod { get; set; }
        public int? LearningAimProgrammeType { get; set; }
        public int? LearningAimFrameworkCode { get; set; }
        public int? LearningAimPathwayCode { get; set; }
        public int? LearningAimStandardCode { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public string LearningAimFundingLineType { get; set; }
        public string LearningAimReference { get; set; }
        public DateTime LearningStartDate { get; set; }
        public decimal TransactionType01 { get; set; }
        public decimal TransactionType02 { get; set; }
        public decimal TransactionType03 { get; set; }
        public decimal TransactionType04 { get; set; }
        public decimal TransactionType05 { get; set; }
        public decimal TransactionType06 { get; set; }
        public decimal TransactionType07 { get; set; }
        public decimal TransactionType08 { get; set; }
        public decimal TransactionType09 { get; set; }
        public decimal TransactionType10 { get; set; }
        public decimal TransactionType11 { get; set; }
        public decimal TransactionType12 { get; set; }
        public decimal TransactionType13 { get; set; }
        public decimal TransactionType14 { get; set; }
        public decimal TransactionType15 { get; set; }
        public decimal TransactionType16 { get; set; }
        public int ContractType { get; set; }
        public DateTime? FirstIncentiveCensusDate { get; set; }
        public DateTime? SecondIncentiveCensusDate { get; set; }
        public DateTime? LearnerAdditionalPaymentsDate { get; set; }
        public decimal AgreedPrice { get; set; }
        public DateTime? EndDate { get; set; }
        public int CumulativePmrs { get; set; }
        public int ExemptionCodeForCompletionHoldback { get; set; }
        
        protected bool Equals(Earning other)
        {
            return string.Equals(LearnerReferenceNumber, other.LearnerReferenceNumber) && 
                   Ukprn == other.Ukprn && 
                   string.Equals(PriceEpisodeIdentifier, other.PriceEpisodeIdentifier) && 
                   DeliveryPeriod == other.DeliveryPeriod && 
                   LearningAimProgrammeType == other.LearningAimProgrammeType && 
                   LearningAimFrameworkCode == other.LearningAimFrameworkCode &&
                   LearningAimPathwayCode == other.LearningAimPathwayCode && 
                   LearningAimStandardCode == other.LearningAimStandardCode && 
                   SfaContributionPercentage == other.SfaContributionPercentage && 
                   string.Equals(LearningAimFundingLineType, other.LearningAimFundingLineType) && 
                   string.Equals(LearningAimReference, other.LearningAimReference) && 
                   TransactionType01 == other.TransactionType01 && 
                   TransactionType02 == other.TransactionType02 && 
                   TransactionType03 == other.TransactionType03 && 
                   TransactionType04 == other.TransactionType04 && 
                   TransactionType05 == other.TransactionType05 && 
                   TransactionType06 == other.TransactionType06 && 
                   TransactionType07 == other.TransactionType07 && 
                   TransactionType08 == other.TransactionType08 && 
                   TransactionType09 == other.TransactionType09 && 
                   TransactionType10 == other.TransactionType10 && 
                   TransactionType11 == other.TransactionType11 && 
                   TransactionType12 == other.TransactionType12 && 
                   TransactionType13 == other.TransactionType13 && 
                   TransactionType14 == other.TransactionType14 && 
                   TransactionType15 == other.TransactionType15 && 
                   TransactionType16 == other.TransactionType16 && 
                   ContractType == other.ContractType && 
                   AgreedPrice == other.AgreedPrice;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Earning) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (LearnerReferenceNumber != null ? LearnerReferenceNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Ukprn.GetHashCode();
                hashCode = (hashCode * 397) ^ (PriceEpisodeIdentifier != null ? PriceEpisodeIdentifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DeliveryPeriod;
                hashCode = (hashCode * 397) ^ LearningAimProgrammeType.GetHashCode();
                hashCode = (hashCode * 397) ^ LearningAimFrameworkCode.GetHashCode();
                hashCode = (hashCode * 397) ^ LearningAimPathwayCode.GetHashCode();
                hashCode = (hashCode * 397) ^ LearningAimStandardCode.GetHashCode();
                hashCode = (hashCode * 397) ^ SfaContributionPercentage.GetHashCode();
                hashCode = (hashCode * 397) ^ (LearningAimFundingLineType != null ? LearningAimFundingLineType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LearningAimReference != null ? LearningAimReference.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TransactionType01.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType02.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType03.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType04.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType05.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType06.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType07.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType08.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType09.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType10.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType11.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType12.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType13.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType14.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType15.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionType16.GetHashCode();
                hashCode = (hashCode * 397) ^ ContractType;
                hashCode = (hashCode * 397) ^ AgreedPrice.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Earning left, Earning right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Earning left, Earning right)
        {
            return !Equals(left, right);
        }
    }
}
