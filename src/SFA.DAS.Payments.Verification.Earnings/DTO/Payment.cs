using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.Verification.Earnings.DTO
{
    public class Payment 
    {
        private static readonly List<int> MathsEnglishTransactionTypes = new List<int>{13,14,15};
        private static readonly List<int> TransactionTypesToTestSfaContribution = new List<int> { 1, 2, 3 };

        public long LearnerUln { get; set; }
        public long? CommitmentId { get; set; }
        public long? AccountId { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long Ukprn { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public string PriceEpisodeIdentifier { get; set; }

        private string PriceEpisodeIdentifierForComparison =>
            MathsEnglishTransactionTypes.Contains(TransactionType) ? "" : PriceEpisodeIdentifier;

        public int? LearningAimStandardCode { get; set; }
        public int? LearningAimProgrammeType { get; set; }
        public int? LearningAimFrameworkCode { get; set; }
        public int? LearningAimPathwayCode { get; set; }

        private int StandardCode => LearningAimStandardCode ?? 0;
        private int ProgrammeType => LearningAimProgrammeType ?? 0;
        private int FrameworkCode => LearningAimFrameworkCode ?? 0;
        private int PathwayCode => LearningAimPathwayCode ?? 0;


        public int ContractType { get; set; }
        public string LearningAimReference { get; set; }
        public string CollectionPeriodName { get; set; }
        public int TransactionType { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        private decimal SfaContributionPercentageToCompare =>
            TransactionTypesToTestSfaContribution.Contains(TransactionType) ? SfaContributionPercentage : 0;
        
        public string LearningAimFundingLineType { get; set; }
        public int DeliveryPeriod { get; set; }
        public int AcademicYear { get; set; }
        public int FundingSource { get; set; }
        public decimal Amount { get; set; }

        private decimal AmountToCompare => Math.Round(Amount, 5);

        
        public bool Equals(Payment other)
        {
            return CommitmentId == other.CommitmentId && 
                   AccountId == other.AccountId &&
                   string.Equals(LearnerReferenceNumber, other.LearnerReferenceNumber) && 
                   Ukprn == other.Ukprn &&
                   string.Equals(PriceEpisodeIdentifierForComparison, other.PriceEpisodeIdentifierForComparison) &&
                   StandardCode == other.StandardCode &&
                   ProgrammeType == other.ProgrammeType &&
                   FrameworkCode == other.FrameworkCode &&
                   PathwayCode == other.PathwayCode && 
                   ContractType == other.ContractType &&
                   string.Equals(LearningAimReference, other.LearningAimReference) &&
                   string.Equals(CollectionPeriodName, other.CollectionPeriodName) &&
                   TransactionType == other.TransactionType &&
                   SfaContributionPercentageToCompare == other.SfaContributionPercentageToCompare &&
                   string.Equals(LearningAimFundingLineType, other.LearningAimFundingLineType) &&
                   DeliveryPeriod == other.DeliveryPeriod && 
                   AcademicYear == other.AcademicYear &&
                   FundingSource == other.FundingSource &&
                   AmountToCompare == other.AmountToCompare;
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
                hashCode = (hashCode * 397) ^ (PriceEpisodeIdentifierForComparison != null ? PriceEpisodeIdentifierForComparison.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ StandardCode.GetHashCode();
                hashCode = (hashCode * 397) ^ ProgrammeType.GetHashCode();
                hashCode = (hashCode * 397) ^ FrameworkCode.GetHashCode();
                hashCode = (hashCode * 397) ^ PathwayCode.GetHashCode();
                hashCode = (hashCode * 397) ^ ContractType;
                hashCode = (hashCode * 397) ^ (LearningAimReference != null ? LearningAimReference.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CollectionPeriodName != null ? CollectionPeriodName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TransactionType;
                hashCode = (hashCode * 397) ^ SfaContributionPercentageToCompare.GetHashCode();
                hashCode = (hashCode * 397) ^ (LearningAimFundingLineType != null ? LearningAimFundingLineType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DeliveryPeriod;
                hashCode = (hashCode * 397) ^ AcademicYear;
                hashCode = (hashCode * 397) ^ FundingSource;
                hashCode = (hashCode * 397) ^ AmountToCompare.GetHashCode();
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
