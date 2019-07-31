WITH MaxJobId AS (
	SELECT MAX(JobId) [JobId], Ukprn
	FROM [@@V2DATABASE@@].[Payments2].[RequiredPaymentEvent] 
	WHERE AcademicYear = 1819
	AND DeliveryPeriod IN @deliveryPeriods
	AND CollectionPeriod IN @collectionPeriods
	AND LearnerUln IN (SELECT ULN FROM ##V2Learners)
	GROUP BY Ukprn
)

SELECT [PriceEpisodeIdentifier]
      ,[Ukprn]
      ,[ContractType]
      ,[TransactionType]
      ,[SfaContributionPercentage]
      ,[Amount]
      ,[CollectionPeriod]
      ,[AcademicYear]
      ,[DeliveryPeriod]
      ,[LearnerReferenceNumber]
      ,[LearnerUln]
      ,[LearningAimReference]
      ,[LearningAimProgrammeType]
      ,[LearningAimStandardCode]
      ,[LearningAimFrameworkCode]
      ,[LearningAimPathwayCode]
      ,[LearningAimFundingLineType]
      ,[AccountId]
      
  FROM [@@V2DATABASE@@].[Payments2].[RequiredPaymentEvent]

  WHERE LearnerUln IN (SELECT ULN FROM ##V2Learners)
  AND AcademicYear = 1819
  AND DeliveryPeriod IN @deliveryPeriods
  AND CollectionPeriod IN @collectionPeriods
  AND JobId IN (SELECT JobId FROM MaxJobId)
  AND (
	(@restrictUkprns = 1 AND Ukprn IN @ukprns)
	OR
	(@restrictUkprns = 0)
)

  Order by UKPRN, learneruln, AcademicYear, CollectionPeriod, DeliveryPeriod, TransactionType