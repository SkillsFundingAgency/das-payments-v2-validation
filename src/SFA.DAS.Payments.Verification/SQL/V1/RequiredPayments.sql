WITH RawPayments AS (
	SELECT 
		[PriceEpisodeIdentifier]
		  ,[Ukprn]
		  ,ApprenticeshipContractType [ContractType]
		  ,[TransactionType]
		  ,[SfaContributionPercentage]
		  ,AmountDue [Amount]
		  ,SUBSTRING(CollectionPeriodName, 7, 2) [CollectionPeriod]
		  ,SUBSTRING(CollectionPeriodName, 1, 4) [AcademicYear]
		  ,CASE WHEN DeliveryMonth < 8 THEN DeliveryMonth + 5 ELSE DeliveryMonth - 7 END [DeliveryPeriod]
		  ,LearnRefNumber [LearnerReferenceNumber]
		  ,Uln [LearnerUln]
		  ,LearnAimRef [LearningAimReference]
		  ,ProgrammeType [LearningAimProgrammeType]
		  ,StandardCode [LearningAimStandardCode]
		  ,FrameworkCode [LearningAimFrameworkCode]
		  ,PathwayCode [LearningAimPathwayCode]
		  ,FundingLineType [LearningAimFundingLineType]
		  ,[AccountId]
	FROM [DAS_PeriodEnd].PaymentsDue.RequiredPayments

	WHERE ULN IN (SELECT ULN FROM ##V1Learners)
	AND CollectionPeriodName LIKE '1819-R%'
)

SELECT * FROM RawPayments
WHERE DeliveryPeriod IN @periods

Order by UKPRN, learneruln, AcademicYear, CollectionPeriod, DeliveryPeriod, TransactionType



