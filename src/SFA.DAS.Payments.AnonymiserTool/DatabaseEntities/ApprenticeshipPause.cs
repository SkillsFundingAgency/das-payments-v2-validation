using System;

namespace SFA.DAS.Payments.AnonymiserTool.DatabaseEntities
{
    class ApprenticeshipPause
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public DateTime PauseDate { get; set; }
        public DateTime? ResumeDate { get; set; }
    }
}
