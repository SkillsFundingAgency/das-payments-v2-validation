using System;
using System.Collections.Generic;
using SFA.DAS.Payments.Verification.Constants;

namespace SFA.DAS.Payments.Verification.DTO
{
    internal class RequiredPayment : IContainLearnerDetails, IContainVerificationResults
    {
        private static readonly List<int> MathsEnglishTransactionTypes = new List<int> { 13, 14, 15 };

        public long LearnerUln { get; set; }
        public string PriceEpisodeIdentifier { get; set; }

        private string PriceEpisodeIdentifierForComparison =>
            MathsEnglishTransactionTypes.Contains(TransactionType) ? "" : PriceEpisodeIdentifier;

        public long Ukprn { get; set; }
        public int ContractType { get; set; }
        public int TransactionType { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public decimal Amount { get; set; }

        private decimal AmountToCompare => Math.Round(Amount, 4);

        public int CollectionPeriod { get; set; }
        public int AcademicYear { get; set; }
        public int DeliveryPeriod { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public string LearningAimReference { get; set; }
        public int? LearningAimProgrammeType { get; set; }
        public int? LearningAimStandardCode { get; set; }
        public int? LearningAimFrameworkCode { get; set; }
        public int? LearningAimPathwayCode { get; set; }

        private int StandardCode => LearningAimStandardCode ?? 0;
        private int ProgrammeType => LearningAimProgrammeType ?? 0;
        private int FrameworkCode => LearningAimFrameworkCode ?? 0;
        private int PathwayCode => LearningAimPathwayCode ?? 0;


        public string LearningAimFundingLineType { get; set; }
        public long? AccountId { get; set; }
        public VerificationResult VerificationResult { get; set; }
        public int JobId { get; set; }

        public bool Equals(RequiredPayment other)
        {
            return LearnerUln == other.LearnerUln &&
                   string.Equals(PriceEpisodeIdentifierForComparison, other.PriceEpisodeIdentifierForComparison) && 
                   Ukprn == other.Ukprn &&
                   ContractType == other.ContractType && 
                   TransactionType == other.TransactionType &&
                   SfaContributionPercentage == other.SfaContributionPercentage &&
                   AmountToCompare == other.AmountToCompare &&
                   CollectionPeriod == other.CollectionPeriod && 
                   AcademicYear == other.AcademicYear &&
                   DeliveryPeriod == other.DeliveryPeriod &&
                   string.Equals(LearnerReferenceNumber, other.LearnerReferenceNumber) &&
                   string.Equals(LearningAimReference, other.LearningAimReference) &&
                   ProgrammeType == other.ProgrammeType &&
                   StandardCode == other.StandardCode &&
                   FrameworkCode == other.FrameworkCode &&
                   PathwayCode == other.PathwayCode &&
                   string.Equals(LearningAimFundingLineType, other.LearningAimFundingLineType) &&
                   AccountId == other.AccountId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RequiredPayment other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LearnerUln.GetHashCode();
                hashCode = (hashCode * 397) ^ (PriceEpisodeIdentifierForComparison != null ? PriceEpisodeIdentifierForComparison.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Ukprn.GetHashCode();
                hashCode = (hashCode * 397) ^ ContractType;
                hashCode = (hashCode * 397) ^ TransactionType;
                hashCode = (hashCode * 397) ^ SfaContributionPercentage.GetHashCode();
                hashCode = (hashCode * 397) ^ AmountToCompare.GetHashCode();
                hashCode = (hashCode * 397) ^ CollectionPeriod;
                hashCode = (hashCode * 397) ^ AcademicYear;
                hashCode = (hashCode * 397) ^ DeliveryPeriod;
                hashCode = (hashCode * 397) ^ (LearnerReferenceNumber != null ? LearnerReferenceNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LearningAimReference != null ? LearningAimReference.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ProgrammeType.GetHashCode();
                hashCode = (hashCode * 397) ^ StandardCode.GetHashCode();
                hashCode = (hashCode * 397) ^ FrameworkCode.GetHashCode();
                hashCode = (hashCode * 397) ^ PathwayCode.GetHashCode();
                hashCode = (hashCode * 397) ^ (LearningAimFundingLineType != null ? LearningAimFundingLineType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ AccountId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RequiredPayment left, RequiredPayment right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RequiredPayment left, RequiredPayment right)
        {
            return !left.Equals(right);
        }
    }
}
