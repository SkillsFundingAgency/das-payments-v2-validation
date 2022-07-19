using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.Payments.FM36Tool.ApprenticeshipSetup
{
    public class ApprenticeshipHelper
    {
        private readonly PaymentsDataContext _paymentsDataContext;

        public ApprenticeshipHelper(PaymentsDataContext paymentsDataContext)
        {
            _paymentsDataContext = paymentsDataContext ?? throw new ArgumentNullException(nameof(paymentsDataContext));
        }

        public async Task SetupTestApprenticeshipData(FM36Global fm36, short currentAcademicYear)
        {
            var maxEmployerAccountId = await _paymentsDataContext.LevyAccount.MaxAsync(l => (int?)l.AccountId);

            var random = new Random(Guid.NewGuid().GetHashCode());
            LevyAccountModel employerAccount = null;

            while (employerAccount == null)
            {
                var randomAccountId = random.NextLong(maxEmployerAccountId ?? 1);
                employerAccount = await _paymentsDataContext.LevyAccount.SingleOrDefaultAsync(l => l.AccountId == randomAccountId);
            }

            //if (employerAccount == null) throw new InvalidOperationException("AccountID is needed");

            var existingApprenticeshipIds = await _paymentsDataContext.Apprenticeship.Select(l => l.Id).ToListAsync();
            foreach (var fm36Learner in fm36.Learners)
            {
                //TODO anonomise Uln flag
                fm36Learner.ULN = random.Next(10000000, 99999999);

                var groupedLearningDeliveries = fm36Learner.LearningDeliveries
                    .Where(learningDelivery => learningDelivery.LearningDeliveryValues.LearnAimRef.Equals("ZPROG001", StringComparison.OrdinalIgnoreCase))
                    .GroupBy(ld => new
                    {
                        ld.LearningDeliveryValues.LearnAimRef,
                        ld.LearningDeliveryValues.FworkCode,
                        ld.LearningDeliveryValues.ProgType,
                        ld.LearningDeliveryValues.PwayCode,
                        ld.LearningDeliveryValues.StdCode
                    });

                foreach (var groupedLearningDelivery in groupedLearningDeliveries)
                {
                    var orderedGroupedLearningDelivery = groupedLearningDelivery.OrderByDescending(x => x.LearningDeliveryValues.LearnStartDate).ToList();

                    var priceEpisodes = fm36Learner.PriceEpisodes
                        .Where(x => orderedGroupedLearningDelivery.Any(g => g.AimSeqNumber == x.PriceEpisodeValues.PriceEpisodeAimSeqNumber))
                        .ToList();

                    var group = priceEpisodes
                        .Where(pe => IsPriceEpisodePayable(pe.PriceEpisodePeriodisedValues))
                        .GroupBy(p => p.PriceEpisodeValues.PriceEpisodeContractType);

                    foreach (var episodes in group)
                    {
                        var newApprenticeshipId = random.NextLong();
                        while (existingApprenticeshipIds.Contains(newApprenticeshipId))
                        {
                            newApprenticeshipId = random.NextLong();
                        }

                        var apprenticeshipModel = new ApprenticeshipBuilder()
                            .BuildSimpleApprenticeship(newApprenticeshipId, fm36.UKPRN, fm36Learner.ULN, employerAccount.AccountId, employerAccount.AccountName, groupedLearningDelivery.First().LearningDeliveryValues)
                            .WithALevyPayingEmployer()
                            .WithApprenticeshipPriceEpisode(episodes)
                            .ToApprenticeshipModel();

                        await _paymentsDataContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM [Payments2].[Apprenticeship] WHERE Uln = {fm36Learner.ULN}; DELETE FROM [Payments2].[ApprenticeshipPriceEpisode] WHERE ApprenticeshipId IN (SELECT ID FROM [Payments2].[Apprenticeship] WHERE Uln = {fm36Learner.ULN});");

                        await _paymentsDataContext.Apprenticeship.AddAsync(apprenticeshipModel);
                        await _paymentsDataContext.ApprenticeshipPriceEpisode.AddRangeAsync(apprenticeshipModel.ApprenticeshipPriceEpisodes);
                    }
                }
            }

            await _paymentsDataContext.SaveChangesAsync();
        }

        private static bool IsPriceEpisodePayable(List<PriceEpisodePeriodisedValues> priceEpisodePeriodisedValuesList)
        {
            //var calendarYear = currentAcademicYear / 100 + 2000;
            //var yearStartDate = new DateTime(calendarYear, 8, 1);
            //var yearEndDate = yearStartDate.AddYears(1);

            //var episodeStartDate = priceEpisodeValues.EpisodeStartDate;
            //var isCurrent = episodeStartDate.HasValue &&
            //       episodeStartDate.Value >= yearStartDate &&
            //       episodeStartDate.Value < yearEndDate;

            var periods = priceEpisodePeriodisedValuesList
                .FirstOrDefault(pe => pe.AttributeName == "PriceEpisodeOnProgPayment");
            var isPayable = periods != null && 0 != periods.Period1 + periods.Period2 + periods.Period3 + periods.Period4 + periods.Period5 + periods.Period6 + periods.Period7 + periods.Period8 + periods.Period9 + periods.Period10 + periods.Period11 + periods.Period12;

            //return isCurrent && isPayable;
            return isPayable;
        }
    }
}