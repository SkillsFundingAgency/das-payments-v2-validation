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
    class Act2WithPayments : IProduceContingencyPayments
    {
        public async Task GenerateContingencyPayments(int period)
        {
            List<Earning> earnings;
            List<Payment> payments;

            Console.WriteLine("Processing in year ACT2...");

            // Load data
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ILR1920DataStore"].ConnectionString))
            {
                earnings = (await connection.QueryAsync<Earning>(Sql.YtdEarnings, new {collectionPeriod = period}, 
                    commandTimeout: 3600).ConfigureAwait(false)).ToList();
            }
            Console.WriteLine($"Loaded {earnings.Count} earnings");

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DASPayments"].ConnectionString))
            {
                payments = (await connection.QueryAsync<Payment>(Sql.YtdV2Payments, commandTimeout: 3600)
                    .ConfigureAwait(false)).ToList();
            }
            Console.WriteLine($"Loaded {payments.Count} payments");

            var excel = new XLWorkbook(Path.Combine("Template", "Contingency.xlsx"));

            // Filter out all ACT1 earnings
            earnings = earnings
                .Where(x => x.ApprenticeshipContractType == 2)
                .ToList();
            
            // Apply co-funding multiplier
            earnings.ForEach(x =>
            {
                x.TransactionType01 *= x.SfaContributionPercentage;
                x.TransactionType02 *= x.SfaContributionPercentage;
                x.TransactionType03 *= x.SfaContributionPercentage;
            });

            var rawEarnings = earnings.ToList();
            rawEarnings.ForEach(x => x.Amount = x.AllTransactions);
            
            // Calculate Earnings - Payments
            var newPayments = PaymentsCalculator.Generate(rawEarnings, payments);


            // Write raw earnings
            var sheet = excel.Worksheet("Raw Earnings");
            //Program.WriteToTable(sheet, rawEarnings);
            Console.WriteLine($"Found {earnings.Count} ACT2 earnings");

            // Write raw payments
            sheet = excel.Worksheet("Raw Payments");
            //Program.WriteRawResults(sheet, newPayments);

            // Write payments summarised by UKPRN
            sheet = excel.Worksheet("Final Payments");
            Program.WriteToSummarisedTable(sheet, newPayments);

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
