using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.Migration.Constants
{
    public static class CollectionPeriods
    {
        public static Dictionary<int, DateTime> CollectionPeriodDates = new Dictionary<int, DateTime>
        {
            {1, new DateTime(2018, 9, 6, 22, 00, 00) },
            {2, new DateTime(2018, 10, 4, 22, 00, 00) },
            {3, new DateTime(2018, 11, 6, 22, 00, 00) },
            {4, new DateTime(2018, 12, 6, 22, 00, 00) },
            {5, new DateTime(2019, 1, 7, 22, 00, 00) },
            {6, new DateTime(2019, 2, 6, 22, 00, 00) },
            {7, new DateTime(2019, 3, 6, 22, 00, 00) },
            {8, new DateTime(2019, 4, 4, 22, 00, 00) },
            {9, new DateTime(2019, 5, 7, 22, 00, 00) },
            {10, new DateTime(2019, 6, 6, 22, 00, 00) },
            {11, new DateTime(2019, 7, 4, 22, 00, 00) },
            {12, new DateTime(2019, 8, 6, 22, 00, 00) },
            {13, new DateTime(2019, 10, 15, 22, 00, 00) },
            {14, new DateTime(2019, 11, 15, 22, 00, 00) },
        };
    }
}
