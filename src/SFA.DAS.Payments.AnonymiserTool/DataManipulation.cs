using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDA.DAS.Payments.ConsoleUtilities;
using SFA.DAS.Payments.AnonymiserTool.Dto;

namespace SFA.DAS.Payments.AnonymiserTool
{
    internal static class DataManipulation
    {
        public static async Task<StringBuilder> AlterUlns(ApprenticeshipData apprenticeshipData, Dictionary<long, ReadOptimisedProviderData> anonymisedProviders)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Old ULN, New Uln");

            foreach (var apprenticeship in apprenticeshipData.Apprenticeships)
            {
                var providerData = anonymisedProviders[apprenticeship.Ukprn];

                var listOfChangedLearners = providerData.OptimisedLearners[apprenticeship.Uln];
                foreach (var changedLearner in listOfChangedLearners)
                {
                    if (changedLearner.OldUln != apprenticeship.Uln)
                    {
                        await Logger.Log(
                            $"Multiple learners for UKPRN: {apprenticeship.Ukprn} and ULN: {apprenticeship.Uln} - results are not guaranteed");

                        foreach (var learner in listOfChangedLearners)
                        {
                            stringBuilder.AppendLine($"{learner.OldUln},{learner.NewUln}");
                            await Logger.Log($"New ULN: {learner.NewUln}", 1);
                        }
                    }

                    apprenticeship.Uln = changedLearner.NewUln;
                }
            }

            return stringBuilder;
        }

        public static async Task RemoveApprenticeships(ApprenticeshipData apprenticeshipData,
            Dictionary<long, ReadOptimisedProviderData> anonymisedProviders)
        {
            var apprenticeshipsToRemove = apprenticeshipData.Apprenticeships.Where(apprenticeship =>
                    !anonymisedProviders.ContainsKey(apprenticeship.Ukprn) ||
                    !anonymisedProviders[apprenticeship.Ukprn].OptimisedLearners.ContainsKey(apprenticeship.Uln))
                .Select(a => a.Id)
                .ToList();

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
