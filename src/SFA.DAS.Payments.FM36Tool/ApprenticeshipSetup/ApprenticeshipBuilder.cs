using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;

namespace SFA.DAS.Payments.FM36Tool.ApprenticeshipSetup
{
    public class ApprenticeshipBuilder
    {
        private ApprenticeshipModel _apprenticeship;

        public ApprenticeshipBuilder BuildSimpleApprenticeship(long id, long ukprn, long uln, long employerAccountId, string employerAccountName, LearningDeliveryValues learningDeliveryValues)
        {
            _apprenticeship ??= new ApprenticeshipModel();
            _apprenticeship.Id = id;
            _apprenticeship.Ukprn = ukprn;
            _apprenticeship.AccountId = employerAccountId;
            _apprenticeship.Uln = uln;
            _apprenticeship.StandardCode = learningDeliveryValues.StdCode ?? 0;
            _apprenticeship.ProgrammeType = learningDeliveryValues.ProgType;
            _apprenticeship.Status = ApprenticeshipStatus.Active;
            _apprenticeship.LegalEntityName = employerAccountName;
            _apprenticeship.EstimatedStartDate = new DateTime(2019, 08, 01);
            _apprenticeship.EstimatedEndDate = new DateTime(2020, 08, 06);
            _apprenticeship.AgreedOnDate = DateTime.UtcNow;
            _apprenticeship.FrameworkCode = learningDeliveryValues.FworkCode ?? 0;
            _apprenticeship.PathwayCode = learningDeliveryValues.PwayCode ?? 0;

            return this;
        }

        public ApprenticeshipBuilder WithALevyPayingEmployer()
        {
            _apprenticeship.IsLevyPayer = true;
            _apprenticeship.ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy;

            return this;
        }

        public ApprenticeshipBuilder WithApprenticeshipPriceEpisode(IGrouping<string, PriceEpisode> fm36PriceEpisodeValues)
        {
            _apprenticeship ??= new ApprenticeshipModel();
            _apprenticeship.ApprenticeshipPriceEpisodes ??= new List<ApprenticeshipPriceEpisodeModel>();

            var apprenticeshipPriceEpisodes = fm36PriceEpisodeValues.Select(pe =>
            {
                var priceEpisodeValues = pe.PriceEpisodeValues;
                return new ApprenticeshipPriceEpisodeModel
                {
                    ApprenticeshipId = _apprenticeship.Id,
                    Cost = priceEpisodeValues.PriceEpisodeTotalTNPPrice.GetValueOrDefault(),
                    StartDate = new DateTime(2017, 08, 01),
                    EndDate = new DateTime(2023, 07, 31)
                };
            });
            _apprenticeship.ApprenticeshipPriceEpisodes.AddRange(apprenticeshipPriceEpisodes);

            return this;
        }

        public ApprenticeshipModel ToApprenticeshipModel()
        {
            return _apprenticeship;
        }
    }
}