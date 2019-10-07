﻿namespace SFA.DAS.Payments.Migration.Constants
{
    public static class V2Sql
    {
        public const string DeleteData = @"

                DELETE Payments2.ApprenticeshipPause
                DELETE Payments2.ApprenticeshipDuplicate
                DELETE Payments2.ApprenticeshipPriceEpisode
                DELETE Payments2.Apprenticeship
                DELETE Payments2.LevyAccount
            ";

        public const string IdentityInsertOn = @"
                SET IDENTITY_INSERT Payments2.ApprenticeshipPriceEpisode ON;
            ";

        public const string IdentityInsertOff = @"
                SET IDENTITY_INSERT Payments2.ApprenticeshipPriceEpisode OFF;
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
                SELECT R.EventId [RequiredPaymentEventId], P.*, E.LearningAimSequenceNumber, R.Amount [AmountDue]
                  FROM [Payments2].[Payment] P
                JOIN Payments2.FundingSourceEvent F
	                ON F.EventId = P.FundingSourceEventId
                JOIN Payments2.RequiredPaymentEvent R
	                ON R.EventId = F.RequiredPaymentEventId
                JOIN Payments2.EarningEvent E
	                ON E.EventId = P.EarningEventId
                WHERE P.AcademicYear = 1920
                    AND P.CollectionPeriod IN (2)
                ORDER BY P.Id
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY
                ";
    }
}
