using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastMember;
using MoreLinq;
using SFA.DAS.Payments.Verification.Constants;
using SFA.DAS.Payments.Verification.DTO;

namespace SFA.DAS.Payments.Verification
{
    static class Program
    {
        private static List<int> _periods = new List<int>{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
        private static readonly List<long> Ukprns = new List<long>();

        static async Task Main(string[] args)
        {
            try
            {
                Log.Initialise();
                InitialiseSavedSettings();

                var readyToProcess = false;

                while (!readyToProcess)
                {
                    // Get the list of learners that we are interested in
                    Console.Clear();
                    Console.WriteLine("Payments V2 Comparison Tool");
                    Console.WriteLine("---------------------------");
                    Console.WriteLine();

                    Console.WriteLine($"Processing for periods: {string.Join(",", _periods)}");
                    Console.WriteLine();

                    if (Ukprns.Any())
                    {
                        Console.WriteLine("Resticted to the following UKPRNs:");
                        foreach (var ukprn in Ukprns)
                        {
                            Console.WriteLine($"\t{ukprn}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Processing all UKPRNs");
                    }
                    Console.WriteLine();

                    Console.WriteLine("Please select an option:");
                    Console.WriteLine("");
                    Console.WriteLine("\t1. Process All learners");
                    Console.WriteLine("\t2. Process ACT2 Basic Day");
                    Console.WriteLine("\t3. Process ACT2 with Refunds");
                    Console.WriteLine("\t4. Process ACT2 with Incentives");
                    Console.WriteLine();
                    Console.WriteLine("\t8 add specific UKPRN");
                    Console.WriteLine("\t9 use a specific period");

                    var key = Console.ReadKey(true);
                    Console.WriteLine();
                    Log.Write($"{key.KeyChar} pressed. Processing...");

                    switch (key.Key)
                    {
                        case ConsoleKey.D1:
                            await InitialiseActiveLearners(Inclusions.AllLearners);
                            readyToProcess = true;
                            Log.Write("Initialised learner group");
                            break;
                        case ConsoleKey.D2:
                            await InitialiseActiveLearners(Inclusions.Act2BasicDay);
                            readyToProcess = true;
                            Log.Write("Initialised learner group");
                            break;
                        case ConsoleKey.D3:
                            await InitialiseActiveLearners(Inclusions.Act2Refund);
                            readyToProcess = true;
                            Log.Write("Initialised learner group");
                            break;
                        case ConsoleKey.D4:
                            await InitialiseActiveLearners(Inclusions.Act2Incentives);
                            readyToProcess = true;
                            Log.Write("Initialised learner group");
                            break;

                        case ConsoleKey.D8:
                            AddUkprn();
                            break;

                        case ConsoleKey.D9:
                            UpdatePeriods();
                            break;

                        default:
                            Console.WriteLine("Unknown response, exiting when you press any key...");
                            Console.ReadKey();
                            return;
                    }
                }

                var jobId = await Sql.InitialiseJob();

                await ProcessComparison(jobId);
                
                Log.Write($"Job complete with JobId: {jobId}, press any key to close");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void InitialiseSavedSettings()
        {
            var list = Config.UkprnList.Split(',');
            foreach (var ukprn in list)
            {
                SetUkprn(ukprn);
            }

            SetPeriodsFromString(Config.PeriodList);
        }

        private static void AddUkprn()
        {
            Console.WriteLine("Type a new UKRPN and press enter when done");
            Console.WriteLine("Enter 0 to clear");
            var unparsedUkprn = Console.ReadLine();
            SetUkprn(unparsedUkprn);
        }

        private static void SetUkprn(string ukprn)
        {
            if (ukprn.Trim().Equals("0"))
            {
                Ukprns.Clear();
                Config.UkprnList = "0";
            }
            else if (long.TryParse(ukprn, out var result))
            {
                Ukprns.Add(result);
                Config.UkprnList += $",{ukprn}";
            }
        }

        private static void UpdatePeriods()
        {
            Console.WriteLine("Type a comma separated list of delivery periods and press enter when done");
            var allPeriods = Console.ReadLine();
            SetPeriodsFromString(allPeriods);
        }

        private static void SetPeriodsFromString(string allPeriods)
        {
            var periodArray = allPeriods?.Split(',') ?? new string[0];
            var candidatePeriods = new List<int>();

            foreach (var unparsedPeriod in periodArray)
            {
                if (int.TryParse(unparsedPeriod, out var result))
                {
                    candidatePeriods.Add(result);
                }
            }

            if (candidatePeriods.Count > 0)
            {
                _periods = candidatePeriods;
                Config.PeriodList = allPeriods;
            }
        }

        private static async Task ProcessComparison(int jobId)
        {
            var filename = $"V2 Verification Results - Job ID - {jobId} - {DateTime.Now:yyyy-MM-dd hh-mm}.xlsx";

            // Get the payments
            var v1Payments = await Sql.Read<Payment>(PaymentSystem.V1, Script.Payments, _periods, Ukprns);
            Log.Write($"Retrieved {v1Payments.Count} V1 Payments");

            var v2Payments = await Sql.Read<Payment>(PaymentSystem.V2, Script.Payments, _periods, Ukprns);
            Log.Write($"Retrieved {v2Payments.Count} V2 Payments");

            var v1PaymentsWithoutV2 = v1Payments.Except(v2Payments).ToList();
            var v2PaymentsWithoutV1 = v2Payments.Except(v1Payments).ToList();
            var commonPayments = v1Payments.Intersect(v2Payments).ToList();
            Log.Write("Payment comparison complete");

            // Get the earnings
            var v1Earnings = new List<Earning>();
            var v2Earnings = new List<Earning>();
            //var v1Earnings = await Sql.Read<Earning>(PaymentSystem.V1, Script.Earnings, _periods);
            //Log.Write($"Retrieved {v1Earnings.Count} V1 Earnings");

            //var v2Earnings = await Sql.Read<Earning>(PaymentSystem.V2, Script.Earnings, _periods);
            //Log.Write($"Retrieved {v2Earnings.Count} V2 Earnings");

            var v1EarningsWithoutV2 = v1Earnings.Except(v2Earnings).ToList();
            var v2EarningsWithoutV1 = v2Earnings.Except(v1Earnings).ToList();
            var commonEarnings = v1Earnings.Intersect(v2Earnings).ToList();
            Log.Write("Earning comparison complete");

            // Get the required payments
            var v1RequiredPayments =
                await Sql.Read<RequiredPayment>(PaymentSystem.V1, Script.RequiredPayments, _periods, Ukprns);
            Log.Write($"Retrieved {v1RequiredPayments.Count} V1 Required Payments");

            var v2RequiredPayments =
                await Sql.Read<RequiredPayment>(PaymentSystem.V2, Script.RequiredPayments, _periods, Ukprns);
            Log.Write($"Retrieved {v2RequiredPayments.Count} V2 Required Payments");

            var v1RequiredPaymentsWithoutV2 = v1RequiredPayments.Except(v2RequiredPayments).ToList();
            var v2RequiredPaymentsWithoutV1 = v2RequiredPayments.Except(v1RequiredPayments).ToList();
            var commonRequiredPayments = v1RequiredPayments.Intersect(v2RequiredPayments).ToList();
            Log.Write("Required Payments comparison complete");

            // For V1 payments without V2 - are the earnings the same?



            // High level summary
            var v1PaymentsByTransactionType = v1Payments.ToLookup(x => x.TransactionType);
            var v2PaymentsByTransactionType = v2Payments.ToLookup(x => x.TransactionType);
            var v1RequiredPaymentsByTransactionType = v1RequiredPayments.ToLookup(x => x.TransactionType);
            var v2RequiredPaymentsByTransactionType = v2RequiredPayments.ToLookup(x => x.TransactionType);
            var summary = new List<HighLevelSummaryByTransactionType>();

            // For each transaction type
            summary.Add(new HighLevelSummaryByTransactionType{Heading = "All Payments"});
            for (int i = 1; i < 17; i++)
            {
                // Create a new row
                summary.Add(new HighLevelSummaryByTransactionType
                {
                    TransactionType = i,
                    // Aggregate all amounts for this transaction type
                    V1PaymentsAmount = v1PaymentsByTransactionType[i].Sum(x => x.Amount),
                    V2PaymentsAmount = v2PaymentsByTransactionType[i].Sum(x => x.Amount),
                    V1RequiredPaymentsAmount = v1RequiredPaymentsByTransactionType[i].Sum(x => x.Amount),
                    V2RequiredPaymentsAmount = v2RequiredPaymentsByTransactionType[i].Sum(x => x.Amount),
                    V1EarningsAmount = CalculateEarnings(v1Earnings, i),
                    V2EarningsAmount = CalculateEarnings(v2Earnings, i),
                    NumberOfV1Payments = v1Payments.Count(x => x.TransactionType == i),
                    NumberOfV2Payments = v2Payments.Count(x => x.TransactionType == i),
                    NumberOfV1RequiredPayments = v1RequiredPayments.Count(x => x.TransactionType == i),
                    NumberOfV2RequiredPayments = v2RequiredPayments.Count(x => x.TransactionType == i),
                    NumberOfV1Learners = v1Payments.Where(x => x.TransactionType == i).ToList().DistinctBy(x => x.LearnerUln).Count(),
                    NumberOfV2Learners = v2Payments.Where(x => x.TransactionType == i).ToList().DistinctBy(x => x.LearnerUln).Count(),
                    AbsoluteSumOfV1OnlyPayments = v1PaymentsWithoutV2.Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                    AbsoluteSumOfV2OnlyPayments = v2PaymentsWithoutV1.Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                    AbsoluteSumOfV1OnlyRequiredPayments = v1RequiredPaymentsWithoutV2.Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                    AbsoluteSumOfV2OnlyRequiredPayments = v2PaymentsWithoutV1.Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),

                });
            }

            // ACT1
            summary.Add(new HighLevelSummaryByTransactionType{Heading = "ACT1"});
            for (int i = 1; i < 17; i++)
            {
                // Create a new row
                summary.Add(new HighLevelSummaryByTransactionType
                {
                    TransactionType = i,
                    // Aggregate all amounts for this transaction type
                    V1PaymentsAmount = v1PaymentsByTransactionType[i].Where(x => x.ContractType == 1).Sum(x => x.Amount),
                    V2PaymentsAmount = v2PaymentsByTransactionType[i].Where(x => x.ContractType == 1).Sum(x => x.Amount),
                    V1RequiredPaymentsAmount = v1RequiredPaymentsByTransactionType[i].Where(x => x.ContractType == 1).Sum(x => x.Amount),
                    V2RequiredPaymentsAmount = v2RequiredPaymentsByTransactionType[i].Where(x => x.ContractType == 1).Sum(x => x.Amount),
                    V1EarningsAmount = CalculateEarnings(v1Earnings.Where(x => x.ContractType == 1).ToList(), i),
                    V2EarningsAmount = CalculateEarnings(v2Earnings.Where(x => x.ContractType == 1).ToList(), i),
                    NumberOfV1Payments = v1Payments.Where(x => x.TransactionType == i).Count(x => x.ContractType == 1),
                    NumberOfV2Payments = v2Payments.Where(x => x.TransactionType == i).Count(x => x.ContractType == 1),
                    NumberOfV1RequiredPayments = v1RequiredPayments.Where(x => x.TransactionType == i).Count(x => x.ContractType == 1),
                    NumberOfV2RequiredPayments = v2RequiredPayments.Where(x => x.TransactionType == i).Count(x => x.ContractType == 1),
                    NumberOfV1Learners = v1Payments.Where(x => x.ContractType == 1).Where(x => x.TransactionType == i).ToList().DistinctBy(x => x.LearnerUln).Count(),
                    NumberOfV2Learners = v2Payments.Where(x => x.ContractType == 1).Where(x => x.TransactionType == i).ToList().DistinctBy(x => x.LearnerUln).Count(),
                    AbsoluteSumOfV1OnlyPayments = v1PaymentsWithoutV2.Where(x => x.ContractType == 1).Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                    AbsoluteSumOfV2OnlyPayments = v2PaymentsWithoutV1.Where(x => x.ContractType == 1).Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                    AbsoluteSumOfV1OnlyRequiredPayments = v1RequiredPaymentsWithoutV2.Where(x => x.ContractType == 1).Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                    AbsoluteSumOfV2OnlyRequiredPayments = v2PaymentsWithoutV1.Where(x => x.ContractType == 1).Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                });
            }

            // ACT2
            summary.Add(new HighLevelSummaryByTransactionType { Heading = "ACT2" });
            for (int i = 1; i < 17; i++)
            {
                // Create a new row
                summary.Add(new HighLevelSummaryByTransactionType
                {
                    TransactionType = i,
                    // Aggregate all amounts for this transaction type
                    V1PaymentsAmount = v1Payments.Where(x => x.TransactionType == i).Where(x => x.ContractType == 2).Sum(x => x.Amount),
                    V2PaymentsAmount = v2Payments.Where(x => x.TransactionType == i).Where(x => x.ContractType == 2).Sum(x => x.Amount),
                    V1RequiredPaymentsAmount = v1RequiredPayments.Where(x => x.TransactionType == i).Where(x => x.ContractType == 2).Sum(x => x.Amount),
                    V2RequiredPaymentsAmount = v2RequiredPayments.Where(x => x.TransactionType == i).Where(x => x.ContractType == 2).Sum(x => x.Amount),
                    V1EarningsAmount = CalculateEarnings(v1Earnings.Where(x => x.ContractType == 2).ToList(), i),
                    V2EarningsAmount = CalculateEarnings(v2Earnings.Where(x => x.ContractType == 2).ToList(), i),
                    NumberOfV1Payments = v1Payments.Where(x => x.TransactionType == i).Count(x => x.ContractType == 2),
                    NumberOfV2Payments = v2Payments.Where(x => x.TransactionType == i).Count(x => x.ContractType == 2),
                    NumberOfV1RequiredPayments = v1RequiredPayments.Where(x => x.TransactionType == i).Count(x => x.ContractType == 2),
                    NumberOfV2RequiredPayments = v2RequiredPayments.Where(x => x.TransactionType == i).Count(x => x.ContractType == 2),
                    NumberOfV1Learners = v1Payments.Where(x => x.ContractType == 2).Where(x => x.TransactionType == i).ToList().DistinctBy(x => x.LearnerUln).Count(),
                    NumberOfV2Learners = v2Payments.Where(x => x.ContractType == 2).Where(x => x.TransactionType == i).ToList().DistinctBy(x => x.LearnerUln).Count(),
                    AbsoluteSumOfV1OnlyPayments = v1PaymentsWithoutV2.Where(x => x.ContractType == 2).Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                    AbsoluteSumOfV2OnlyPayments = v2PaymentsWithoutV1.Where(x => x.ContractType == 2).Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                    AbsoluteSumOfV1OnlyRequiredPayments = v1RequiredPaymentsWithoutV2.Where(x => x.ContractType == 2).Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                    AbsoluteSumOfV2OnlyRequiredPayments = v2PaymentsWithoutV1.Where(x => x.ContractType == 2).Where(x => x.TransactionType == i).Sum(x => Math.Abs(x.Amount)),
                });
            }

            var jobSummary = new List<JobSummary>
            {
                new JobSummary
                {
                    Heading = "All",
                    NumberOfV1Learners = v1RequiredPayments.DistinctBy(x => x.LearnerUln).Count(),
                    NumberOfV2Learners = v2RequiredPayments.DistinctBy(x => x.LearnerUln).Count(),
                    Ukprns = string.Join(", ", Ukprns),
                    Periods = string.Join(", ", _periods),
                },
                new JobSummary
                {
                    Heading = "ACT 1",
                    NumberOfV1Learners = v1RequiredPayments.Where(x => x.ContractType == 1).DistinctBy(x => x.LearnerUln).Count(),
                    NumberOfV2Learners = v2RequiredPayments.Where(x => x.ContractType == 1).DistinctBy(x => x.LearnerUln).Count(),
                },
                new JobSummary
                {
                    Heading = "ACT 2",
                    NumberOfV1Learners = v1RequiredPayments.Where(x => x.ContractType == 2).DistinctBy(x => x.LearnerUln).Count(),
                    NumberOfV2Learners = v2RequiredPayments.Where(x => x.ContractType == 2).DistinctBy(x => x.LearnerUln).Count(),
                },
            };

            using (var dataStream = Excel.CreateExcelDocumentWithSheets(
                (summary, "High Level Summary"),
                (jobSummary, "Job Summary"),
                (v1PaymentsWithoutV2, "V1 Payments without V2"),
                (v2PaymentsWithoutV1, "V2 Payments without V1"),
                //(commonPayments, "Common Payments"),
                //(v1EarningsWithoutV2, "V1 Earnings without V2"),
                //(v2EarningsWithoutV1, "V2 Earnings without V1"),
                //(commonEarnings, "Common Earnings"),
                (v1RequiredPaymentsWithoutV2, "V1 Required Payments without V2"),
                (v2RequiredPaymentsWithoutV1, "V2 Required Payments without V1")
                //(commonRequiredPayments, "Common Required Payments")
                ))
            using (var file = File.Create(filename))
            {
                dataStream.CopyTo(file);
            }

            Log.Write("Finished creating Excel file");

            Log.Write($"Memory usage: {GC.GetTotalMemory(false):###,###,###,###,###,### bytes}");

            Log.Write("Saving to SQL");

            // Save 3 tables with a flag for where the record came from
            SetVerificationResult(v1PaymentsWithoutV2, VerificationResult.V1Only, jobId);
            await Sql.Write(PaymentSystem.Output, v1PaymentsWithoutV2, "Payments");
            SetVerificationResult(v2PaymentsWithoutV1, VerificationResult.V2Only, jobId);
            await Sql.Write(PaymentSystem.Output, v2PaymentsWithoutV1, "Payments");
            SetVerificationResult(commonPayments, VerificationResult.Okay, jobId);
            await Sql.Write(PaymentSystem.Output, commonPayments, "Payments");

            SetVerificationResult(v1EarningsWithoutV2, VerificationResult.V1Only, jobId);
            await Sql.Write(PaymentSystem.Output, v1EarningsWithoutV2, "Earnings");
            SetVerificationResult(v2EarningsWithoutV1, VerificationResult.V2Only, jobId);
            await Sql.Write(PaymentSystem.Output, v2EarningsWithoutV1, "Earnings");
            SetVerificationResult(commonEarnings, VerificationResult.Okay, jobId);
            await Sql.Write(PaymentSystem.Output, commonEarnings, "Earnings");

            SetVerificationResult(v1RequiredPaymentsWithoutV2, VerificationResult.V1Only, jobId);
            await Sql.Write(PaymentSystem.Output, v1RequiredPaymentsWithoutV2, "RequiredPayments");
            SetVerificationResult(v2RequiredPaymentsWithoutV1, VerificationResult.V2Only, jobId);
            await Sql.Write(PaymentSystem.Output, v2RequiredPaymentsWithoutV1, "RequiredPayments");
            SetVerificationResult(commonRequiredPayments, VerificationResult.Okay, jobId);
            await Sql.Write(PaymentSystem.Output, commonRequiredPayments, "RequiredPayments");
        }

        private static readonly TypeAccessor EarningsAccesor = TypeAccessor.Create(typeof(Earning));
        private static decimal CalculateEarnings(List<Earning> earnings, int transactionType)
        {
            var total = 0m;
            foreach (var earning in earnings)
            {
                total += (decimal)EarningsAccesor[earning, $"TransactionType{transactionType:D2}"];
            }

            return total;
        }

        private static Task InitialiseActiveLearners(Inclusions inclusions)
        {
            return Sql.InitialiseLearnerTables(inclusions, Ukprns, _periods);
        }

        private static void SetVerificationResult<T>(List<T> input, VerificationResult result, int jobId) where T : IContainVerificationResults
        {
            for (int i = 0; i < input.Count; i++)
            {
                var temp = input[i];
                temp.VerificationResult = result;
                temp.JobId = jobId;
                input[i] = temp;
            }
        }
    }
}
