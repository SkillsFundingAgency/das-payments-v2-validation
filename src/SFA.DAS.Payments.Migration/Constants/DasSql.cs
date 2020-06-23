namespace SFA.DAS.Payments.Migration.Constants
{
	public static class DasSql
    {
	    public const string Commitments = @"
            SELECT  A.Id [ApprenticeshipId], 
                A.Uln, 
                ProviderId [Ukprn], 
                EmployerAccountId [AccountId], 
                StartDate,
                EndDate,
                H.Cost [AgreedCost],
                PaymentStatus,
                PaymentOrder [Priority],
                H.FromDate [EffectiveFromDate],
                H.ToDate [EffectiveToDate],
                TransferSenderId [TransferSendingEmployerAccountId], 
                TransferApprovalActionedOn [TransferApprovalDate],
                PauseDate [PausedOnDate],
                StopDate [WithdrawnOnDate],
                ALE.[Name] AS LegalEntityName, 
                TrainingType,
                TrainingCode,  
	            ISNULL(ApprenticeshipEmployerTypeOnApproval, 1) [ApprenticeshipEmployerType], 
	            ALE.PublicHashedId AS AccountLegalEntityPublicHashedId,
                A.AgreedOn [AgreedOnDate],
                A.CreatedOn [CreatedDate]
            FROM [dbo].[Apprenticeship] A
            JOIN PriceHistory H
	            ON A.Id = H.ApprenticeshipId
            JOIN Commitment C
	            ON C.Id = A.CommitmentId
	        LEFT JOIN [AccountLegalEntities] AS ALE
	        	ON c.AccountLegalEntityId = ALE.Id
            WHERE A.PaymentStatus > 0
            ";
    }
}
