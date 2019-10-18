namespace SFA.DAS.Payments.Migration.Constants
{
    public static class CommitmentsSql
    {
        public static string NonLevyCommitments = @"
            SELECT A.Id, ApprenticeshipEmployerTypeOnApproval
              FROM [dbo].[Commitment] C
              JOIN Apprenticeship A
	            ON C.Id = A.CommitmentId
              WHERE ApprenticeshipEmployerTypeOnApproval = 0
        ";
    }
}
