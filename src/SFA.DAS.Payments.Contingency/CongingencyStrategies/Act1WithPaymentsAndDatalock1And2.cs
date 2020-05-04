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
    class Act1WithPaymentsAndDatalock1And2 : IProduceContingencyPayments
    {
        public async Task GenerateContingencyPayments(int period)
        {
            List<Earning> earnings;
            List<Payment> payments;
            List<BasicV2Apprenticeship> basicV2Apprenticeships;

            Console.WriteLine("Processing in year ACT1...");

            // Load data
            using (var connection =
                new SqlConnection(ConfigurationManager.ConnectionStrings["ILR1920DataStore"].ConnectionString))
            {
                earnings = (await connection.QueryAsync<Earning>(Sql.YtdEarnings, new {collectionPeriod = period},
                    commandTimeout: 3600).ConfigureAwait(false)).ToList();
            }

            Console.WriteLine($"Loaded {earnings.Count} earnings");

            using (var connection =
                new SqlConnection(ConfigurationManager.ConnectionStrings["DASPayments"].ConnectionString))
            {
                payments = (await connection.QueryAsync<Payment>(Sql.YtdV2Payments, commandTimeout: 3600)
                    .ConfigureAwait(false)).ToList();
            }

            Console.WriteLine($"Loaded {payments.Count} payments");

            using (var connection =
                new SqlConnection(ConfigurationManager.ConnectionStrings["DASPayments"].ConnectionString))
            {
                basicV2Apprenticeships = (await connection
                    .QueryAsync<BasicV2Apprenticeship>(Sql.BasicV2Apprenticeships, commandTimeout: 3600)
                    .ConfigureAwait(false)).ToList();
            }

            Console.WriteLine($"Loaded {basicV2Apprenticeships.Count} V2 apprenticeships");
            
            var excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));

            // Filter out all non ACT1 earnings
            earnings = earnings
                .Where(x => x.ApprenticeshipContractType == 1)
                .ToList();
            Console.WriteLine($"Found {earnings.Count} ACT1 earnings");

            var earningsWithApprenticeships = DatalockCalculator
                .ApplyDatalocks(earnings, basicV2Apprenticeships);
            earningsWithApprenticeships.ForEach(x => x.Amount = x.AllTransactions);
            Console.WriteLine($"Found {earningsWithApprenticeships.Count} ACT1 " +
                              $"earnings after removing earnings without an apprenticeship");

            // Calculate Earnings - Payments
            var newPayments = PaymentsCalculator.Generate(earningsWithApprenticeships, payments);


            // Write raw earnings
            await AuditData.Output(earningsWithApprenticeships, $"ACT1EarningsByLearner-{DateTime.Now:yyyy-MM-dd-hh-mm}.csv");
            
            // And payments
            await AuditData.Output(newPayments, $"ACT1PaymentsByLearner-{DateTime.Now:yyyy-MM-dd-hh-mm}.csv");
            
            // Write payments summarised by UKPRN
            var sheet = excel.Worksheet("Final Payments");
            XlWriter.WriteToSummarisedTable(sheet, newPayments);

            // Summary
            sheet = excel.Worksheet("Summary");
            sheet.Cell(2, "A").Value = earningsWithApprenticeships.Select(x => x.Uln).Distinct().Count();
            sheet.Cell(2, "B").Value = earningsWithApprenticeships.Sum(x => x.Amount);


            using (var stream = new MemoryStream())
            using (var file = File.OpenWrite($"Contingency-Output-ACT1-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx"))
            {
                excel.SaveAs(stream, true, true);
                Console.WriteLine("Saved to memory");
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(file);
            }
        }
    }
}
