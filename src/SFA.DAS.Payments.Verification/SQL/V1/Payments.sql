WITH RawPayments AS (
	SELECT 
	 CommitmentId,
	 AccountId, 
	 Uln [LearnerUln],
	 LearnRefNumber [LearnerReferenceNumber],
	 Ukprn,
	 IlrSubmissionDateTime,
	 PriceEpisodeIdentifier,
	 StandardCode [LearningAimStandardCode],
	 ProgrammeType [LearningAimProgrammeType],
	 FrameworkCode [LearningAimFrameworkCode],
	 PathwayCode [LearningAimPathwayCode],
	 ApprenticeshipContractType [ContractType],
	 R.CollectionPeriodName,
	 R.TransactionType,
	 SfaContributionPercentage,
	 FundingLineType [LearningAimFundingLineType],
	 LearnAimRef [LearningAimReference],
	 CASE WHEN P.DeliveryMonth < 8 THEN P.DeliveryMonth + 5 ELSE P.DeliveryMonth - 7 END [DeliveryPeriod],
	 SUBSTRING(R.CollectionPeriodName, 1, 4) [AcademicYear],
	 --P.DeliveryYear,
	 FundingSource,
	 Amount,
	 CAST(SUBSTRING(R.CollectionPeriodName, 7, 2) AS INT) [CollectionPeriod]
	FROM [DAS_PeriodEnd].Payments.Payments P
	JOIN [DAS_PeriodEnd].PaymentsDue.RequiredPayments R
	 ON P.RequiredPaymentId = R.Id

	WHERE Uln IN (SELECT ULN FROM ##V1Learners)
	AND R.CollectionPeriodName LIKE '1819-R%'
)

SELECT * FROM RawPayments
WHERE DeliveryPeriod IN @periods
AND CollectionPeriod IN @periods
AND (
	(@restrictUkprns = 1 AND Ukprn IN @ukprns)
	OR
	(@restrictUkprns = 0)
)

Order by UKPRN, learneruln, AcademicYear, CollectionPeriodName, DeliveryPeriod, TransactionType, FundingSource