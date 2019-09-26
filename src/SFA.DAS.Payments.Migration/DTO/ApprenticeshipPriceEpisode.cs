using System;

namespace SFA.DAS.Payments.Migration.DTO
{
    class ApprenticeshipPriceEpisode
    {
        public long ApprenticeshipId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Cost { get; set; }
        public bool Removed { get; set; }
        public DateTime CreationDate { get; set; }
    }
}


 
