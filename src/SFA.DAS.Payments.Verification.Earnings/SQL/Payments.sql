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
      
  FROM [Payments2].[Payment]

  WHERE AcademicYear = 1920
  AND DeliveryPeriod IN (1, 2)
  

  Order by UKPRN, learneruln, AcademicYear, CollectionPeriod, DeliveryPeriod, TransactionType, FundingSource