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
    class Act2WithPayments : IProduceContingencyPayments, IDisposable
    {
        public void Dispose()
        {}

        public async Task GenerateContingencyPayments(int period)
        {
            List<Earning> earnings;
            List<Payment> payments;

            Console.WriteLine("Processing in year ACT2...");

            // Load data
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ILR1920DataStore"].ConnectionString))
            {
                earnings = (await connection.QueryAsync<Earning>(Sql.YtdEarnings, new {collectionPeriod = period, act = 2}, 
                    commandTimeout: 3600).ConfigureAwait(false))
                        .Where(x => x.ApprenticeshipContractType == 2)
                        .ToList();
            }
            Console.WriteLine($"Loaded {earnings.Count} earnings");

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DASPayments"].ConnectionString))
            {
                payments = (await connection.QueryAsync<Payment>(Sql.YtdV2Payments, new { collectionPeriod = period, act = 2 },
                    commandTimeout: 3600)
                        .ConfigureAwait(false))
                        .ToList();
            }
            Console.WriteLine($"Loaded {payments.Count} payments");

            var excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));

            GC.Collect();

            // Apply co-funding multiplier
            earnings.ForEach(x =>
            {
                x.TransactionType01 *= x.SfaContributionPercentage;
                x.TransactionType02 *= x.SfaContributionPercentage;
                x.TransactionType03 *= x.SfaContributionPercentage;
            });

            earnings.ForEach(x => x.Amount = x.AllTransactions);
            Console.WriteLine($"Found {earnings.Count} ACT2 earnings");

            GC.Collect();

            // Calculate Earnings - Payments
            var newPayments = PaymentsCalculator.Generate(earnings, payments);

            // Write raw earnings
            await AuditData.Output(earnings, $"ACT2EarningsByLearner-{DateTime.Now:yyyy-MM-dd-hh-mm}.csv");

            // And payments
            await AuditData.Output(newPayments, $"ACT2PaymentsByLearner-{DateTime.Now:yyyy-MM-dd-hh-mm}.csv");

            
            // Write payments summarised by UKPRN
            var sheet = excel.Worksheet("Final Payments");
            XlWriter.WriteToSummarisedTable(sheet, newPayments);

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
