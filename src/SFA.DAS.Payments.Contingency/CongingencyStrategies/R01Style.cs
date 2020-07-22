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
    class R01Style : IProduceContingencyPayments
    {
        public async Task GenerateContingencyPayments(int period)
        {
            List<Earning> earnings;
            List<Datalock> datalocks1819;
            List<Commitment> commitments;

            Console.WriteLine("Processing R01 style contingency...");

            // Load data
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ILR1920DataStore"].ConnectionString))
            {
                earnings = (await connection.QueryAsync<Earning>(Sql.PeriodEarnings, new {collectionperiod=period}, commandTimeout: 3600).ConfigureAwait(false)).ToList();
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
            var excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));

            var sheet = excel.Worksheet("Earnings");
            XlWriter.WriteToTable(sheet, earnings);
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
            XlWriter.WriteToTable(sheet, earningsWithDatalocks);
            Console.WriteLine($"Found {act1Earnings.Count} earnings with {earningsWithDatalocks.Count} 1819 datalocks and {earningsWithoutDatalocks.Count} remaining payable earnings");


            // Calculate 1920 datalocks
            var searchableCommitments = commitments.ToLookup(x =>
                (x.Ukprn, x.Uln, x.FrameworkCode, x.PathwayCode, x.ProgrammeType, x.StandardCode));
            var finalEarningsWithDatalocks = new List<Earning>();
            var finalEarningsWithoutDatalocks = new List<Earning>();
            var finalEarningsWithPartialDatalocks = new List<Earning>();
            var finalEarningsWithoutPartialDatalocks = new List<Earning>();
            var searchablePartCommitments = commitments.ToLookup(x => (x.Ukprn, x.Uln));

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
            sheet = excel.Worksheet("1920 Datalocks (Full)");
            XlWriter.WriteToTable(sheet, finalEarningsWithDatalocks);
            Console.WriteLine($"Found {finalEarningsWithoutDatalocks.Count} remaining earnings with {finalEarningsWithDatalocks.Count} 1920 datalocks (full match)");


            foreach (var earning in earningsWithoutDatalocks)
            {
                if (searchablePartCommitments.Contains((earning.Ukprn, earning.Uln)))
                {
                    finalEarningsWithoutPartialDatalocks.Add(earning);
                }
                else
                {
                    finalEarningsWithPartialDatalocks.Add(earning);
                }
            }
            // Write the remainder of the datalocks to '1920 Datalocks' tab
            sheet = excel.Worksheet("1920 Datalocks (Partial)");
            XlWriter.WriteToTable(sheet, finalEarningsWithPartialDatalocks);
            Console.WriteLine($"Found {finalEarningsWithoutPartialDatalocks.Count} remaining earnings with {finalEarningsWithPartialDatalocks.Count} 1920 datalocks (partial match)");



            // Write a summary tab
            sheet = excel.Worksheet("Final Amounts (Full)");
            var paidEarnings = finalEarningsWithoutDatalocks.Union(act2Earnings).ToList();
            XlWriter.WriteToTable(sheet, paidEarnings);

            // Write a summary tab
            sheet = excel.Worksheet("Final Amounts (Partial)");
            paidEarnings = finalEarningsWithoutPartialDatalocks.Union(act2Earnings).ToList();
            XlWriter.WriteToTable(sheet, paidEarnings);


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
            sheet.Cell(2, "F").Value = finalEarningsWithDatalocks.Select(x => x.Uln).Distinct().Count();
            sheet.Cell(2, "G").Value = finalEarningsWithPartialDatalocks.Select(x => x.Uln).Distinct().Count();
            sheet.Cell(2, "H").Value = finalEarningsWithDatalocks.Sum(x => x.Amount);
            sheet.Cell(2, "I").Value = finalEarningsWithPartialDatalocks.Sum(x => x.Amount);

            // Raw earnings
            sheet = excel.Worksheet("Raw Earnings");
            WriteRawResults(sheet, earnings);


            sheet = excel.Worksheet("Raw 1819 Datalocks");
            WriteRawResults(sheet, earningsWithDatalocks);

            sheet = excel.Worksheet("Raw 1920 Datalocks (Full)");
            WriteRawResults(sheet, finalEarningsWithDatalocks);

            sheet = excel.Worksheet("Raw 1920 Datalocks (Partial)");
            WriteRawResults(sheet, finalEarningsWithPartialDatalocks);


            using (var stream = new MemoryStream())
            using (var file = File.OpenWrite($"Contingency-Output-R01-Style-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx"))
            {
                excel.SaveAs(stream, true, true);
                Console.WriteLine("Saved to memory");
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(file);
            }
        }

        private static void WriteRawResults(IXLWorksheet sheet, List<Earning> earnings)
        {
            var row = 2;
            foreach (var earning in earnings.OrderBy(x => x.Ukprn).ThenBy(x => x.Uln))
            {
                sheet.Cell(row, "A").Value = earning.Ukprn;
                sheet.Cell(row, "B").Value = earning.Uln;
                sheet.Cell(row, "C").Value = earning.FundingLineType;

                sheet.Cell(row, "D").Value = earning.Amount;
                sheet.Cell(row, "E").Value = earning.OneToThree;
                sheet.Cell(row, "F").Value = earning.Incentives;
                row++;
            }
        }
    }
}
