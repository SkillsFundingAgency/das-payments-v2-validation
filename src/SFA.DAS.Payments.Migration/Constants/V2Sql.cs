﻿namespace SFA.DAS.Payments.Migration.Constants
{
    public static class V2Sql
    {
        public const string DeleteData = @"
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
    }
}