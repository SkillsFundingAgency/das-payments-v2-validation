using SFA.DAS.Payments.Verification.Constants;

namespace SFA.DAS.Payments.Verification.DTO
{
    internal interface IContainVerificationResults
    {
        VerificationResult VerificationResult { get; set; }
        int JobId { get; set; }
    }
}