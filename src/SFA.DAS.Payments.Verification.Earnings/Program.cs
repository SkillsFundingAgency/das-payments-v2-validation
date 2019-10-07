using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Payments.Verification.Earnings.Constants;
using SFA.DAS.Payments.Verification.Earnings.DTO;
using SFA.DAS.Payments.Verification.Earnings.Extensions;

namespace SFA.DAS.Payments.Verification.Earnings
{
    class Program
    {
        static HashSet<int> TransactionTypesToCompare = new HashSet<int> { 1, 2, 3, };
        static async Task Main(string[] args)
        {
            try
            {
                Log.Initialise();
                Log.Write("Starting comparison");

                var earnings = await Sql.Read<Earning>(PaymentSystem.Earnings, Script.Earnings);
                Log.Write($"Read {earnings.Count} earnings");
                var payments = await Sql.Read<Payment>(PaymentSystem.Payments, Script.Payments);
                Log.Write($"Read {payments.Count} payments");

                var earningsForComparison = earnings.ToComparisonClass();
                var paymentsForComparison = payments.ToComparisonClass();

                earningsForComparison = earningsForComparison
                    .Where(x => TransactionTypesToCompare.Contains(x.TransactionType))
                    .Where(x => x.ContractType == 1)
                    .GroupBy(x => new { x.Ukprn, x.DeliveryPeriod, x.TransactionType })
                    .Select(x => new ComparisonClass
                    {
                        Amount = x.Sum(y => y.Amount),
                        ContractType = 0,
                        TransactionType = x.Key.TransactionType,
                        DeliveryPeriod = x.Key.DeliveryPeriod,
                        Ukprn = x.Key.Ukprn,
                        Uln = 0,
                        FundingLineType = string.Empty,
                    })
                    .Where(x => x.Amount != 0)
                    .ToList();

                paymentsForComparison = paymentsForComparison
                    .Where(x => TransactionTypesToCompare.Contains(x.TransactionType))
                    .GroupBy(x => new { x.Ukprn, x.DeliveryPeriod, x.TransactionType })
                    .Select(x => new ComparisonClass
                    {
                        Amount = x.Sum(y => y.Amount),
                        ContractType = 0,
                        TransactionType = x.Key.TransactionType,
                        DeliveryPeriod = x.Key.DeliveryPeriod,
                        Ukprn = x.Key.Ukprn,
                        Uln = 0,
                        FundingLineType = string.Empty,
                    })
                    .ToList();

                var paymentsWithoutEarnings = paymentsForComparison.Except(earningsForComparison).ToList();
                Log.Write($"Found {paymentsWithoutEarnings.Count} payments without earnings");

                var earningsWithoutPayments = earningsForComparison.Except(paymentsForComparison).ToList();
                Log.Write($"Found {earningsWithoutPayments.Count} earnings without payments");

                var matched = earningsForComparison.Intersect(paymentsForComparison).ToList();
                Log.Write($"Found {matched.Count} matched records");

                var outputRows = earningsWithoutPayments.Union(paymentsWithoutEarnings)
                    .GroupBy(x => new { x.Ukprn, x.DeliveryPeriod })
                    .Select(x => new OutputRow
                    {
                        Ukprn = x.Key.Ukprn,
                        Earnings = earningsWithoutPayments
                            .Where(y => y.Ukprn == x.Key.Ukprn)
                            .Where(y => y.DeliveryPeriod == x.Key.DeliveryPeriod)
                            .Sum(z => z.Amount),
                        Payments = paymentsWithoutEarnings
                            .Where(y => y.Ukprn == x.Key.Ukprn)
                            .Where(y => y.DeliveryPeriod == x.Key.DeliveryPeriod)
                            .Sum(z => z.Amount),
                        Period = x.Key.DeliveryPeriod,
                    })
                    .ToList();


                var filename = $"Earning Verification Results {DateTime.Now:yyyy-MM-dd hh-mm}.xlsx";
                using (var dataStream = Excel.CreateExcelDocumentWithSheets(
                    (paymentsWithoutEarnings, "Payments without earnings"),
                    (earningsWithoutPayments, "Earnings without payments"),
                    (outputRows, "Collated Output")
                ))
                using (var file = File.Create(filename))
                {
                    dataStream.CopyTo(file);
                }

                Log.Write("Completed");

            }
            catch (Exception e)
            {
                Log.Write(e.Message);
                throw;
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

        }
    }
}
