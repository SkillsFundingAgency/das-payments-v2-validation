

IF OBJECT_ID('tempdb..##Learners') IS NOT NULL
	DROP TABLE ##Learners


--DROP TABLE #Payments
IF OBJECT_ID('tempdb..#Payments') IS NULL 
BEGIN
	WITH InitialPayments AS (

		SELECT [LearnerUln] [Uln], *
		FROM [SFA.DAS.Payments.Database].[Payments2].[Payment]
		WHERE AcademicYear = 1819
	)

	SELECT * INTO #Payments
	FROM InitialPayments
END


--DROP TABLE #Act2Payments
IF OBJECT_ID('tempdb..#Act2Payments') IS NULL 
BEGIN
	WITH Act2Payments AS (
		SELECT * FROM #Payments R
		WHERE ContractType = 2
		AND SfaContributionPercentage = 0.9
		
		AND NOT EXISTS (
			SELECT * 
			FROM #Payments R1
			WHERE R1.Uln = R.Uln 
			-- Exclude ULNs that have every been ACT1
			-- Exclude ULNs that have different sfa contrib 
			AND (R1.ContractType = 1 OR
				R1.SfaContributionPercentage != 0.9)
		)
	)

	SELECT * INTO #Act2Payments 
	FROM Act2Payments
END

--DROP TABLE #OnProgAct2Payments
IF OBJECT_ID('tempdb..#OnProgAct2Payments') IS NULL 
BEGIN
	WITH OnProgAct2Payments AS (
		SELECT * FROM #Act2Payments R
				WHERE TransactionType IN (1, 2, 3)

		-- Exclude ULNs that have non-on-prog transaction types
		AND NOT EXISTS (
			SELECT *
			FROM #Act2Payments R2
			WHERE R.Uln = R2.Uln
			AND R2.TransactionType > 3
		)
	)
	SELECT * INTO #OnProgAct2Payments
	FROM OnProgAct2Payments
END


--DROP TABLE #SingleCourseOnProgAct2Payments
IF OBJECT_ID('tempdb..#SingleCourseOnProgAct2Payments') IS NULL 
BEGIN
	WITH SingleCourseOnProgAct2Payments AS (
		SELECT * FROM #OnProgAct2Payments R

		-- Exclude ULNs that have non-on-prog transaction types
		WHERE NOT EXISTS (
			SELECT *
			FROM #OnProgAct2Payments R3
			WHERE R3.Uln = R.Uln
			AND (COALESCE(R.LearningAimStandardCode, 0) != COALESCE(R3.LearningAimStandardCode, 0)
				OR COALESCE(R.LearningAimProgrammeType, 0) != COALESCE(R3.LearningAimProgrammeType, 0)
				OR COALESCE(R.LearningAimFrameworkCode, 0) != COALESCE(R3.LearningAimFrameworkCode, 0)
				OR COALESCE(R.LearningAimPathwayCode, 0) != COALESCE(R3.LearningAimPathwayCode, 0)
			)
		)
	)
	SELECT * INTO #SingleCourseOnProgAct2Payments
	FROM SingleCourseOnProgAct2Payments
END



--DROP TABLE #SingePaymentPerMonth
IF OBJECT_ID('tempdb..#Refunds') IS NULL 
BEGIN
	WITH Refunds AS (
		SELECT * FROM #SingleCourseOnProgAct2Payments R

		-- Exclude ULNs that have no refunds
		WHERE EXISTS (
			SELECT *
			FROM #SingleCourseOnProgAct2Payments R2
			WHERE R.Uln = R2.Uln
			AND R2.Amount < 0
		)
		--AND Ukprn IN @ukprns
	)
	SELECT * INTO #Refunds
	FROM Refunds
END



SELECT DISTINCT (ULN) INTO ##Learners
FROM #Refunds

--SELECT * FROM ##Learners
