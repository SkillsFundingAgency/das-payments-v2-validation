namespace SFA.DAS.Payments.Verification.Constants
{
    internal interface IContainVerificationResults
    {
        VerificationResult VerificationResult { get; set; }
    }

    enum VerificationResult
    {
        V1Only = 1,
        V2Only = 2,
        Okay = 3,
    }
}
