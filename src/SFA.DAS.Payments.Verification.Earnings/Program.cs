using System;
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

                var paymentsWithoutEarnings = paymentsForComparison.Except(earningsForComparison).ToList();
                Log.Write($"Found {paymentsWithoutEarnings.Count} payments without earnings");

                var earningsWithoutPayments = earningsForComparison.Except(paymentsForComparison).ToList();
                Log.Write($"Found {earningsWithoutPayments.Count} earnings without payments");

                var matched = earningsForComparison.Intersect(paymentsForComparison).ToList();
                Log.Write($"Found {matched.Count} matched records");

                var filename = $"Earning Verification Results {DateTime.Now:yyyy-MM-dd hh-mm}.xlsx";
                using (var dataStream = Excel.CreateExcelDocumentWithSheets(
                    (paymentsWithoutEarnings, "Payments without earnings"),
                    (earningsWithoutPayments, "Earnings without payments")
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
