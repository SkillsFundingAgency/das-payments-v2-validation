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

  WHERE LearnerUln IN (SELECT ULN FROM ##Learners)
  AND AcademicYear = 1819
  AND DeliveryPeriod IN @periods
  AND CollectionPeriod IN @periods

  Order by UKPRN, learneruln, AcademicYear, CollectionPeriod, DeliveryPeriod, TransactionType