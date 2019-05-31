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
      
  FROM [SFA.DAS.Payments.Database].[Payments2].[Payment]

  WHERE LearnerUln IN (SELECT ULN FROM ##Learners)
  AND AcademicYear = 1819
  AND DeliveryPeriod IN @periods

  Order by UKPRN, learneruln, AcademicYear, CollectionPeriod, DeliveryPeriod, TransactionType, FundingSource