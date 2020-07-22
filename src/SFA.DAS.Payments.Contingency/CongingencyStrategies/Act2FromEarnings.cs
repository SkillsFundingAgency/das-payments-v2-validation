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
    class Act2FromEarnings : IProduceContingencyPayments
    {
        public async Task GenerateContingencyPayments(int period)
        {
            List<Earning> earnings;
            
            Console.WriteLine("Processing ACT2...");

            // Load data
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ILR1920DataStore"].ConnectionString))
            {
                earnings = (await connection.QueryAsync<Earning>(Sql.PeriodEarnings, new {collectionperiod=period}, commandTimeout: 3600)
                    .ConfigureAwait(false)).ToList();
            }
            Console.WriteLine($"Loaded {earnings.Count} earnings");

            
            var excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));

            // Filter out all ACT1 earnings
            earnings = earnings
                .Where(x => x.ApprenticeshipContractType == 2)
                .ToList();
            
            var rawEarnings = earnings.ToList();
            rawEarnings.ForEach(x => x.Amount = x.AllTransactions);
            // Write earnings tab
            var sheet = excel.Worksheet("Earnings");
            XlWriter.WriteToTable(sheet, rawEarnings);


            // Apply co-funding multiplier
            earnings.ForEach(x =>
            {
                if (x.ApprenticeshipContractType == 2)
                {
                    x.TransactionType01 *= x.SfaContributionPercentage;
                    x.TransactionType02 *= x.SfaContributionPercentage;
                    x.TransactionType03 *= x.SfaContributionPercentage;
                }
            });

            // Set the 'Amount'
            earnings.ForEach(x => x.Amount = x.AllTransactions);


            // Get all earnings
            // Write earnings to 'Earnings' tab
            
            Console.WriteLine($"Found {earnings.Count} ACT2 earnings");

            // Write a summary tab
            sheet = excel.Worksheet("Final Amounts (Full)");
            XlWriter.WriteToTable(sheet, earnings);

            
            // Summary
            sheet = excel.Worksheet("Summary");
            sheet.Cell(2, "A").Value = earnings.Select(x => x.Uln).Distinct().Count();
            sheet.Cell(2, "B").Value = earnings.Sum(x => x.Amount);

            

            using (var stream = new MemoryStream())
            using (var file = File.OpenWrite($"Contingency-Output-ACT2-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx"))
            {
                excel.SaveAs(stream, true, true);
                Console.WriteLine("Saved to memory");
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(file);
            }
        }
    }
}
