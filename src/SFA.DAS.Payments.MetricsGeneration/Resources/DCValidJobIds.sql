declare @collectionperiod int = 3
declare @academicyear int = 1920
	
Select 
JOBID
from
dbo.vw_JobInfo j
where 
j.[Status] = 4
AND
J.CollectionId =8
AND J.PeriodNumber = @collectionperiod
AND J.CollectionYear = @academicyear