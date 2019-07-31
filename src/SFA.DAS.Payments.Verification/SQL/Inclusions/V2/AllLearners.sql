
IF OBJECT_ID('tempdb..##V2Learners') IS NOT NULL
	DROP TABLE ##V2Learners


;WITH InitialPayments AS (

	SELECT [LearnerUln] [Uln]
	FROM [@@V2DATABASE@@].[Payments2].[Payment]
	WHERE AcademicYear = 1819
	AND (
			(@restrictUkprns = 1 AND Ukprn IN @ukprns)
			OR
			(@restrictUkprns = 0)
		)
	AND (
		(@restrictCollectionPeriods = 1 AND CollectionPeriod IN @collectionPeriods)
		OR
		(@restrictCollectionPeriods = 0)
	)
	AND DeliveryPeriod IN @deliveryPeriods
)

SELECT * INTO ##V2Learners
	FROM InitialPayments


