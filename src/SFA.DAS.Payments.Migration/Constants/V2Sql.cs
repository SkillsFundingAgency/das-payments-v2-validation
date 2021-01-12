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
;WITH FundingSourceEvents AS (
	SELECT DISTINCT EventId, RequiredPaymentEventId
	FROM Payments2.FundingSourceEvent
	WHERE AcademicYear = @academicYear 
	AND CollectionPeriod = @collectionPeriod
)

SELECT * INTO #FundingSourceEvents
FROM FundingSourceEvents

;WITH RequiredPaymentEvents AS (
	SELECT MAX(Amount) [Amount], EventId
	FROM Payments2.RequiredPaymentEvent
    WHERE AcademicYear = @academicYear
    AND CollectionPeriod = @collectionPeriod
	GROUP BY EventId
)

SELECT * INTO #RequiredPaymentEvents
FROM RequiredPaymentEvents

;WITH MigrationPayments AS (
	SELECT R.EventId [RequiredPaymentEventId], P.Id, P.EventId, P.EarningEventId, P.FundingSourceEventId,
		P.EventTime, P.JobId, P.DeliveryPeriod, P.CollectionPeriod, P.AcademicYear, P.Ukprn,
		P.LearnerReferenceNumber, P.LearnerUln, P.PriceEpisodeIdentifier, P.Amount, P.LearningAimReference,
		P.LearningAimProgrammeType, P.LearningAimStandardCode, P.LearningAimFrameworkCode, 
		P.LearningAimPathwayCode, P.LearningAimFundingLineType, P.ContractType, P.TransactionType,
		P.FundingSource, P.IlrSubmissionDateTime, P.SfaContributionPercentage, P.AgreementId,
		P.AccountId, P.TransferSenderAccountId, P.CreationDate, P.EarningsStartDate, P.EarningsPlannedEndDate,
		P.EarningsActualEndDate, P.EarningsCompletionStatus, P.EarningsCompletionAmount, P.EarningsInstalmentAmount,
		P.EarningsNumberOfInstalments, P.LearningStartDate, P.ApprenticeshipId, P.ApprenticeshipPriceEpisodeId,
		P.ApprenticeshipEmployerType, P.ReportingAimFundingLineType, P.NonPaymentReason,  
		ISNULL(E.LearningAimSequenceNumber, 0) [LearningAimSequenceNumber], R.Amount [AmountDue]
	FROM [Payments2].[Payment] P
	JOIN #FundingSourceEvents F
		ON F.EventId = P.FundingSourceEventId
	JOIN #RequiredPaymentEvents R
		ON R.EventId = F.RequiredPaymentEventId
	LEFT JOIN Payments2.EarningEvent E
		ON E.EventId = P.EarningEventId
	WHERE P.AcademicYear = @academicYear
	AND P.CollectionPeriod = @collectionPeriod
)

SELECT *
FROM MigrationPayments
ORDER BY RequiredPaymentEventId, Id
OFFSET @offset ROWS
FETCH NEXT @pageSize ROWS ONLY
";

        public const string PreviousEarnings = @"
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
                    AND P.CollectionPeriod <= @collectionPeriod
                ORDER BY R.EventId
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY
                ";

        public const string PaymentsAndEarningsForFailedTransfers = @"
                SELECT R2.EventId [RequiredPaymentEventId], P.Id, P.EventId, P.EarningEventId, P.FundingSourceEventId,
		P.EventTime, P.JobId, P.DeliveryPeriod, P.CollectionPeriod, P.AcademicYear, P.Ukprn,
		P.LearnerReferenceNumber, P.LearnerUln, P.PriceEpisodeIdentifier, P.Amount, P.LearningAimReference,
		P.LearningAimProgrammeType, P.LearningAimStandardCode, P.LearningAimFrameworkCode, 
		P.LearningAimPathwayCode, P.LearningAimFundingLineType, P.ContractType, P.TransactionType,
		P.FundingSource, P.IlrSubmissionDateTime, P.SfaContributionPercentage, P.AgreementId,
		P.AccountId, P.TransferSenderAccountId, P.CreationDate, P.EarningsStartDate, P.EarningsPlannedEndDate,
		P.EarningsActualEndDate, P.EarningsCompletionStatus, P.EarningsCompletionAmount, P.EarningsInstalmentAmount,
		P.EarningsNumberOfInstalments, P.LearningStartDate, P.ApprenticeshipId, P.ApprenticeshipPriceEpisodeId,
		P.ApprenticeshipEmployerType, P.ReportingAimFundingLineType, P.NonPaymentReason, 
	                ISNULL(E.LearningAimSequenceNumber, 0) [LearningAimSequenceNumber], R2.Amount [AmountDue]
                FROM [Payments2].[Payment] P 
                LEFT JOIN Payments2.FundingSourceEvent F 
	                ON F.EventId = P.FundingSourceEventId
                LEFT JOIN Payments2.EarningEvent E 
	                ON E.EventId = P.EarningEventId
                LEFT JOIN Payments2.RequiredPaymentEvent R 
    	            ON R.EventId = F.RequiredPaymentEventId
                LEFT JOIN Payments2.RequiredPaymentEvent R2
	                ON R2.EarningEventId = E.EventId
	                AND R2.LearnerUln = P.LearnerUln
	                AND R2.TransactionType = P.TransactionType
                    AND R2.DeliveryPeriod = P.DeliveryPeriod
                WHERE P.AcademicYear = @academicYear
                    AND P.CollectionPeriod = @collectionPeriod
	                AND R.EventId IS NULL
                ORDER BY R2.EventId, P.Id
            ";


    }
}
