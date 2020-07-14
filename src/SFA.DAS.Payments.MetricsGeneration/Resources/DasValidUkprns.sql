--declare @collectionperiod int = 3
--declare @academicyear int = 1920

SELECT 
      [Ukprn]
      
  FROM [Payments2].[Job]
  WHERE 
  JobType = 1
  and
  CollectionPeriod = @collectionperiod
  and 
  AcademicYear = @academicyear
  and
  [Status] in (2,3)
  and DCJobSucceeded = 1
  and JobType = 1 
  and 
  DCJobId in (<validDcJobIds>)