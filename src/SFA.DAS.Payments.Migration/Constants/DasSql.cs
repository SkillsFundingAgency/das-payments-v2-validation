namespace SFA.DAS.Payments.Migration.Constants
{
    static class DasSql
    {
        public static string Commitments = @"
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
                LegalEntityName, 
                TrainingType,
                TrainingCode,  
	            ISNULL(ApprenticeshipEmployerTypeOnApproval, 1) [ApprenticeshipEmployerType], 
	            AccountLegalEntityPublicHashedId,
                A.AgreedOn [AgreedOnDate],
                A.CreatedOn [CreatedDate]
	            
            FROM [dbo].[Apprenticeship] A
            JOIN PriceHistory H
	            ON A.Id = H.ApprenticeshipId
            JOIN Commitment C
	            ON C.Id = A.CommitmentId
            WHERE A.PaymentStatus > 0
            ";
    }
}
