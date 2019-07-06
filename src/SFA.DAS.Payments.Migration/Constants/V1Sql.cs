namespace SFA.DAS.Payments.Migration.Constants
{
    public static class V1Sql
    {
        public const string Commitments = @"
                WITH CommitmentsToReturn AS (
	                SELECT MAX(CAST(SUBSTRING(VersionId, 0, CHARINDEX('-', VersionId)) AS INT)) [Event ID], CommitmentId 
	                FROM [DAS_CommitmentsReferenceData].[dbo].[DasCommitmentsHistory]
	                WHERE EventDateTime < @inputDate
	                GROUP BY CommitmentId
                )
                SELECT TOP(10) * FROM DasCommitmentsHistory
                WHERE CAST(SUBSTRING(VersionId, 0, CHARINDEX('-', VersionId)) AS INT) IN (
	                SELECT [Event ID] FROM CommitmentsToReturn
                )
                ORDER BY CommitmentId
            ";

        public const string Accounts = @"
             SELECT [AccountId]
                  ,[AccountHashId]
                  ,[AccountName]
                  ,[Balance]
                  ,[VersionId]
                  ,[IsLevyPayer]
                  ,[TransferAllowance]
              FROM [DasAccounts]
            ";
    }
}
