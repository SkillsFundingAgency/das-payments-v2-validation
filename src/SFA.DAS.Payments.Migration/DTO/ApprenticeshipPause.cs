using System;

namespace SFA.DAS.Payments.Migration.DTO
{
    class ApprenticeshipPause
    {
        public long ApprenticeshipId { get; set; }
        public DateTime PauseDate { get; set; } = DateTime.Now;
    }
}
