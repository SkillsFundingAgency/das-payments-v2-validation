
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using FastMember;
using SFA.DAS.Payments.Migration.Constants;
using SFA.DAS.Payments.Migration.DTO;
using SFA.DAS.Payments.Migration.Services;
using SFA.DAS.Payments.ProviderPayments.Model.V1;
using SFA.DAS.Payments.Verification.Constants;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace SFA.DAS.Payments.Migration
{
    class Program
    {
        static async Task Log(string message)
        {
            using (var file = File.AppendText("log.txt"))
            {
                await file.WriteLineAsync(message);
            }
            Console.WriteLine(message);
        }

        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
            START:

                await Log("");
                await Log("Please select what you would like to migrate:");
                await Log("1 - Commitments");
                await Log("2 - Accounts");
                await Log("3 - Payments");
                await Log("4 - EAS");
                await Log("5 - V2 Payments -> V1");
                await Log("6 - Complete Return (1920 only)");
                await Log("7 - V2 Transfers that didn't work -> V1");
                await Log("8 - V2 Account Transfers -> V1");
                await Log("9 - All V1 -> V2 (1, 2, 3 & 4)");
                await Log("10 - All V2 -> V1 (5, 8, 7, 6)");
                await Log("T - Test Connections");
                await Log("Esc - exit");

                var typeinput = Console.ReadKey();

                if (typeinput.Key == ConsoleKey.Escape)
                {
                    await Log("Finished - press enter to continue...");
                    Console.ReadLine();
                    return;
                }

                if (typeinput.Key == ConsoleKey.T)
                {
                    await TestConnections();
                    goto START;
                }

                if (!int.TryParse(typeinput.KeyChar.ToString(), out var typeinputAsInteger))
                {
                    await Log("Please enter a number");
                    goto START;
                }

                await MakeSelection(typeinputAsInteger);
                goto START;
            }
            catch (Exception e)
            {
                await Log(e.Message);
                await Log(e.StackTrace);
                await Log("Press enter to continue...");
                Console.ReadLine();
            }
        }

        private static async Task MakeSelection(int selection)
        {
            if (selection == 1 || selection == 9)
            {
                await ProcessCommitmentsData();
            }
            if (selection == 2 || selection == 9)
            {
                await ProcessAccountsData(0);
            }

            if (selection == 3 || selection == 9)
            {
                await ProcessPayments(0);
            }

            if (selection == 4 || selection == 9)
            {
                await ProcessEas();
            }

            if (selection == 5)
            {
                await ProcessV1Payments();
            }

            if (selection == 6)
            {
                await CompletePeriod();
            }

            if (selection == 7)
            {
                await ProcessFailedV1Payments();
            }

            if (selection == 8)
            {
                await ProcessV1AccountTransfers();
            }

            if (selection == 10)
            {
                var period = await GetPeriod();
                await ProcessV1Payments(period);
                await ProcessV1AccountTransfers(period);
                await ProcessFailedV1Payments(period);
                await CompletePeriod(period);
            }
        }

        private static async Task TestConnections()
        {
            await TestConnection("V1", "V1 Payments + Eas");
            await TestConnection("V2", "V2 (Payments/Eas/Commitments/Accounts)");
            await TestConnection("V1Commitments", "V1 Commitments");
            await TestConnection("V1Accounts", "V1 Accounts");
            await TestConnection("DasCommitments", "DAS Commitments");
        }

        private static async Task TestConnection(string connectionString, string friendlyName)
        {
            await Log($"Testing: {friendlyName}");
            try
            {
                using (var connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString))
                {
                    await connection.OpenAsync();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                await Log($"Connection successful for {friendlyName}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Log($"Connection failed for {friendlyName}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            await Log("");
        }

        private static async Task CompletePeriod()
        {
            var period = await GetPeriod();
            await CompletePeriod(period);
        }

        private static async Task CompletePeriod(int period)
        {
            var trigger = CreateTrigger(period);
            var triggerList = new List<LegacyPeriodModel> { trigger };

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V1"].ConnectionString))
            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                bulkCopy.BatchSize = 1000;
                bulkCopy.BulkCopyTimeout = 30;

                bulkCopy.DestinationTableName = "[Payments].[Periods]";
                PopulateBulkCopy(bulkCopy, typeof(LegacyPeriodModel));

                using (var reader = ObjectReader.Create(triggerList))
                {
                    await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                }
            }
        }

        private static LegacyPeriodModel CreateTrigger(int period)
        {
            var now = DateTime.Now;

            var trigger = new LegacyPeriodModel
            {
                AccountDataValidAt = now,
                CommitmentDataValidAt = now,
                CompletionDateTime = now,
                PeriodName = $"1920-R{period:D2}",
                CalendarMonth = PaymentMapper.MonthFromPeriod((byte)period),
                CalendarYear = PaymentMapper.YearFromPeriod(1920, (byte)period),
            };

            return trigger;
        }


        private static async Task<int> GetPeriod()
        {
            while (true)
            {
                await Log("");
                await Log("Please enter the collection period: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 or 14");
                var chosenPeriod = Console.ReadLine();
                if (!int.TryParse(chosenPeriod, out var collectionPeriod) || collectionPeriod < 1 || collectionPeriod > 14)
                {
                    await Log($"Invalid collection period: '{chosenPeriod}'.");
                    continue;
                }

                return collectionPeriod;
            }
        }

        private static async Task ProcessV1Payments()
        {
            var collectionPeriod = await GetPeriod();
            await ProcessV1Payments(collectionPeriod);
        }

        private static async Task ProcessV1Payments(int collectionPeriod)
        {
            var mapper = new PaymentMapper();

            //using(var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            using (var v2Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
            using (var v1Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V1"].ConnectionString))
            {
                await v1Connection.OpenAsync().ConfigureAwait(false);

                // Per page
                var pageSize = 100000;
                var offset = 0;

                List<V2PaymentAndEarning> paymentsAndEarnings;

                do
                {
                    // Load from v2
                    paymentsAndEarnings = (await v2Connection.QueryAsync<V2PaymentAndEarning>(V2Sql.PaymentsAndEarnings, 
                            new { collectionPeriod, offset, pageSize },
                            commandTimeout: 3600))
                        .ToList();
                    await Log($"Loaded {paymentsAndEarnings.Count} records from page {offset / pageSize}");

                    // Map
                    var outputResults = mapper.MapV2Payments(paymentsAndEarnings, new HashSet<Guid>());

                    var requiredPayments = outputResults.requiredPayments;
                    var payments = outputResults.payments;
                    var earnings = outputResults.earnings;
                    
                    var minDate = new DateTime(2000, 1, 1);
                    requiredPayments.ForEach(x =>
                    {
                        if (x.LearningStartDate < minDate) x.LearningStartDate = null;
                    });

                    earnings.ForEach(x =>
                    {
                        if (x.ActualEnddate < minDate) x.ActualEnddate = null;
                        if (x.PlannedEndDate < minDate) x.PlannedEndDate = minDate;
                        if (x.StartDate < minDate) x.StartDate = minDate;
                    });

                    // Write to V1
                    using (var bulkCopy = new SqlBulkCopy(v1Connection))
                    {
                        bulkCopy.BatchSize = 1000;
                        bulkCopy.BulkCopyTimeout = 3600;

                        bulkCopy.DestinationTableName = "[PaymentsDue].[RequiredPayments]";
                        PopulateBulkCopy(bulkCopy, typeof(LegacyRequiredPaymentModel));

                        using (var reader = ObjectReader.Create(requiredPayments))
                        {
                            await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                        }

                        bulkCopy.DestinationTableName = "[Payments].[Payments]";
                        bulkCopy.ColumnMappings.Clear();
                        PopulateBulkCopy(bulkCopy, typeof(LegacyPaymentModel));

                        using (var reader = ObjectReader.Create(payments))
                        {
                            await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                        }

                        bulkCopy.DestinationTableName = "[PaymentsDue].[Earnings]";
                        bulkCopy.ColumnMappings.Clear();
                        PopulateBulkCopy(bulkCopy, typeof(LegacyEarningModel));

                        using (var reader = ObjectReader.Create(earnings))
                        {
                            await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                        }
                    }
                    
                    offset += pageSize;
                } while (paymentsAndEarnings.Count > 0);

                //scope.Complete();
            }
        }

        private static async Task ProcessV1AccountTransfers()
        {
            var collectionPeriod = await GetPeriod();
            await ProcessV1AccountTransfers(collectionPeriod);
        }

        private static async Task ProcessV1AccountTransfers(int collectionPeriod)
        {
            var mapper = new PaymentMapper();

            //using(var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            using (var v2Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
            using (var v1Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V1"].ConnectionString))
            {
                await v1Connection.OpenAsync().ConfigureAwait(false);

                // Per page
                var pageSize = 100000;
                var offset = 0;

                List<V2PaymentAndEarning> paymentsAndEarnings;

                do
                {
                    // Load from v2
                    paymentsAndEarnings = (await v2Connection.QueryAsync<V2PaymentAndEarning>(V2Sql.PaymentsAndEarnings,
                            new { collectionPeriod, offset, pageSize },
                            commandTimeout: 3600))
                        .ToList();
                    await Log($"Loaded {paymentsAndEarnings.Count} records from page {offset / pageSize}");

                    // Map
                    var accountTransfers = mapper.MapV2AccountTransfers(paymentsAndEarnings);

                    // Write to V1
                    using (var bulkCopy = new SqlBulkCopy(v1Connection))
                    {
                        bulkCopy.BatchSize = 1000;
                        bulkCopy.BulkCopyTimeout = 3600;

                        bulkCopy.DestinationTableName = "[TransferPayments].[AccountTransfers]";
                        bulkCopy.ColumnMappings.Clear();
                        PopulateBulkCopy(bulkCopy, typeof(LegacyAccountTransferModel));

                        using (var reader = ObjectReader.Create(accountTransfers))
                        {
                            await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                        }
                    }

                    offset += pageSize;
                } while (paymentsAndEarnings.Count > 0);

                //scope.Complete();
            }
        }

        private static async Task ProcessFailedV1Payments()
        {
            var collectionPeriod = await GetPeriod();
            await ProcessFailedV1Payments(collectionPeriod);
        }

        private static async Task ProcessFailedV1Payments(int collectionPeriod)
        {
            var mapper = new PaymentMapper();

            using (var v2Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
            using (var v1Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V1"].ConnectionString))
            {
                await v1Connection.OpenAsync().ConfigureAwait(false);

                var paymentsAndEarnings = (await v2Connection.QueryAsync<V2PaymentAndEarning>(
                        V2Sql.PaymentsAndEarningsForFailedTransfers,
                        new {collectionPeriod},
                        commandTimeout: 3600))
                    .ToList();
                await Log($"Loaded {paymentsAndEarnings.Count} records");

                // Get any existing required payments that have already been copied
                var potentialIds = paymentsAndEarnings.Select(x => x.RequiredPaymentEventId).ToList();
                var existingIds = new HashSet<Guid>(await v1Connection.QueryAsync<Guid>(
                        V1Sql.ExistingRequiredPayments, new {requiredPaymentids = potentialIds}));

                await Log($"Found {existingIds.Count} existing required payments");

                // Map
                var outputResults = mapper.MapV2Payments(paymentsAndEarnings, new HashSet<Guid>(existingIds));

                var requiredPayments = outputResults.requiredPayments;
                var payments = outputResults.payments;
                var earnings = outputResults.earnings;

                var minDate = new DateTime(2000, 1, 1);
                requiredPayments.ForEach(x =>
                {
                    if (x.LearningStartDate < minDate) x.LearningStartDate = null;
                });

                earnings.ForEach(x =>
                {
                    if (x.ActualEnddate < minDate) x.ActualEnddate = null;
                    if (x.PlannedEndDate < minDate) x.PlannedEndDate = minDate;
                    if (x.StartDate < minDate) x.StartDate = minDate;
                });

                // Write to V1
                using (var bulkCopy = new SqlBulkCopy(v1Connection))
                {
                    bulkCopy.BatchSize = 5000;
                    bulkCopy.BulkCopyTimeout = 3600;

                    bulkCopy.DestinationTableName = "[PaymentsDue].[RequiredPayments]";
                    PopulateBulkCopy(bulkCopy, typeof(LegacyRequiredPaymentModel));

                    using (var reader = ObjectReader.Create(requiredPayments))
                    {
                        await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                    }

                    bulkCopy.DestinationTableName = "[Payments].[Payments]";
                    bulkCopy.ColumnMappings.Clear();
                    PopulateBulkCopy(bulkCopy, typeof(LegacyPaymentModel));

                    using (var reader = ObjectReader.Create(payments))
                    {
                        await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                    }

                    bulkCopy.DestinationTableName = "[PaymentsDue].[Earnings]";
                    bulkCopy.ColumnMappings.Clear();
                    PopulateBulkCopy(bulkCopy, typeof(LegacyEarningModel));

                    using (var reader = ObjectReader.Create(earnings))
                    {
                        await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                    }
                }
            }
        }

        private static void PopulateBulkCopy(SqlBulkCopy bulkCopy, Type entityType)
        {
            var columns = entityType.GetProperties();
            foreach (var propertyInfo in columns)
            {
                bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(propertyInfo.Name, propertyInfo.Name));
            }
        }

        static async Task ProcessEas()
        {
            var properties = typeof(EasRecord).GetProperties();
            var mappings = new List<SqlBulkCopyColumnMapping>();

            foreach (var propertyInfo in properties)
            {
                mappings.Add(new SqlBulkCopyColumnMapping(propertyInfo.Name, propertyInfo.Name));
            }

            await Log("Processing EAS migration");
            using (var connection =
                new SqlConnection(ConfigurationManager.ConnectionStrings["V1"].ConnectionString))
            {
                var records = (await connection
                        .QueryAsync<EasRecord>(V1Sql.EasRecords, commandTimeout: 3600)
                        .ConfigureAwait(false))
                    .ToList();
                await Log($"Loaded {records.Count} V1 EAS payments");

                using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    TimeSpan.FromMinutes(10),
                    TransactionScopeAsyncFlowOption.Enabled))
                using (var v2Connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                using (var bulkCopy = new SqlBulkCopy(v2Connection))
                using (var reader = ObjectReader.Create(records))
                {
                    if (v2Connection.State != ConnectionState.Open)
                    {
                        await v2Connection.OpenAsync();
                    }

                    var deleted = await v2Connection.ExecuteAsync(V2Sql.DeleteEasPayments, commandTimeout: 3600);
                    await Log($"Deleted {deleted} V2 EAS records");

                    bulkCopy.DestinationTableName = "Payments2.ProviderAdjustmentPayments";
                    bulkCopy.BatchSize = 5000;
                    bulkCopy.BulkCopyTimeout = 3600;

                    foreach (var sqlBulkCopyColumnMapping in mappings)
                    {
                        bulkCopy.ColumnMappings.Add(sqlBulkCopyColumnMapping);
                    }

                    await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                    await Log("Inserted payments in V2");

                    scope.Complete();
                    await Log("Completed EAS migration");
                }
            }
        }

        static async Task ProcessPayments(int maxPeriod)
        {
            await Log("Process 1617 and 1718?");
            await Log("Press 1 to process or 0 to ignore...");
            var process = Console.ReadKey().Key == ConsoleKey.D1;

            var periods = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            var academicYears = new List<int> { 1617, 1718, 1819 };

            var periodsToIgnore = ConfigurationManager.AppSettings["PeriodsToIgnore"]
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(int.Parse)
                .ToList();

            var properties = typeof(Payment).GetProperties();
            var mappings = new List<SqlBulkCopyColumnMapping>();

            foreach (var propertyInfo in properties)
            {
                mappings.Add(new SqlBulkCopyColumnMapping(propertyInfo.Name, propertyInfo.Name));
            }

            var v1PaymentsSql = UpdateTableAndSchemaNames(V1Sql.Payments);

            foreach (var academicYear in academicYears)
            {
                if ((academicYear == 1617 || academicYear == 1718) && !process)
                {
                    continue;
                }

                foreach (var collectionPeriod in periods)
                {
                    if (academicYear == 1819 && collectionPeriod > maxPeriod)
                    {
                        break;
                    }

                    if (periodsToIgnore.Contains(collectionPeriod))
                    {
                        await Log($"Ignoring period {collectionPeriod}");
                        continue;
                    }

                    using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V1"].ConnectionString))
                    {
                        var collectionPeriodName = $"{academicYear}-R{collectionPeriod:D2}";

                        var payments = (await connection
                            .QueryAsync<Payment>(v1PaymentsSql, new { period = collectionPeriodName },
                                commandTimeout: 3600)
                            .ConfigureAwait(false))
                            .ToList();

                        var minSqlDate = new DateTime(1800, 1, 1);
                        var maxSqlDate = new DateTime(9000, 1, 1);

                        payments.ForEach(x =>
                        {
                            if (x.EarningsStartDate < minSqlDate)
                                x.EarningsStartDate = minSqlDate;
                            if (x.EarningsStartDate > maxSqlDate)
                                x.EarningsStartDate = maxSqlDate;

                            if (x.EarningsActualEndDate < minSqlDate)
                                x.EarningsActualEndDate = minSqlDate;
                            if (x.EarningsActualEndDate > maxSqlDate)
                                x.EarningsActualEndDate = maxSqlDate;

                            if (x.EarningsPlannedEndDate < minSqlDate)
                                x.EarningsPlannedEndDate = minSqlDate;
                            if (x.EarningsPlannedEndDate > maxSqlDate)
                                x.EarningsPlannedEndDate = maxSqlDate;
                        });

                        await Log($"Retrieved {payments.Count} payments for {collectionPeriodName}");

                        using (var scope = new TransactionScope(
                            TransactionScopeOption.Required,
                            TimeSpan.FromMinutes(5),
                            TransactionScopeAsyncFlowOption.Enabled))
                        using (var v2Connection =
                            new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                        using (var bulkCopy = new SqlBulkCopy(v2Connection))
                        using (var reader = ObjectReader.Create(payments))
                        {
                            await v2Connection.OpenAsync();
                            await v2Connection.ExecuteAsync(V2Sql.DeletePayments, new { academicYear, collectionPeriod }, commandTimeout: 3600);
                            await Log("Deleted existing payments");

                            if (v2Connection.State != ConnectionState.Open)
                            {
                                await v2Connection.OpenAsync();
                            }

                            bulkCopy.DestinationTableName = "Payments2.Payment";
                            bulkCopy.BatchSize = 5000;
                            bulkCopy.BulkCopyTimeout = 3600;

                            foreach (var sqlBulkCopyColumnMapping in mappings)
                            {
                                bulkCopy.ColumnMappings.Add(sqlBulkCopyColumnMapping);
                            }


                            await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);

                            scope.Complete();
                        }

                        await Log($"Saved payments for {collectionPeriodName}");
                    }

                    GC.Collect();
                }
            }
        }

        private static string UpdateTableAndSchemaNames(string originalSql)
        {
            return originalSql
                    .Replace("[DAS_PeriodEnd]", $"[{Config.PaymentsDatabase}]")
                    .Replace(".Payments.", $".[{Config.PaymentsSchemaPrefix}Payments].")
                    .Replace(".PaymentsDue.", $".[{Config.PaymentsSchemaPrefix}PaymentsDue].")
                    .Replace(".TransferPayments.", $".[{Config.PaymentsSchemaPrefix}TransferPayments].")
                    .Replace("DS_SILR1819_Collection", Config.EarningsDatabase)
                    .Replace("[@@V2DATABASE@@]", $"[{Config.V2PaymentsDatabase}]")
                ;
        }

        static async Task ProcessCommitmentsData()
        {
            HashSet<long> levyAccounts;
            using (var v2Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
            {
                var accounts = await v2Connection.QueryAsync<long>(
                    "SELECT AccountId FROM Payments2.LevyAccount WHERE IsLevyPayer = 1",
                    commandTimeout: 3600);
                levyAccounts = new HashSet<long>(accounts);
            }

            using (var dasConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DASCommitments"].ConnectionString))
            {
                var commitments = (await dasConnection
                    .QueryAsync<Commitment>(DasSql.Commitments, commandTimeout: 3600)
                    .ConfigureAwait(false))
                    .ToList();

                foreach (var commitment in commitments)
                {
                    if (commitment.TrainingType == 0)
                    {
                        commitment.ProgrammeType = 25;
                        commitment.StandardCode = int.Parse(commitment.TrainingCode);
                    }
                    else if (commitment.TrainingType == 1)
                    {
                        var portions = commitment.TrainingCode.Split('-');
                        commitment.FrameworkCode = int.Parse(portions[0]);
                        commitment.ProgrammeType = int.Parse(portions[1]);
                        commitment.PathwayCode = int.Parse(portions[2]);
                    }
                    else
                    {
                        await Log($"Unknown TrainingType: {commitment.TrainingType} for CommitmentId: {commitment.ApprenticeshipId}");
                    }
                }

                // ApprenticeshipId is what used to be called CommitmentId
                var commitmentsById = commitments.GroupBy(x => x.ApprenticeshipId);
                var apprenticeships = new List<Apprenticeship>();
                var apprenticeshipPriceEpisodes = new List<ApprenticeshipPriceEpisode>();
                var apprenticeshipPause = new List<ApprenticeshipPause>();

                foreach (var commitmentGroup in commitmentsById)
                {
                    var firstCommitment = commitmentGroup.First();

                    apprenticeships.Add(new Apprenticeship
                    {
                        AccountId = firstCommitment.AccountId,
                        EstimatedEndDate = firstCommitment.EndDate,
                        EstimatedStartDate = firstCommitment.StartDate,
                        FrameworkCode = firstCommitment.FrameworkCode,
                        LegalEntityName = firstCommitment.LegalEntityName,
                        PathwayCode = firstCommitment.PathwayCode,
                        ProgrammeType = firstCommitment.ProgrammeType,
                        Priority = firstCommitment.Priority,
                        StandardCode = firstCommitment.StandardCode,
                        Status = firstCommitment.PaymentStatus,
                        StopDate = firstCommitment.WithdrawnOnDate,
                        TransferSendingEmployerAccountId = firstCommitment.TransferSendingEmployerAccountId,
                        Ukprn = firstCommitment.Ukprn,
                        Uln = firstCommitment.Uln,
                        Id = firstCommitment.ApprenticeshipId,
                        IsLevyPayer = levyAccounts.Contains(firstCommitment.AccountId),
                        AgreedOnDate = firstCommitment.AgreedOnDate,
                        ApprenticeshipEmployerType = firstCommitment.ApprenticeshipEmployerType,
                        CreationDate = firstCommitment.CreatedDate,
                        AgreementId = firstCommitment.AccountLegalEntityPublicHashedId,
                    });

                    foreach (var commitment in commitmentGroup)
                    {
                        apprenticeshipPriceEpisodes.Add(new ApprenticeshipPriceEpisode
                        {
                            ApprenticeshipId = firstCommitment.ApprenticeshipId,
                            Cost = commitment.AgreedCost,
                            EndDate = commitment.EffectiveToDate,
                            Removed = false,
                            StartDate = commitment.EffectiveFromDate,
                            CreationDate = firstCommitment.CreatedDate,
                        });
                    }

                    if (firstCommitment.PaymentStatus == 2)
                    {
                        apprenticeshipPause.Add(new ApprenticeshipPause
                        {
                            ApprenticeshipId = firstCommitment.ApprenticeshipId,
                            PauseDate = firstCommitment.PausedOnDate ?? new DateTime(1900, 1, 1),
                        });
                    }
                }

                await Log($"Loaded {apprenticeships.Count} commitments");

                var providerPriority = (await dasConnection
                    .QueryAsync<ProviderPriority>("SELECT * FROM CustomProviderPaymentPriority"))
                    .ToList();
                await Log($"Loaded {providerPriority.Count} provider priority records");

                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.Serializable,
                    Timeout = TimeSpan.FromMinutes(15)
                }, TransactionScopeAsyncFlowOption.Enabled))
                using (var v2Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                {
                    await v2Connection.OpenAsync().ConfigureAwait(false);

                    await v2Connection.ExecuteAsync("DELETE Payments2.EmployerProviderPriority", commandTimeout: 3600);

                    using (var bulkCopy = new SqlBulkCopy(v2Connection))
                    using (var reader = ObjectReader.Create(apprenticeships))
                    {
                        await v2Connection.ExecuteAsync(V2Sql.DeleteCommitments, commandTimeout: 3600)
                            .ConfigureAwait(false);
                        await Log("Deleted old data");

                        bulkCopy.DestinationTableName = "Payments2.Apprenticeship";
                        bulkCopy.BatchSize = 5000;
                        bulkCopy.BulkCopyTimeout = 3600;

                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AccountId", "AccountId"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EstimatedEndDate", "EstimatedEndDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EstimatedStartDate", "EstimatedStartDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FrameworkCode", "FrameworkCode"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("LegalEntityName", "LegalEntityName"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PathwayCode", "PathwayCode"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ProgrammeType", "ProgrammeType"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Priority", "Priority"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StandardCode", "StandardCode"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Status", "Status"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StopDate", "StopDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TransferSendingEmployerAccountId", "TransferSendingEmployerAccountId"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Ukprn", "Ukprn"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Uln", "Uln"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Id", "Id"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsLevyPayer", "IsLevyPayer"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AgreedOnDate", "AgreedOnDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ApprenticeshipEmployerType", "ApprenticeshipEmployerType"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CreationDate", "CreationDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AgreementId", "AgreementId"));

                        await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                    }

                    await Log("Saved apprenticeships");

                    using (var bulkCopy = new SqlBulkCopy(v2Connection))
                    using (var reader = ObjectReader.Create(apprenticeshipPriceEpisodes))
                    {
                        bulkCopy.DestinationTableName = "Payments2.ApprenticeshipPriceEpisode";
                        bulkCopy.BatchSize = 5000;
                        bulkCopy.BulkCopyTimeout = 3600;

                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ApprenticeshipId", "ApprenticeshipId"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Cost", "Cost"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EndDate", "EndDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Removed", "Removed"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StartDate", "StartDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CreationDate", "CreationDate"));

                        await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                    }

                    await Log("Saved apprenticeship price episodes");

                    using (var bulkCopy = new SqlBulkCopy(v2Connection))
                    using (var reader = ObjectReader.Create(apprenticeshipPause))
                    {
                        bulkCopy.DestinationTableName = "Payments2.ApprenticeshipPause";
                        bulkCopy.BatchSize = 5000;
                        bulkCopy.BulkCopyTimeout = 3600;

                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ApprenticeshipId", "ApprenticeshipId"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PauseDate", "PauseDate"));

                        await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                    }

                    await Log("Saved apprenticeship pauses");

                    using (var bulkCopy = new SqlBulkCopy(v2Connection))
                    using (var reader = ObjectReader.Create(providerPriority))
                    {
                        bulkCopy.DestinationTableName = "Payments2.EmployerProviderPriority";
                        bulkCopy.BatchSize = 5000;
                        bulkCopy.BulkCopyTimeout = 3600;
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EmployerAccountId", "EmployerAccountId"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ProviderId", "Ukprn"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PriorityOrder", "Order"));

                        await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                        await Log("Saved priovider priorities");
                    }

                    scope.Complete();
                    await Log("Committed transaction");
                }
            }
        }

        static async Task ProcessAccountsData(int period)
        {
            using (var connection =
                new SqlConnection(ConfigurationManager.ConnectionStrings["V1Accounts"].ConnectionString))
            {
                // Data already deleted and identity insert is on
                var v1Accounts = await connection.QueryAsync<V1Account>(V1Sql.Accounts);
                var accounts = new List<LevyAccount>();

                foreach (var v1Account in v1Accounts)
                {
                    accounts.Add(new LevyAccount
                    {
                        AccountId = v1Account.AccountId,
                        AccountName = v1Account.AccountName,
                        Balance = v1Account.Balance,
                        IsLevyPayer = v1Account.IsLevyPayer,
                        TransferAllowance = v1Account.TransferAllowance,
                    });
                }

                await Log("Finished loading v1 accounts");

                using (var v2Connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                {
                    await v2Connection.OpenAsync().ConfigureAwait(false);

                    await v2Connection.ExecuteAsync(V2Sql.DeleteAccounts, commandTimeout: 3600);
                    await Log("Deleted accounts");

                    using (var bulkCopy = new SqlBulkCopy(v2Connection))
                    using (var reader = ObjectReader.Create(accounts))
                    {
                        bulkCopy.DestinationTableName = "Payments2.LevyAccount";
                        bulkCopy.BatchSize = 5000;
                        bulkCopy.BulkCopyTimeout = 3600;

                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AccountId", "AccountId"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AccountName", "AccountName"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Balance", "Balance"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsLevyPayer", "IsLevyPayer"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TransferAllowance",
                            "TransferAllowance"));

                        await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                    }

                    // Update the IsLevyPayer flag on the commitments
                    var accountIds = accounts.Where(x => x.IsLevyPayer == false).Select(x => x.AccountId).ToList();

                    var page = 0;
                    var pageSize = 1000;
                    List<long> accountIdsToProcess;
                    do
                    {
                        accountIdsToProcess = accountIds.Skip(page * pageSize).Take(pageSize).ToList();
                        await v2Connection.ExecuteAsync(V2Sql.UpdateLevyPayerFlag,
                                new { accountIds = accountIdsToProcess })
                            .ConfigureAwait(false);
                        page++;
                    } while (accountIdsToProcess.Any());

                }

                await Log("Finished writing v2 levy accounts");
            }
        }
    }
}
