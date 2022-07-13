using System;

namespace SFA.DAS.Payments.FM36Tool.Data
{
    public class PeriodEndTaskData
    {
        public PeriodEndTaskData()
        {
            var month = DateTime.UtcNow.Month;
            var year = DateTime.UtcNow.Year;

            if (month < 8)
            {
                AcademicYear = (short)((year - 2000 - 1) * 100 + (year - 2000));
            }
            else
            {
                AcademicYear = (short)((year - 2000) * 100 + (year - 2000 + 1));
            }


            if (month < 8)
            {
                CollectionPeriod = (month + 5);
            }
            else
            {
                CollectionPeriod = (month - 7);
            }
        }

        public short AcademicYear { get; set; }
        public int CollectionPeriod { get; set; }
    }
}