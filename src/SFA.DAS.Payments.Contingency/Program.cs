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
                List<Datalock> datalocks1819;
                List<Commitment> commitments;

                var act1CofundingSetting = ConfigurationManager.AppSettings["act1-cofunding"];
                var act1CofundingMultiplier = 0.95m;

                if (string.IsNullOrEmpty(act1CofundingSetting))
                {
                    Console.WriteLine("act1-cofunding setting not found, using 95% for ACT1 co-funding");
                }
                else
                {
                    var act1CoFundingMultiplier = decimal.Parse(act1CofundingSetting);
                    Console.WriteLine($"Using {act1CoFundingMultiplier * 100}% for ACT1 co-funding");
                }
                Console.WriteLine("NOTE that values for transaction types 1-3 have been reduced due to co-funding %");
                
                var selection = 0;

                while (selection < 1 || selection > 3)
                {
                    Console.WriteLine("Please choose the source data:");
                    Console.WriteLine("1. Transaction types 1 - 16");
                    Console.WriteLine("2. Transaction types 1 - 3");
                    Console.WriteLine("3. Transaction types 4 - 16");
                    var entry = Console.ReadLine();
                    int.TryParse(entry, out selection);
                }
                
                Console.WriteLine("Processing...");

                // Load data
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ILR1920DataStore"].ConnectionString))
                {
                    earnings = (await connection.QueryAsync<Earning>(Sql.Earnings, commandTimeout: 3600).ConfigureAwait(false)).ToList();
                }
                Console.WriteLine($"Loaded {earnings.Count} earnings");

                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DAS_CommitmentsReferenceData"].ConnectionString))
                {
                    commitments = (await connection.QueryAsync<Commitment>(Sql.Commitments1920, commandTimeout: 3600).ConfigureAwait(false)).ToList();
                }
                Console.WriteLine($"Loaded {commitments.Count} 1920 commitments");

                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DAS_PeriodEnd"].ConnectionString))
                {
                    datalocks1819 = (await connection.QueryAsync<Datalock>(Sql.Datalocks1819, commandTimeout: 3600).ConfigureAwait(false)).ToList();
                }
                Console.WriteLine($"Loaded {datalocks1819.Count} 1819 datalocks");

                
                // Apply co-funding multiplier
                earnings.ForEach(x =>
                {
                    if (x.ApprenticeshipContractType == 1)
                    {
                        x.TransactionType01 *= act1CofundingMultiplier;
                        x.TransactionType02 *= act1CofundingMultiplier;
                        x.TransactionType03 *= act1CofundingMultiplier;
                    }
                    else
                    {
                        x.TransactionType01 *= x.SfaContributionPercentage;
                        x.TransactionType02 *= x.SfaContributionPercentage;
                        x.TransactionType03 *= x.SfaContributionPercentage;
                    }
                });

                // Set the 'Amount'
                switch (selection)
                {
                    case 1: // all
                        earnings.ForEach(x => x.Amount = x.AllTransactions);
                        break;
                    case 2: // 1 - 3
                        earnings.ForEach(x => x.Amount = x.OneToThree);
                        break;
                    case 3: // 4 - 16
                        earnings.ForEach(x => x.Amount = x.Incentives);
                        break;
                }


                // Get all earnings
                // Write earnings to 'Earnings' tab
                var excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));

                var sheet = excel.Worksheet("Earnings");
                WriteToTable(sheet, earnings);
                Console.WriteLine("Written earnings page");

                // Extract ACT1 earnings for datalock processing
                var act1Earnings = earnings.Where(x => x.ApprenticeshipContractType == 1).ToList();
                var act2Earnings = earnings.Where(x => x.ApprenticeshipContractType == 2).ToList();
                var otherEarnings = earnings.Where(x => x.ApprenticeshipContractType != 1 && x.ApprenticeshipContractType != 2);
                
                Console.WriteLine($"Found {act1Earnings.Count} ACT1 earnings and {act2Earnings.Count} ACT2 earnings and {otherEarnings.Count()} other earnings");

                // Get 1819 datalocks
                // Merge R12 and R13 datalocks
                var datalocks = datalocks1819.Distinct().ToDictionary(x => (x.Ukprn, x.LearnRefNumber, x.AimSeqNumber));
                var earningsWithDatalocks = act1Earnings.Where(x => datalocks.ContainsKey((x.Ukprn, x.LearnRefNumber, x.AimSeqNumber))).ToList();
                var earningsWithoutDatalocks = act1Earnings.Where(x => !datalocks.ContainsKey((x.Ukprn, x.LearnRefNumber, x.AimSeqNumber))).ToList();

                // Write to '1819 Datalocks' tab
                sheet = excel.Worksheet("1819 Datalocks");
                WriteToTable(sheet, earningsWithDatalocks);
                Console.WriteLine($"Found {act1Earnings.Count} earnings with {earningsWithDatalocks.Count} 1819 datalocks and {earningsWithoutDatalocks.Count} remaining payable earnings");


                // Calculate 1920 datalocks
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
                        if (matchedCommitments.Any(x => x.Amount == earning.TotalPrice //&& 
                                                        //x.StartDate <= earning.EpisodeEffectiveTNPStartDate
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
                var paidEarnings = finalEarningsWithoutDatalocks.Union(act2Earnings).ToList();
                WriteToTable(sheet, paidEarnings);


                var datalockedUlns = earningsWithDatalocks.Select(x => x.Uln).Distinct().ToList();
                datalockedUlns.AddRange(finalEarningsWithDatalocks.Select(x => x.Uln));
                datalockedUlns = datalockedUlns.Distinct().ToList();

                Console.WriteLine($"{datalockedUlns.Count} datalocked learners");

                // Summary
                sheet = excel.Worksheet("Summary");
                sheet.Cell(2, "A").Value = earnings.Select(x => x.Uln).Distinct().Count();
                sheet.Cell(2, "B").Value = earnings.Sum(x => x.Amount);
                sheet.Cell(2, "C").Value = earningsWithDatalocks.Union(finalEarningsWithDatalocks).Sum(x => x.Amount);
                sheet.Cell(2, "D").Value = datalockedUlns.Count;
                sheet.Cell(2, "E").Value = paidEarnings.Select(x => x.Uln).Distinct().Count();

                // Raw earnings
                //sheet = excel.Worksheet("Raw Earnings");
                //WriteRawResults(sheet, earnings);


                using (var stream = new MemoryStream())
                using (var file = File.OpenWrite($"Earning-Output-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx"))
                {
                    excel.SaveAs(stream, true, true);
                    Console.WriteLine("Saved to memory");
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(file);
                }
                Console.WriteLine("Finished writing earnings output");


                // Reset XL
                excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));

                sheet = excel.Worksheet("Raw 1819 Datalocks");
                WriteRawResults(sheet, earningsWithDatalocks);

                sheet = excel.Worksheet("Raw 1920 Datalocks");
                WriteRawResults(sheet, finalEarningsWithDatalocks);

                using (var stream = new MemoryStream())
                using (var file = File.OpenWrite($"Datalock-Output-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx"))
                {
                    excel.SaveAs(stream, true, true);
                    Console.WriteLine("Saved to memory");
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(file);
                }
                
                Console.WriteLine("Finished - press enter to exit...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        static void WriteRawResults(IXLWorksheet sheet, List<Earning> earnings)
        {
            var row = 2;
            foreach (var earning in earnings)
            {
                sheet.Cell(row, "A").Value = earning.Ukprn;
                sheet.Cell(row, "B").Value = earning.Uln;
                sheet.Cell(row, "C").Value = earning.FundingLineType;

                sheet.Cell(row, "D").Value = earning.Amount;
                row++;
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
