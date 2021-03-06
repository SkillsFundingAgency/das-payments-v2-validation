SELECT 
	  [DeliveryPeriod]
      ,[CollectionPeriod]
      ,[AcademicYear]
      ,[Ukprn]
      ,[LearnerReferenceNumber]
      ,[LearnerUln]
      ,[PriceEpisodeIdentifier]
      ,[Amount]
      ,[LearningAimReference]
      ,[LearningAimProgrammeType]
      ,[LearningAimStandardCode]
      ,[LearningAimFrameworkCode]
      ,[LearningAimPathwayCode]
      ,[LearningAimFundingLineType]
      ,[ContractType]
      ,[TransactionType]
      ,[FundingSource]
      ,[IlrSubmissionDateTime]
      ,[SfaContributionPercentage]
      ,[AccountId]
	  ,CAST([AcademicYear] AS VARCHAR(4)) + '-R' + RIGHT('00' + CAST([CollectionPeriod] AS VARCHAR(2)), 2) [CollectionPeriodName]
      
  FROM [@@V2DATABASE@@].[Payments2].[Payment]

  WHERE LearnerUln IN (SELECT ULN FROM ##V2Learners)
  AND AcademicYear = 1819
  AND DeliveryPeriod IN @deliveryPeriods
  AND CollectionPeriod IN @collectionPeriods
  AND (
	(@restrictUkprns = 1 AND Ukprn IN @ukprns)
	OR
	(@restrictUkprns = 0)
)

  Order by UKPRN, learneruln, AcademicYear, CollectionPeriod, DeliveryPeriod, TransactionType, FundingSource