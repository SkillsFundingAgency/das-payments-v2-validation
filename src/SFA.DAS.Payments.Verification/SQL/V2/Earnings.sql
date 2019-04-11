WITH Data AS (
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
 )

 SELECT
	Ukprn
	[ContractType]
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
	, DeliveryPeriod
	, PriceEpisodeIdentifier
	, SfaContributionPercentage
	, TotalNegotiatedPrice1
	, TotalNegotiatedPrice2
	, TotalNegotiatedPrice3
	, TotalNegotiatedPrice4
	, StartDate
	, PlannedEndDate
	, ActualEndDate
	, Completed
	, COALESCE([1], 0) [TransactionType01]
	, COALESCE([2], 0) [TransactionType02]
	, COALESCE([3], 0) [TransactionType03]
	, COALESCE([4], 0) [TransactionType04]
	, COALESCE([5], 0) [TransactionType05]
	, COALESCE([6], 0) [TransactionType06]
	, COALESCE([7], 0) [TransactionType07]
	, COALESCE([8], 0) [TransactionType08]
	, COALESCE([9], 0) [TransactionType09]
	, COALESCE([10], 0) [TransactionType10]
	, COALESCE([11], 0) [TransactionType11]
	, COALESCE([12], 0) [TransactionType12]
	, COALESCE([13], 0) [TransactionType13]
	, COALESCE([14], 0) [TransactionType14]
	, COALESCE([15], 0) [TransactionType15]
	, COALESCE([16], 0) [TransactionType16]

	FROM (SELECT * FROM Data) AS SourceTable
	PIVOT (
		SUM(Amount) FOR TransactionType IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12], [13], [14], [15], [16])

	) AS PivotTable

Order by UKPRN, learneruln, AcademicYear, CollectionPeriod, DeliveryPeriod