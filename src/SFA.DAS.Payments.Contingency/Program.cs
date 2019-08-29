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

namespace SFA.DAS.Payments.Contingency
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                List<Earning> earnings;
                List<Datalock> datalocks1819R12;
                List<Datalock> datalocks1819R13;
                List<Commitment> commitments;

                // Load data
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ILR1920DataStore"].ConnectionString))
                {
                    earnings = (await connection.QueryAsync<Earning>(Sql.Earnings, commandTimeout: 3600).ConfigureAwait(false)).ToList();
                }
                Console.WriteLine($"Loaded {earnings.Count} earnings");

                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DasPayments"].ConnectionString))
                {
                    commitments = (await connection.QueryAsync<Commitment>(Sql.Commitments1920, commandTimeout: 3600).ConfigureAwait(false)).ToList();
                }
                Console.WriteLine($"Loaded {commitments.Count} 1920 commitments");

                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DS_SILR1819_Collection"].ConnectionString))
                {
                    datalocks1819R13 = (await connection.QueryAsync<Datalock>(Sql.Datalocks1819R13, commandTimeout: 3600).ConfigureAwait(false)).ToList();
                }
                Console.WriteLine($"Loaded {datalocks1819R13.Count} R13 datalocks");


                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DAS_PeriodEnd"].ConnectionString))
                {
                    datalocks1819R12 = (await connection.QueryAsync<Datalock>(Sql.Datalocks1819R12, commandTimeout: 3600).ConfigureAwait(false)).ToList();
                }
                Console.WriteLine($"Loaded {datalocks1819R12.Count} R12 datalocks");

                var excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));


                // Get all earnings
                // Write earnings to 'Earnings' tab

                var sheet = excel.Worksheet("Earnings");
                WriteToTable(sheet, earnings);
                Console.WriteLine("Written earnings page");


                // Get 1819 datalocks
                // Merge R12 and R13 datalocks
                var datalocks = datalocks1819R12.Union(datalocks1819R13).Distinct().ToDictionary(x => (x.Ukprn, x.LearnRefNumber, x.AimSeqNumber));
                var earningsWithDatalocks = earnings.Where(x => datalocks.ContainsKey((x.Ukprn, x.LearnRefNumber, x.AimSeqNumber))).ToList();
                var earningsWithoutDatalocks = earnings.Where(x => !datalocks.ContainsKey((x.Ukprn, x.LearnRefNumber, x.AimSeqNumber))).ToList();

                // Write to '1819 Datalocks' tab
                sheet = excel.Worksheet("1819 Datalocks");
                WriteToTable(sheet, earningsWithDatalocks);
                Console.WriteLine($"Found {earnings.Count} earnings with {earningsWithDatalocks.Count} datalocks and {earningsWithoutDatalocks.Count} payable earnings");


                // Get 1920 datalocks
                var searchableCommitments = commitments.ToLookup(x => 
                    (x.Ukprn, x.Uln, x.FrameworkCode, x.PathwayCode, x.ProgrammeType, x.StandardCode));
                var finalEarningsWithDatalocks = new List<Earning>();
                var finalEarningsWithoutDatalocks = new List<Earning>();

                foreach (var earning in earningsWithoutDatalocks)
                {
                    if (searchableCommitments.Contains(
                        (earning.Ukprn, earning.Uln, earning.FrameworkCode, earning.PathwayCode, earning.ProgrammeType, earning.StandardCode)))
                    {
                        if (earning.MathsAndEnglish)
                        {
                            finalEarningsWithoutDatalocks.Add(earning);
                            continue;
                        }
                        
                        var matchedCommitments = searchableCommitments[
                            (earning.Ukprn, earning.Uln, earning.FrameworkCode, earning.PathwayCode, earning.ProgrammeType, earning.StandardCode)];
                        if (matchedCommitments.Any(x => x.Amount == earning.TotalPrice && 
                                                        x.StartDate <= earning.EpisodeEffectiveTNPStartDate
                                                        ))
                        {
                            finalEarningsWithoutDatalocks.Add(earning);
                        }
                        else
                        {
                            finalEarningsWithDatalocks.Add(earning);
                        }
                    }
                    else
                    {
                        finalEarningsWithDatalocks.Add(earning);
                    }
                }

                // Write the remainder of the datalocks to '1920 Datalocks' tab
                sheet = excel.Worksheet("1920 Datalocks");
                WriteToTable(sheet, finalEarningsWithDatalocks);
                Console.WriteLine($"Found {finalEarningsWithoutDatalocks.Count} remaining earnings with {finalEarningsWithDatalocks.Count} 1920 datalocks");


                // Write a summary tab
                sheet = excel.Worksheet("Final Amounts");
                WriteToTable(sheet, finalEarningsWithoutDatalocks);


                var datalockedUlns = earningsWithDatalocks.Select(x => x.Uln).Distinct().ToList();
                datalockedUlns.AddRange(finalEarningsWithDatalocks.Select(x => x.Uln));
                datalockedUlns = datalockedUlns.Distinct().ToList();

                Console.WriteLine($"{datalockedUlns.Count} datalocked learners");


                sheet = excel.Worksheet("Summary");
                sheet.Cell(2, "A").Value = earnings.Select(x => x.Uln).Distinct().Count();
                sheet.Cell(2, "B").Value = earnings.Sum(x => x.Amount);
                sheet.Cell(2, "C").Value = earningsWithDatalocks.Union(finalEarningsWithDatalocks).Sum(x => x.Amount);
                sheet.Cell(2, "D").Value = earningsWithDatalocks.Union(finalEarningsWithDatalocks).Select(x => x.Uln)
                    .Distinct().Count();
                sheet.Cell(2, "E").Value = finalEarningsWithoutDatalocks.Select(x => x.Uln).Distinct().Count();

                excel.SaveAs($"Output-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx", true, true);


                Console.WriteLine("Finished - press enter to exit...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        static void WriteToTable(IXLWorksheet sheet, List<Earning> earnings)
        {
            var row = 2;
            var groupedEarnings = earnings.GroupBy(x => new { x.Ukprn, x.FundingLineType });
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
