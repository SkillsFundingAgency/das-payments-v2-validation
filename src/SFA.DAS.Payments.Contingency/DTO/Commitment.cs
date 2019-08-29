using System;

namespace SFA.DAS.Payments.Contingency.DTO
{
    public class Commitment
    {
        public long Ukprn { get; set; }
        public long Uln { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public int StandardCode { get; set; }
        public int ProgrammeType { get; set; }
        public int FrameworkCode { get; set; }
        public int PathwayCode { get; set; }
    }
}