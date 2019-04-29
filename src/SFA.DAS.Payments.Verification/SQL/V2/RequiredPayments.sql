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
      
  FROM [SFA.DAS.Payments.Database].[Payments2].[RequiredPaymentEvent]

  WHERE LearnerUln IN @ulns

  Order by UKPRN, learneruln, AcademicYear, CollectionPeriod, DeliveryPeriod, TransactionType