using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Dapper;
using SFA.DAS.Payments.Contingency.Constants;
using SFA.DAS.Payments.Contingency.DTO;

namespace SFA.DAS.Payments.Contingency.CongingencyStrategies
{
    class R13Payments : IProduceContingencyPayments
    {
        public async Task GenerateContingencyPayments(int period)
        {
            List<Payment> payments;
            
            Console.WriteLine("Processing R13 Payments...");

            // Load data
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DASPayments"].ConnectionString))
            {
                payments = (await connection.QueryAsync<Payment>(Sql.R13Payments, commandTimeout: 3600)
                    .ConfigureAwait(false)).ToList();
            }
            Console.WriteLine($"Loaded {payments.Count} R13 Payments");


            // Get all earnings
            // Write earnings to 'Earnings' tab
            var excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));

            // Write a summary tab
            var sheet = excel.Worksheet("Final Amounts (Full)");
            WritePaymentsToTable(sheet, payments);

            

            using (var stream = new MemoryStream())
            using (var file = File.OpenWrite($"Contingency-Output-R13-Payments-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx"))
            {
                excel.SaveAs(stream, true, true);
                Console.WriteLine("Saved to memory");
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(file);
            }
        }

        static void WritePaymentsToTable(IXLWorksheet sheet, List<Payment> payments)
        {
            var row = 2;
            var groupedEarnings = payments.GroupBy(x => new { x.Ukprn, x.FundingLineType, }).OrderBy(x => x.Key.Ukprn);
            foreach (var groupedEarning in groupedEarnings)
            {
                sheet.Cell(row, "A").Value = groupedEarning.Key.Ukprn;
                sheet.Cell(row, "B").Value = groupedEarning.Key.FundingLineType;
                sheet.Cell(row, "C").Value = groupedEarning.Sum(x => x.Amount);
                row++;
            }
        }
    }
}
