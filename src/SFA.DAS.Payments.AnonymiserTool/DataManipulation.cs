using System.Collections.Generic;
using System.Threading.Tasks;
using SDA.DAS.Payments.ConsoleUtilities;
using SFA.DAS.Payments.AnonymiserTool.Dto;

namespace SFA.DAS.Payments.AnonymiserTool
{
    static class DataManipulation
    {
        public static async Task<List<long>> AlterUlns(ApprenticeshipData apprenticeshipData, Dictionary<long, ReadOptimisedProviderData> anonymisedProviders)
        {
            var apprenticeshipsToRemove = new List<long>();
            foreach (var apprenticeship in apprenticeshipData.Apprenticeships)
            {
                var ukprn = apprenticeship.Ukprn;
                if (!anonymisedProviders.ContainsKey(ukprn))
                {
                    apprenticeshipsToRemove.Add(apprenticeship.Id);
                    continue;
                }

                var providerData = anonymisedProviders[ukprn];
                if (!providerData.OptimisedLearners.ContainsKey(apprenticeship.Uln))
                {
                    apprenticeshipsToRemove.Add(apprenticeship.Id);
                    continue;
                }

                var listOfChangedLearners = providerData.OptimisedLearners[apprenticeship.Uln];
                foreach (var changedLearner in listOfChangedLearners)
                {
                    if (changedLearner.OldUln != apprenticeship.Uln)
                    {
                        await Logger.Log(
                            $"Multiple learners for UKPRN: {ukprn} and ULN: {apprenticeship.Uln} - results are not guaranteed");
                        foreach (var learner in listOfChangedLearners)
                        {
                            await Logger.Log($"New ULN: {learner.NewUln}", 1);
                        }
                    }

                    apprenticeship.Uln = changedLearner.NewUln;
                }
            }

            return apprenticeshipsToRemove;
        }

        public static async Task RemoveApprenticeships(ApprenticeshipData apprenticeshipData,
            List<long> apprenticeshipsToRemove)
        {
            await Logger.Log($"Removing {apprenticeshipsToRemove.Count} apprenticeships");

            var removed = apprenticeshipData.ApprenticeshipPauses.RemoveAll(x =>
                apprenticeshipsToRemove.Contains(x.ApprenticeshipId));
            await Logger.Log($"Removed {removed} apprenticeship paused", 1);
            
            removed = apprenticeshipData.ApprenticeshipPriceEpisodes.RemoveAll(x => 
                apprenticeshipsToRemove.Contains(x.ApprenticeshipId));
            await Logger.Log($"Removed {removed} apprenticeship price episodes", 1);

            removed = apprenticeshipData.Apprenticeships.RemoveAll(x =>
                apprenticeshipsToRemove.Contains(x.Id));
            await Logger.Log($"Removed {removed} apprenticeships", 1);
        }
    }
}
