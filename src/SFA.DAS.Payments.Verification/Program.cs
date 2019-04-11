using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Payments.Verification.Constants;
using SFA.DAS.Payments.Verification.DTO;

namespace SFA.DAS.Payments.Verification
{
    static class Program
    {
        private static HashSet<long> _activeLearners;

        static async Task Main(string[] args)
        {
            Log.Initialise();
            
            // Get the list of learners that we are interested in
            await InitialiseActiveLearners();
            Log.Write("Initialised learner group");

            // Get the payments
            var v1Payments = await Sql.Read<Payment>(PaymentSystem.V1, Script.Payments);
            //v1Payments = v1Payments.LimitToActiveLearners();
            Log.Write("Retrieved V1 Payments");

            var v2Payments = await Sql.Read<Payment>(PaymentSystem.V2, Script.Payments);
            //v2Payments = v2Payments.LimitToActiveLearners();
            Log.Write("Retrieved V2 Payments");

            var v1PaymentsWithoutV2 = v1Payments.Except(v2Payments);
            var v2PaymentsWithoutV1 = v2Payments.Except(v1Payments);
            var commonPayments = v1Payments.Intersect(v2Payments);
            Log.Write("Payment comparison complete");

            // Get the earnings
            var v1Earnings = await Sql.Read<Earning>(PaymentSystem.V1, Script.Earnings);
            v1Earnings = v1Earnings.LimitToActiveLearners();
            Log.Write("Retrieved V1 Earnings");

            var v2Earnings = await Sql.Read<Earning>(PaymentSystem.V2, Script.Earnings);
            v2Earnings = v2Earnings.LimitToActiveLearners();
            Log.Write("Retrieved V2 Earnings");

            var v1EarningsWithoutV2 = v1Earnings.Except(v2Earnings);
            var v2EarningsWithoutV1 = v2Earnings.Except(v1Earnings);
            var commonEarnings = v1Earnings.Intersect(v2Earnings);
            Log.Write("Earning comparison complete");

            // Get the required payments
            var v1RequiredPayments = await Sql.Read<RequiredPayment>(PaymentSystem.V1, Script.RequiredPayments);
            v1RequiredPayments = v1RequiredPayments.LimitToActiveLearners();
            Log.Write("Retrieved V1 Required Payments");

            var v2RequiredPayments = await Sql.Read<RequiredPayment>(PaymentSystem.V2, Script.RequiredPayments);
            v2RequiredPayments = v2RequiredPayments.LimitToActiveLearners();
            Log.Write("Retrieved V2 Required Payments");

            var v1RequiredPaymentsWithoutV2 = v1RequiredPayments.Except(v2RequiredPayments);
            var v2RequiredPaymentsWithoutV1 = v2RequiredPayments.Except(v1RequiredPayments);
            var commonRequiredPayments = v1RequiredPayments.Intersect(v2RequiredPayments);
            Log.Write("Required Payments comparison complete");

            // For V1 payments without V2 - are the earnings the same?



            // High level summary
            var v1PaymentsByTransactionType = v1Payments.ToLookup(x => x.TransactionType);
            var v2PaymentsByTransactionType = v2Payments.ToLookup(x => x.TransactionType);
            var v1RequiredPaymentsByTransactionType = v1RequiredPayments.ToLookup(x => x.TransactionType);
            var v2RequiredPaymentsByTransactionType = v2RequiredPayments.ToLookup(x => x.TransactionType);
            var summary = new List<HighLevelSummary>();

            // For each transaction type
            for (int i = 1; i < 17; i++)
            {
                // Create a new row
                summary.Add(new HighLevelSummary
                {
                    TransactionType = i,
                    // Aggregate all amounts for this transaction type
                    V1PaymentsAmount = v1PaymentsByTransactionType[i].Sum(x => x.Amount), 
                    V2PaymentsAmount = v2PaymentsByTransactionType[i].Sum(x => x.Amount),
                    V1RequiredPaymentsAmount = v1RequiredPaymentsByTransactionType[i].Sum(x => x.Amount),
                    V2RequiredPaymentsAmount = v2RequiredPaymentsByTransactionType[i].Sum(x => x.Amount),
                });
            }

            using (var dataStream = Excel.CreateExcelDocumentWithSheets(
                (summary, "High Level Summary"),
                (v1PaymentsWithoutV2, "V1 Payments without V2"),
                (v2PaymentsWithoutV1, "V2 Payments without V1"),
                (commonPayments, "Common Payments"),
                (v1EarningsWithoutV2, "V1 Earnings without V2"),
                (v2EarningsWithoutV1, "V2 Earnings without V1"),
                (commonEarnings, "Common Earnings"),
                (v1RequiredPaymentsWithoutV2, "V1 Required Payments without V2"),
                (v2RequiredPaymentsWithoutV1, "V2 Required Payments without V1"),
                (commonRequiredPayments, "Common Required Payments")
                ))
            using (var file = File.Create("V2 Verification Results.xlsx"))
            {
                dataStream.CopyTo(file);
            }

            Log.Write("Complete");
            Console.ReadKey();
        }

        private static async Task InitialiseActiveLearners()
        {
            _activeLearners = new HashSet<long>(await Sql.Read<long>(PaymentSystem.V1, Script.Inclusions));
        }
        
        private static List<T> LimitToActiveLearners<T>(this IEnumerable<T> source) where T : IContainLearnerDetails
        {
            return source.Where(x => _activeLearners.Contains(x.LearnerUln)).ToList();
        }
    }
}
