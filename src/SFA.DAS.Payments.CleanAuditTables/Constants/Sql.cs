namespace SFA.DAS.Payments.CleanAuditTables.Constants
{
    static class Sql
    {
        public const string CheckIfJobIsRunning = @"
SELECT COUNT(*)
FROM Payments2.Job
WHERE Status = 1
AND DATEADD(hour, 2, StartTime) > GETDATE()
";

        public const string CleanAuditForPeriod = @"
DROP TABLE IF EXISTS #JobIds
SELECT * INTO #JobIds FROM (
	SELECT DcJobId
	FROM Payments2.LatestSuccessfulJobs
	
	UNION
	
	SELECT DcJobId
	FROM Payments2.Job
	WHERE [Status] = 1
	AND StartTime > DATEADD(d, -1, GETDATE())
) q


DROP TABLE IF EXISTS #EarningEventIdsToDelete
SELECT EventId INTO #EarningEventIdsToDelete 
FROM (
    SELECT EventId FROM Payments2.EarningEvent EE
    WHERE EE.JobId NOT IN (SELECT DcJobId FROM #JobIds)
    AND CollectionPeriod = @collectionPeriod
    AND AcademicYear = @academicYear
) q

PRINT 'Deleting Earning Events and related tables'
DELETE Payments2.EarningEventPeriod
WHERE EarningEventId IN (
    SELECT EventId FROM #EarningEventIdsToDelete
)
DELETE Payments2.EarningEventPriceEpisode
WHERE EarningEventId IN (
    SELECT EventId FROM #EarningEventIdsToDelete
)
DELETE Payments2.EarningEvent
WHERE EventId IN (
    SELECT EventId FROM #EarningEventIdsToDelete
)


DROP TABLE IF EXISTS #RequiredPaymentsToDelete
SELECT EventId INTO #RequiredPaymentsToDelete
FROM (
	SELECT EventId 
    FROM Payments2.RequiredPaymentEvent
    WHERE JobId NOT IN (
	    SELECT DcJobId FROM #JobIds
    )
    AND CollectionPeriod = @collectionPeriod
    AND AcademicYear = @academicYear
) q


PRINT 'Deleting Funding Source Events and related tables'
DELETE Payments2.FundingSourceEvent
WHERE RequiredPaymentEventId IN (
    SELECT EventId FROM #RequiredPaymentsToDelete
)

PRINT 'Deleting Required Payment Events and related tables'
DELETE Payments2.RequiredPaymentEvent
WHERE EventId IN (
	SELECT EventId FROM #RequiredPaymentsToDelete
)


DROP TABLE IF EXISTS #DatalocksToDelete
SELECT EventId INTO #DatalocksToDelete
FROM (
	SELECT EventId FROM Payments2.DataLockEvent DLE
	WHERE DLE.JobId NOT IN (SELECT DcJobId FROM #JobIds)
	AND CollectionPeriod = @collectionPeriod
    AND AcademicYear = @academicYear
) q


PRINT 'Deleting Datalock Events and related tables'
DELETE Payments2.DataLockEventNonPayablePeriodFailures
WHERE DataLockEventNonPayablePeriodId IN (
	SELECT DataLockEventNonPayablePeriodId FROM Payments2.DataLockEventNonPayablePeriod
	WHERE DataLockEventId IN (
		SELECT EventId FROM #DatalocksToDelete
	)
)
DELETE Payments2.DataLockEventNonPayablePeriod
WHERE DataLockEventId IN (
	SELECT EventId FROM #DatalocksToDelete
)
DELETE Payments2.DataLockEventPayablePeriod
WHERE DataLockEventId IN (
	SELECT EventId FROM #DatalocksToDelete
)
DELETE Payments2.DataLockEventPriceEpisode
WHERE DataLockEventId IN (
	SELECT EventId FROM #DatalocksToDelete
)
DELETE Payments2.DataLockEvent
WHERE EventId IN (
	SELECT EventId FROM #DatalocksToDelete
)

";
    }
}
