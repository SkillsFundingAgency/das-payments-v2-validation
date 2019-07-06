using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.Migration.Constants
{
    public static class CollectionPeriods
    {
        public static Dictionary<int, DateTime> CollectionPeriodDates = new Dictionary<int, DateTime>
        {
            {1, new DateTime(2018, 9, 6, 22, 00, 00) },
            {1, new DateTime(2018, 10, 4, 22, 00, 00) },
            {1, new DateTime(2018, 11, 6, 22, 00, 00) },
            {1, new DateTime(2018, 12, 6, 22, 00, 00) },
            {1, new DateTime(2019, 1, 7, 22, 00, 00) },
            {1, new DateTime(2019, 2, 6, 22, 00, 00) },
            {1, new DateTime(2019, 3, 6, 22, 00, 00) },
            {1, new DateTime(2019, 4, 4, 22, 00, 00) },
            {1, new DateTime(2019, 5, 7, 22, 00, 00) },
            {1, new DateTime(2019, 6, 6, 22, 00, 00) },
            {1, new DateTime(2019, 7, 4, 22, 00, 00) },
        };
    }
}
