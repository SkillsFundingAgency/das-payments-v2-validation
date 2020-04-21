using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using SFA.DAS.Payments.AnonymiserTool.Constants;
using SFA.DAS.Payments.AnonymiserTool.DatabaseEntities;

namespace SFA.DAS.Payments.AnonymiserTool.V2Database
{
    class DatabaseUtilities
    {
        public static ApprenticeshipData LoadProductionApprenticeships(List<long> ukprns)
        {
            var result = new ApprenticeshipData();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ProductionV2DatabaseConnectionString"].ConnectionString))
            {
                var apprenticeships = connection.Query<Apprenticeship>(Sql.Apprenticeships, new {ukprns}, commandTimeout: 3600);
                var priceEpisodes = connection.Query<ApprenticeshipPriceEpisode>(Sql.ApprenticeshipPriceEpisodes, new {ukprns}, commandTimeout: 3600);
                var pauses = connection.Query<ApprenticeshipPause>(Sql.ApprenticeshipPauses, new {ukprns}, commandTimeout: 3600);

                result.Apprenticeships.AddRange(apprenticeships);
                result.ApprenticeshipPauses.AddRange(pauses);
                result.ApprenticeshipPriceEpisodes.AddRange(priceEpisodes);
            }

            return result;
        }
    }
}
