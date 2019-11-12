namespace SFA.DAS.Payments.Migration.Constants
{
    public static class V2Sql
    {
        public const string DeleteCommitments = @"

                DELETE Payments2.ApprenticeshipPause
                DELETE Payments2.ApprenticeshipDuplicate
                DELETE Payments2.ApprenticeshipPriceEpisode
                DELETE Payments2.Apprenticeship
            ";

        public const string DeleteEasPayments = @"
                DELETE Payments2.ProviderAdjustmentPayments
            ";

        public const string IdentityInsertOn = @"
                SET IDENTITY_INSERT Payments2.ApprenticeshipPriceEpisode ON;
            ";

        public const string IdentityInsertOff = @"
                SET IDENTITY_INSERT Payments2.ApprenticeshipPriceEpisode OFF;
            ";

        public const string DeletePayments = @"
                DELETE 
                FROM Payments2.Payment
                WHERE AcademicYear = @academicYear
                AND CollectionPeriod = @collectionPeriod
            ";

        public const string UpdateLevyPayerFlag = @"
                UPDATE Payments2.Apprenticeship
                SET IsLevyPayer = 0
                WHERE AccountId IN @accountIds
            ";

        public const string DeleteAccounts = @"
                DELETE Payments2.LevyAccount
            ";

        public const string PaymentsAndEarnings = @"
                SELECT R.EventId [RequiredPaymentEventId], P.*, ISNULL(E.LearningAimSequenceNumber, 0), 
                    R.Amount [AmountDue]
                FROM [Payments2].[Payment] P
                JOIN Payments2.FundingSourceEvent F
	                ON F.EventId = P.FundingSourceEventId
                JOIN Payments2.RequiredPaymentEvent R
	                ON R.EventId = F.RequiredPaymentEventId
                LEFT JOIN Payments2.EarningEvent E
	                ON E.EventId = P.EarningEventId
                WHERE P.AcademicYear = 1920
                    AND P.CollectionPeriod IN (3)
                ORDER BY R.EventId
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY
                ";
    }
}
