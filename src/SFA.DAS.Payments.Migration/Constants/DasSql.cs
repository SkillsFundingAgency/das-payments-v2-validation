namespace SFA.DAS.Payments.Migration.Constants
{
    static class DasSql
    {
        public static string Commitments = @"
            SELECT  A.Id [CommitmentId], A.Uln, ProviderId [Ukprn], EmployerAccountId [AccountId], 
                StartDate, EndDate, H.Cost [AgreedCost], PaymentStatus, PaymentOrder [Priority],
                H.FromDate [EffectiveFromDate], H.ToDate [EffectiveToDate],
                TransferSenderId [TransferSendingEmployerAccountId], 
                TransferApprovalActionedOn [TransferApprovalDate], PauseDate [PausedOnDate],
                StopDate [WithdrawnOnDate], LegalEntityName, 
                TrainingType, TrainingCode,  
	            ApprenticeshipEmployerTypeOnApproval [ApprenticeshipEmployerType], 
	            AccountLegalEntityPublicHashedId
	            
            FROM [dbo].[Apprenticeship] A
            JOIN PriceHistory H
	            ON A.Id = H.ApprenticeshipId
            JOIN Commitment C
	            ON C.Id = A.CommitmentId
            WHERE A.PaymentStatus > 0
            ";
    }
}
