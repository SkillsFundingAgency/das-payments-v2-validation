SELECT 
      [Ukprn]
      ,[ContractType]
      ,[CollectionPeriod]
      ,[AcademicYear]
      ,[LearnerReferenceNumber]
      ,[LearnerUln]
      ,[LearningAimReference]
      ,[LearningAimProgrammeType]
      ,[LearningAimStandardCode]
      ,[LearningAimFrameworkCode]
      ,[LearningAimPathwayCode]
      ,[LearningAimFundingLineType]
   ,P.Amount
   , P.DeliveryPeriod
   , P.PriceEpisodeIdentifier
   , P.SfaContributionPercentage
   , P.TransactionType
   , PE.TotalNegotiatedPrice1
   , PE.TotalNegotiatedPrice2
   , PE.TotalNegotiatedPrice3
   , PE.TotalNegotiatedPrice4
   , PE.StartDate
   , PE.PlannedEndDate
   , PE.ActualEndDate
   , PE.Completed

FROM [SFA.DAS.Payments.Database].[Payments2].[EarningEvent] E
  JOIN [SFA.DAS.Payments.Database].Payments2.EarningEventPeriod P
 ON E.EventId = P.EarningEventId
JOIN [SFA.DAS.Payments.Database].Payments2.EarningEventPriceEpisode PE
 ON E.EventId = PE.EarningEventId 