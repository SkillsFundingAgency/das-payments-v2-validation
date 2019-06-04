
IF OBJECT_ID('tempdb..##Learners') IS NOT NULL
	DROP TABLE ##Learners


;WITH InitialPayments AS (

	SELECT [LearnerUln] [Uln]
	FROM [@@V2DATABASE@@].[Payments2].[Payment]
	WHERE AcademicYear = 1819
	AND (
			(@restrictUkprns = 1 AND Ukprn IN @ukprns)
			OR
			(@restrictUkprns = 0)
		)
)

SELECT * INTO ##Learners
	FROM InitialPayments


