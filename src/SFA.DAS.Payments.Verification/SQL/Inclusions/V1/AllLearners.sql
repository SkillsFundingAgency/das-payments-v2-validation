

IF OBJECT_ID('tempdb..##V1Learners') IS NOT NULL
	DROP TABLE ##V1Learners


;WITH InitialPayments AS (

	SELECT Uln
	FROM [DAS_PeriodEnd].PaymentsDue.RequiredPayments R
	WHERE R.CollectionPeriodName LIKE '1819-R%'
	AND (
		(@restrictUkprns = 1 AND Ukprn IN @ukprns)
		OR
		(@restrictUkprns = 0)
	)
	AND (
		(@restrictCollectionPeriods = 1 AND cast(substring(R.CollectionPeriodName, 7, 2) as int) IN @collectionPeriods)
		OR
		(@restrictCollectionPeriods = 0)
	)
	AND (
		(R.DeliveryMonth > 7 AND (R.DeliveryMonth - 7) IN @deliveryPeriods)
		OR
		(R.DeliveryMonth <= 7 AND (R.DeliveryMonth + 5) IN @deliveryPeriods)
	)
)

SELECT * INTO ##V1Learners
	FROM InitialPayments


