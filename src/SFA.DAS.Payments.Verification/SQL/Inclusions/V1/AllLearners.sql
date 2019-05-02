
--DROP TABLE #Payments
IF OBJECT_ID('tempdb..#Payments') IS NULL 
BEGIN
	WITH InitialPayments AS (

		SELECT Uln
		FROM [DAS_PeriodEnd].PaymentsDue.RequiredPayments R
		WHERE R.CollectionPeriodName LIKE '1819-R%'
	)

	SELECT * INTO #Learners
	 FROM InitialPayments
END

