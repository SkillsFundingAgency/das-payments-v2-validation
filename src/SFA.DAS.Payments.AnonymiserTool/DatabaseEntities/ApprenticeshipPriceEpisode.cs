using System;

namespace SFA.DAS.Payments.AnonymiserTool.DatabaseEntities
{
    internal class ApprenticeshipPriceEpisode
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Cost { get; set; }
        public bool Removed { get; set; }
    }
}
