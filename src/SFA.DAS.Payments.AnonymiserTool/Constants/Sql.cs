namespace SFA.DAS.Payments.AnonymiserTool.Constants
{
    static class Sql
    {
        public static string Apprenticeships = @"
SELECT *
FROM Payments2.Apprenticeship
WHERE Ukprn IN @ukprns
ORDER BY Ukprn, Uln
";

        public static string ApprenticeshipPriceEpisodes = @"
SELECT *
FROM Payments2.ApprenticeshipPriceEpisode
WHERE ApprenticeshipId IN (SELECT Id FROM Payments2.Apprenticeship WHERE Ukprn IN @ukprns)
";

        public static string ApprenticeshipPauses = @"
SELECT *
FROM Payments2.ApprenticeshipPause
WHERE ApprenticeshipId IN (SELECT Id FROM Payments2.Apprenticeship WHERE Ukprn IN @ukprns)
";
    }
}
