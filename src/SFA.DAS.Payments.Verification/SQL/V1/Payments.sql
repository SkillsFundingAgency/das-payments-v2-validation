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
	 Amount
	FROM [DAS_PeriodEnd].Payments.Payments P
	JOIN [DAS_PeriodEnd].PaymentsDue.RequiredPayments R
	 ON P.RequiredPaymentId = R.Id

	WHERE Uln IN (SELECT ULN FROM ##Learners)
	AND R.CollectionPeriodName LIKE '1819-R%'
)

SELECT * FROM RawPayments
WHERE DeliveryPeriod IN @periods

Order by UKPRN, learneruln, AcademicYear, CollectionPeriodName, DeliveryPeriod, TransactionType, FundingSource