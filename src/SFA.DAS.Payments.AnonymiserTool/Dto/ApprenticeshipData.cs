using System.Collections.Generic;
using SFA.DAS.Payments.AnonymiserTool.DatabaseEntities;

namespace SFA.DAS.Payments.AnonymiserTool.Dto
{
    internal class ApprenticeshipData
    {
        public List<Apprenticeship> Apprenticeships { get; set; } = new List<Apprenticeship>();
        public List<ApprenticeshipPause> ApprenticeshipPauses { get; set; } = new List<ApprenticeshipPause>();
        public List<ApprenticeshipPriceEpisode> ApprenticeshipPriceEpisodes { get; set; } = new List<ApprenticeshipPriceEpisode>();
    }
}