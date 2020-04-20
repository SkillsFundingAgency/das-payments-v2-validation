using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDA.DAS.Payments.ConsoleUtilities;
using SFA.DAS.Payments.AnonymiserTool.DatabaseEntities;

namespace SFA.DAS.Payments.AnonymiserTool
{
    class DataManipulation
    {
        public static async Task RemoveApprenticeships(ApprenticeshipData apprenticeshipData,
            List<Apprenticeship> apprenticeshipsToRemove)
        {
            await Logger.Log($"Removing {apprenticeshipsToRemove.Count} apprenticeships");
            await Logger.Log($"Optimising the data...", 1);

            var pausesByApprenticeshipId = apprenticeshipData
                .ApprenticeshipPauses
                .ToLookup(x => x.ApprenticeshipId);

            var priceEpisodesByApprenticeshipId = apprenticeshipData
                .ApprenticeshipPriceEpisodes
                .ToLookup(x => x.ApprenticeshipId);

            var counter = 0;

            foreach (var apprenticeship in apprenticeshipsToRemove)
            {
                if (pausesByApprenticeshipId.Contains(apprenticeship.Id))
                {
                    foreach (var apprenticeshipPause in pausesByApprenticeshipId[apprenticeship.Id])
                    {
                        apprenticeshipData.ApprenticeshipPauses.Remove(apprenticeshipPause);
                    }
                }

                if (priceEpisodesByApprenticeshipId.Contains(apprenticeship.Id))
                {
                    foreach (var apprenticeshipPriceEpisode in priceEpisodesByApprenticeshipId[apprenticeship.Id])
                    {
                        apprenticeshipData.ApprenticeshipPriceEpisodes.Remove(apprenticeshipPriceEpisode);
                    }
                }
                
                apprenticeshipData.Apprenticeships.Remove(apprenticeship);

                counter++;
                if (counter % 1000 == 0)
                {
                    await Logger.Log($"Removed {counter} apprenticeships", 1);
                }
            }
        }
    }
}
