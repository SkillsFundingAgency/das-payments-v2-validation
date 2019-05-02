
IF OBJECT_ID('tempdb..##Learners') IS NOT NULL
	DROP TABLE ##Learners


;WITH InitialPayments AS (

	SELECT [LearnerUln] [Uln]
	FROM [SFA.DAS.Payments.Database].[Payments2].[Payment]
	WHERE AcademicYear = 1819
)

SELECT * INTO ##Learners
	FROM InitialPayments


