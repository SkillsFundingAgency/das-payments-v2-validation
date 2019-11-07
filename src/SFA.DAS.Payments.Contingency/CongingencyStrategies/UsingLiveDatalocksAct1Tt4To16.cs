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
    class UsingLiveDatalocksAct1Tt4To16 : IProduceContingencyPayments
    {
        public async Task GenerateContingencyPayments()
        {
            List<Earning> earnings;
            List<V2Datalock> v2Datalocks;

            Console.WriteLine("Processing ACT1 TT4-16 ...");

            // Load data
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ILR1920DataStore"].ConnectionString))
            {
                earnings = (await connection.QueryAsync<Earning>(Sql.Earnings, commandTimeout: 3600).ConfigureAwait(false)).ToList();
            }
            Console.WriteLine($"Loaded {earnings.Count} earnings");

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DASPayments"].ConnectionString))
            {
                v2Datalocks = (await connection.QueryAsync<V2Datalock>(Sql.V2Datalocks, commandTimeout: 3600)
                    .ConfigureAwait(false)).ToList();
            }
            Console.WriteLine($"Loaded {v2Datalocks.Count} V2 datalocks");


            // Filter results
            earnings = earnings
                .Where(x => x.ApprenticeshipContractType == 1)
                .ToList();


            // Apply co-funding multiplier
            earnings.ForEach(x =>
            {
                x.TransactionType01 = 0;
                x.TransactionType02 = 0;
                x.TransactionType03 = 0;
            });

            // Set the 'Amount'
            earnings.ForEach(x => x.Amount = x.AllTransactions);

            earnings = earnings.Where(x => x.TransactionType01 != 0 ||
                                           x.TransactionType02 != 0 ||
                                           x.TransactionType03 != 0 ||
                                           x.TransactionType04 != 0 ||
                                           x.TransactionType05 != 0 ||
                                           x.TransactionType06 != 0 ||
                                           x.TransactionType07 != 0 ||
                                           x.TransactionType08 != 0 ||
                                           x.TransactionType09 != 0 ||
                                           x.TransactionType10 != 0 ||
                                           x.TransactionType11 != 0 ||
                                           x.TransactionType12 != 0 ||
                                           x.TransactionType13 != 0 ||
                                           x.TransactionType14 != 0 ||
                                           x.TransactionType15 != 0 ||
                                           x.TransactionType16 != 0)
                .ToList();

            // Get all earnings
            // Write earnings to 'Earnings' tab
            var excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));

            var sheet = excel.Worksheet("Earnings");
            Program.WriteToTable(sheet, earnings);
            Console.WriteLine("Written earnings page");

            // Extract ACT1 earnings for datalock processing
            var act1Earnings = earnings.Where(x => x.ApprenticeshipContractType == 1).ToList();
          
            Console.WriteLine($"Found {act1Earnings.Count} ACT1 earnings");

            // Calculate datalocks
            var searchableFailedDatalocks = v2Datalocks.ToLookup(x =>
                (x.Ukprn, x.Uln, x.FrameworkCode, x.PathwayCode, x.ProgrammeType, x.StandardCode, x.DeliveryPeriod));
            var finalEarningsWithDatalocks = new List<Earning>();
            var finalEarningsWithoutDatalocks = new List<Earning>();
            
            foreach (var earning in act1Earnings)
            {
                if (searchableFailedDatalocks.Contains(
                    (earning.Ukprn, earning.Uln, earning.FrameworkCode, earning.PathwayCode, earning.ProgrammeType,
                        earning.StandardCode,
                        (byte) earning.Period)))
                {
                    finalEarningsWithDatalocks.Add(earning);
                }
                else
                {
                    finalEarningsWithoutDatalocks.Add(earning);
                }
            }


            // Write the remainder of the datalocks to '1920 Datalocks' tab
            sheet = excel.Worksheet("1920 Datalocks (Full)");
            Program.WriteToTable(sheet, finalEarningsWithDatalocks);
            Console.WriteLine($"Found {finalEarningsWithoutDatalocks.Count} remaining earnings with {finalEarningsWithDatalocks.Count} 1920 datalocks (full match)");


            
            // Write a summary tab
            sheet = excel.Worksheet("Final Amounts (Full)");
            Program.WriteToTable(sheet, finalEarningsWithoutDatalocks);

            

            var datalockedUlns = finalEarningsWithDatalocks.Select(x => x.Uln).Distinct().ToList();
            datalockedUlns = datalockedUlns.Distinct().ToList();

            Console.WriteLine($"{datalockedUlns.Count} datalocked learners");

            // Summary
            sheet = excel.Worksheet("Summary");
            sheet.Cell(2, "A").Value = earnings.Select(x => x.Uln).Distinct().Count();
            sheet.Cell(2, "B").Value = earnings.Sum(x => x.Amount);

            sheet.Cell(2, "D").Value = datalockedUlns.Count;
            
            sheet.Cell(2, "F").Value = finalEarningsWithDatalocks.Select(x => x.Uln).Distinct().Count();
            
            sheet.Cell(2, "H").Value = finalEarningsWithDatalocks.Sum(x => x.Amount);
            sheet.Cell(2, "H").Value = finalEarningsWithoutDatalocks.Sum(x => x.Amount);


            // Raw earnings
            //sheet = excel.Worksheet("Raw Earnings");
            //Program.WriteRawResults(sheet, earnings);

            //sheet = excel.Worksheet("Raw 1920 Datalocks (Full)");
            //Program.WriteRawResults(sheet, finalEarningsWithDatalocks);

            

            using (var stream = new MemoryStream())
            using (var file = File.OpenWrite($"Contingency-Output-Live-Datalocks-TT4-TT16-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx"))
            {
                excel.SaveAs(stream, true, true);
                Console.WriteLine("Saved to memory");
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(file);
            }
        }
    }
}
