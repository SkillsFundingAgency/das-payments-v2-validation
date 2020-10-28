using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using SFA.DAS.Payments.Contingency.DTO;

namespace SFA.DAS.Payments.Contingency
{
    class AuditData
    {
        public static async Task Output<T>(List<T> earnings, string filename)
        {
            using (var writer = new StreamWriter(filename))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(earnings);
            }
        }
    }
}
