

IF OBJECT_ID('tempdb..##Learners') IS NOT NULL
	DROP TABLE ##Learners


;WITH InitialPayments AS (

	SELECT Uln
	FROM [DAS_PeriodEnd].PaymentsDue.RequiredPayments R
	WHERE R.CollectionPeriodName LIKE '1819-R%'
	AND (
		(@restrictUkprns = 1 AND Ukprn IN @ukprns)
		OR
		(@restrictUkprns = 0)
	)
)

SELECT * INTO ##Learners
	FROM InitialPayments


