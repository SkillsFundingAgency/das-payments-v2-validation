namespace SFA.DAS.Payments.CleanAuditTables.Constants
{
    static class Sql
    {
        public const string CleanAuditForPeriod = @"

DELETE Payments2.EarningEventPeriod
WHERE EarningEventId IN (
	SELECT EventId FROM Payments2.EarningEvent EE
	WHERE EE.JobId NOT IN (SELECT DcJobId FROM Payments2.LatestSuccessfulJobs)
	AND CollectionPeriod = @collectionPeriod
)



DELETE Payments2.EarningEventPriceEpisode
WHERE EarningEventId IN (
	SELECT EventId FROM Payments2.EarningEvent EE
	WHERE EE.JobId NOT IN (SELECT DcJobId FROM Payments2.LatestSuccessfulJobs)
	AND CollectionPeriod = @collectionPeriod
)

DELETE Payments2.EarningEvent
WHERE JobId NOT IN (
	SELECT DcJobId FROM Payments2.LatestSuccessfulJobs
)
AND CollectionPeriod = @collectionPeriod


DELETE Payments2.RequiredPaymentEvent
WHERE JobId NOT IN (
	SELECT DcJobId FROM Payments2.LatestSuccessfulJobs
)
AND CollectionPeriod = @collectionPeriod


DELETE Payments2.DataLockEventNonPayablePeriodFailures
WHERE DataLockEventNonPayablePeriodId IN (
	SELECT DataLockEventNonPayablePeriodId FROM Payments2.DataLockEventNonPayablePeriod
	WHERE DataLockEventId IN (
		SELECT EventId FROM Payments2.DataLockEvent DLE
		WHERE DLE.JobId NOT IN (SELECT DcJobId FROM Payments2.LatestSuccessfulJobs)
		AND CollectionPeriod = @collectionPeriod
	)
)

DELETE Payments2.DataLockEventNonPayablePeriod
WHERE DataLockEventId IN (
	SELECT EventId FROM Payments2.DataLockEvent DLE
	WHERE DLE.JobId NOT IN (SELECT DcJobId FROM Payments2.LatestSuccessfulJobs)
	AND CollectionPeriod = @collectionPeriod
)

DELETE Payments2.DataLockEventPayablePeriod
WHERE DataLockEventId IN (
	SELECT EventId FROM Payments2.DataLockEvent DLE
	WHERE DLE.JobId NOT IN (SELECT DcJobId FROM Payments2.LatestSuccessfulJobs)
	AND CollectionPeriod = @collectionPeriod
)

DELETE Payments2.DataLockEventPriceEpisode
WHERE DataLockEventId IN (
	SELECT EventId FROM Payments2.DataLockEvent DLE
	WHERE DLE.JobId NOT IN (SELECT DcJobId FROM Payments2.LatestSuccessfulJobs)
	AND CollectionPeriod = @collectionPeriod
)

DELETE Payments2.DataLockEvent
WHERE JobId NOT IN (SELECT DcJobId FROM Payments2.LatestSuccessfulJobs)
AND CollectionPeriod = @collectionPeriod

";
    }
}
