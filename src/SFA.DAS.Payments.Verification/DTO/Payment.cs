using System;
using SFA.DAS.Payments.Verification.Constants;

namespace SFA.DAS.Payments.Verification.DTO
{
    internal class Payment : IContainLearnerDetails, IContainVerificationResults
    {
        public long LearnerUln { get; set; }
        public long? CommitmentId { get; set; }
        public long? AccountId { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long Ukprn { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public int? LearningAimStandardCode { get; set; }
        public int? LearningAimProgrammeType { get; set; }
        public int? LearningAimFrameworkCode { get; set; }
        public int? LearningAimPathwayCode { get; set; }
        public int ContractType { get; set; }
        public string LearningAimReference { get; set; }
        public string CollectionPeriodName { get; set; }
        public int TransactionType { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public string LearningAimFundingLineType { get; set; }
        public int DeliveryPeriod { get; set; }
        public int AcademicYear { get; set; }
        public int FundingSource { get; set; }
        public decimal Amount { get; set; }
        public VerificationResult VerificationResult { get; set; }

        public bool Equals(Payment other)
        {
            return CommitmentId == other.CommitmentId && 
                   AccountId == other.AccountId &&
                   string.Equals(LearnerReferenceNumber, other.LearnerReferenceNumber) && 
                   Ukprn == other.Ukprn &&
                   string.Equals(PriceEpisodeIdentifier, other.PriceEpisodeIdentifier) &&
                   LearningAimStandardCode == other.LearningAimStandardCode &&
                   LearningAimProgrammeType == other.LearningAimProgrammeType &&
                   LearningAimFrameworkCode == other.LearningAimFrameworkCode &&
                   LearningAimPathwayCode == other.LearningAimPathwayCode && 
                   ContractType == other.ContractType &&
                   string.Equals(LearningAimReference, other.LearningAimReference) &&
                   string.Equals(CollectionPeriodName, other.CollectionPeriodName) &&
                   TransactionType == other.TransactionType &&
                   SfaContributionPercentage == other.SfaContributionPercentage &&
                   string.Equals(LearningAimFundingLineType, other.LearningAimFundingLineType) &&
                   DeliveryPeriod == other.DeliveryPeriod && 
                   AcademicYear == other.AcademicYear &&
                   FundingSource == other.FundingSource && 
                   Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Payment other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CommitmentId.GetHashCode();
                hashCode = (hashCode * 397) ^ AccountId.GetHashCode();
                hashCode = (hashCode * 397) ^ (LearnerReferenceNumber != null ? LearnerReferenceNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Ukprn.GetHashCode();
                hashCode = (hashCode * 397) ^ (PriceEpisodeIdentifier != null ? PriceEpisodeIdentifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LearningAimStandardCode.GetHashCode();
                hashCode = (hashCode * 397) ^ LearningAimProgrammeType.GetHashCode();
                hashCode = (hashCode * 397) ^ LearningAimFrameworkCode.GetHashCode();
                hashCode = (hashCode * 397) ^ LearningAimPathwayCode.GetHashCode();
                hashCode = (hashCode * 397) ^ ContractType;
                hashCode = (hashCode * 397) ^ (LearningAimReference != null ? LearningAimReference.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CollectionPeriodName != null ? CollectionPeriodName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TransactionType;
                hashCode = (hashCode * 397) ^ SfaContributionPercentage.GetHashCode();
                hashCode = (hashCode * 397) ^ (LearningAimFundingLineType != null ? LearningAimFundingLineType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DeliveryPeriod;
                hashCode = (hashCode * 397) ^ AcademicYear;
                hashCode = (hashCode * 397) ^ FundingSource;
                hashCode = (hashCode * 397) ^ Amount.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Payment left, Payment right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Payment left, Payment right)
        {
            return !left.Equals(right);
        }
    }
}
