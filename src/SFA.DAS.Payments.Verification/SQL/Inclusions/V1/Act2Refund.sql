

IF OBJECT_ID('tempdb..##Learners') IS NOT NULL
	DROP TABLE ##Learners


--DROP TABLE #Payments
IF OBJECT_ID('tempdb..#Payments') IS NULL 
BEGIN
	WITH InitialPayments AS (

		SELECT Uln, LearnRefNumber, Ukprn, PriceEpisodeIdentifier, StandardCode, ProgrammeType, 
			FrameworkCode, PathwayCode, P.DeliveryMonth, R.CollectionPeriodMonth, R.TransactionType,
			AmountDue, SfaContributionPercentage, LearnAimRef, FundingSource, Amount, ApprenticeshipContractType

		FROM [DAS_PeriodEnd].PaymentsDue.RequiredPayments R
		JOIN [DAS_PeriodEnd].Payments.Payments P
			ON R.Id = P.RequiredPaymentId

		WHERE R.CollectionPeriodName LIKE '1819-R%'
	)

	SELECT * INTO #Payments
	FROM InitialPayments
END


--DROP TABLE #Act2Payments
IF OBJECT_ID('tempdb..#Act2Payments') IS NULL 
BEGIN
	WITH Act2Payments AS (
		SELECT * FROM #Payments R
		WHERE ApprenticeshipContractType = 2
		AND SfaContributionPercentage = 0.9
		
		AND NOT EXISTS (
			SELECT * 
			FROM #Payments R1
			WHERE R1.Uln = R.Uln 
			-- Exclude ULNs that have every been ACT1
			-- Exclude ULNs that have different sfa contrib 
			AND (R1.ApprenticeshipContractType = 1 OR
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
			AND (COALESCE(R.StandardCode, 0) != COALESCE(R3.StandardCode, 0)
				OR COALESCE(R.ProgrammeType, 0) != COALESCE(R3.ProgrammeType, 0)
				OR COALESCE(R.FrameworkCode, 0) != COALESCE(R3.FrameworkCode, 0)
				OR COALESCE(R.PathwayCode, 0) != COALESCE(R3.PathwayCode, 0)
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
	)
	SELECT * INTO #Refunds
	FROM Refunds
	WHERE (
			(@restrictUkprns = 1 AND Ukprn IN @ukprns)
			OR
			(@restrictUkprns = 0)
		)
END



SELECT DISTINCT (ULN) INTO ##Learners
FROM #Refunds

--SELECT * FROM ##Learners
