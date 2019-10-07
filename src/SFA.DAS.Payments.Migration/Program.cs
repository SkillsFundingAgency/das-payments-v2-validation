
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
            try
            {
                var period = 0;
                while (period == 0)
                {
                    await Log("Please enter the period to initialise");
                    await Log("");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out var inputAsInteger))
                    {
                        await Log("Please enter a number between 1 and 14");
                    }
                    else
                    {
                        if (CollectionPeriods.CollectionPeriodDates.ContainsKey(inputAsInteger))
                        {
                            period = inputAsInteger;
                        }
                        else
                        {
                            await Log("Please enter a number between 1 and 14");
                        }
                    }
                }

                await Log("What data do you want to migrate");
                await Log("Please enter 1-Commitments, 2-Accounts, 3-Payments, 4-EAS, 5-V1 Payments, 6-Complete R02, 9-All");
                var typeinput = Console.ReadLine();
                if (!int.TryParse(typeinput, out var typeinputAsInteger))
                {
                    await Log("Please enter a number between 1 and 4");
                }


                if (typeinputAsInteger == 1 || typeinputAsInteger == 9)
                {
                    await ProcessCommitmentsData(period);
                }
                if (typeinputAsInteger == 2 || typeinputAsInteger == 9)
                {
                    await ProcessAccountsData(period);
                }

                if (typeinputAsInteger == 3 || typeinputAsInteger == 9)
                {
                    await ProcessPayments(period);
                }

                if (typeinputAsInteger == 4 || typeinputAsInteger == 9)
                {
                    await ProcessEas();
                }

                if (typeinputAsInteger == 5)
                {
                    await ProcessV1Payments();
                }

                if (typeinputAsInteger == 6)
                {
                    await CompleteR02();
                }

                await Log("Finished - press enter to continue...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                await Log(e.Message);
                await Log(e.StackTrace);
                await Log("Press enter to continue...");
                Console.ReadLine();
            }
        }

        private static async Task CompleteR02()
        {
            var trigger = CreateTrigger();
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

        private static LegacyPeriodModel CreateTrigger()
        {
            var now = DateTime.Now;

            var trigger = new LegacyPeriodModel
            {
                AccountDataValidAt = now,
                CommitmentDataValidAt = now,
                CompletionDateTime = now,
                PeriodName = "1920-R02",
                CalendarMonth = 9,
                CalendarYear = 2019,
            };

            return trigger;
        }

        private static async Task ProcessV1Payments()
        {
            var mapper = new PaymentMapper();
            var processedRequiredPayments = new HashSet<Guid>();

            //using(var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            using(var v2Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
            using (var v1Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V1"].ConnectionString))
            {
                await v1Connection.OpenAsync().ConfigureAwait(false);

                // Per page
                var pageSize = 10000;
                var offset = 0;

                List<V2PaymentAndEarning> paymentsAndEarnings;

                do
                {
                    // Load from v2
                    paymentsAndEarnings = (await v2Connection.QueryAsync<V2PaymentAndEarning>(V2Sql.PaymentsAndEarnings,
                            new {offset, pageSize},
                            commandTimeout: 3600))
                        .ToList();
                    await Log($"Loaded {paymentsAndEarnings.Count} records from page {offset / pageSize}");

                    // Map
                    var outputResults = mapper.MapV2Payments(paymentsAndEarnings, processedRequiredPayments);

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

                using (var v2Connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                using (var bulkCopy = new SqlBulkCopy(v2Connection))
                using (var reader = ObjectReader.Create(records))
                {
                    if (v2Connection.State != ConnectionState.Open)
                    {
                        await v2Connection.OpenAsync();
                    }

                    bulkCopy.DestinationTableName = "Payments2.ProviderAdjustmentPayments";
                    bulkCopy.BatchSize = 5000;
                    bulkCopy.BulkCopyTimeout = 3600;

                    foreach (var sqlBulkCopyColumnMapping in mappings)
                    {
                        bulkCopy.ColumnMappings.Add(sqlBulkCopyColumnMapping);
                    }


                    await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                }
            }
        }

        static async Task ProcessPayments(int maxPeriod)
        {
            await Log("Process 1617 and 1718?");
            await Log("Press 1 to process or 0 to ignore...");
            var process = Console.ReadKey().Key == ConsoleKey.D1;

            var periods = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14};
            var academicYears = new List<int> {1617, 1718, 1819};

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

                foreach (var period in periods)
                {
                    if (academicYear == 1819 && period > maxPeriod)
                    {
                        break;
                    }

                    if (periodsToIgnore.Contains(period))
                    {
                        await Log($"Ignoring period {period}");
                        continue;
                    }

                    using (var connection =
                        new SqlConnection(ConfigurationManager.ConnectionStrings["V1"].ConnectionString))
                    {

                        var collectionPeriodName = $"{academicYear}-R{period:D2}";

                        var payments = (await connection
                            .QueryAsync<Payment>(v1PaymentsSql, new {period = collectionPeriodName},
                                commandTimeout: 3600)
                            .ConfigureAwait(false))
                            .ToList();

                        await Log($"Retrieved {payments.Count} payments for {collectionPeriodName}");

                        using (var v2Connection =
                            new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                        using (var bulkCopy = new SqlBulkCopy(v2Connection))
                        using (var reader = ObjectReader.Create(payments))
                        {
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

        static async Task ProcessCommitmentsData(int period)
        {
            using (var connection =
                new SqlConnection(ConfigurationManager.ConnectionStrings["V1Commitments"].ConnectionString))
            {
                var collectionPeriodDate = CollectionPeriods.CollectionPeriodDates[period];
                var commitments = await connection
                    .QueryAsync<Commitment>(V1Sql.Commitments, new { inputDate = collectionPeriodDate })
                    .ConfigureAwait(false);

                var commitmentsById = commitments.GroupBy(x => x.CommitmentId);
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
                        Id = firstCommitment.CommitmentId,
                        IsLevyPayer = true,
                        AgreedOnDate = new DateTime(1950, 1, 1).AddDays(firstCommitment.Priority),
                        ApprenticeshipEmployerType = 1,
                        CreationDate = DateTime.Now,
                    });

                    foreach (var commitment in commitmentGroup)
                    {
                        apprenticeshipPriceEpisodes.Add(new ApprenticeshipPriceEpisode
                        {
                            ApprenticeshipId = firstCommitment.CommitmentId,
                            Cost = commitment.AgreedCost,
                            EndDate = commitment.EffectiveToDate,
                            Removed = false,
                            StartDate = commitment.EffectiveFromDate,
                            CreationDate = DateTime.Now,
                        });
                    }

                    if (firstCommitment.PaymentStatus == 2)
                    {
                        apprenticeshipPause.Add(new ApprenticeshipPause{ApprenticeshipId = firstCommitment.CommitmentId});
                    }
                }

                await Log($"Loaded {apprenticeships.Count} commitments");

                using(var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.Serializable,
                    Timeout = TimeSpan.FromMinutes(15)
                }, TransactionScopeAsyncFlowOption.Enabled))
                using (var v2Connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                {
                    await v2Connection.OpenAsync().ConfigureAwait(false);

                    using (var bulkCopy = new SqlBulkCopy(v2Connection))
                    using (var reader = ObjectReader.Create(apprenticeships))
                    {
                        await v2Connection.ExecuteAsync(V2Sql.DeleteData, commandTimeout: 3600).ConfigureAwait(false);
                        await Log("Deleted old data");

                        bulkCopy.DestinationTableName = "Payments2.Apprenticeship";
                        bulkCopy.BatchSize = 5000;
                        bulkCopy.BulkCopyTimeout = 3600;

                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AccountId", "AccountId"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EstimatedEndDate",
                            "EstimatedEndDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EstimatedStartDate",
                            "EstimatedStartDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FrameworkCode", "FrameworkCode"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("LegalEntityName",
                            "LegalEntityName"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PathwayCode", "PathwayCode"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ProgrammeType", "ProgrammeType"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Priority", "Priority"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StandardCode", "StandardCode"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Status", "Status"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StopDate", "StopDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TransferSendingEmployerAccountId",
                            "TransferSendingEmployerAccountId"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Ukprn", "Ukprn"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Uln", "Uln"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Id", "Id"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsLevyPayer", "IsLevyPayer"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AgreedOnDate", "AgreedOnDate"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ApprenticeshipEmployerType", "ApprenticeshipEmployerType"));
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CreationDate", "CreationDate"));

                        await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                    }
                    await Log("Saved apprenticeships");

                    using (var bulkCopy = new SqlBulkCopy(v2Connection))
                    using (var reader = ObjectReader.Create(apprenticeshipPriceEpisodes))
                    {
                        bulkCopy.DestinationTableName = "Payments2.ApprenticeshipPriceEpisode";
                        bulkCopy.BatchSize = 5000;
                        bulkCopy.BulkCopyTimeout = 3600;

                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ApprenticeshipId",
                            "ApprenticeshipId"));
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
                    var accountIds = accounts.Where(x => x.IsLevyPayer == false).Select(x => x.AccountId);
                    await v2Connection.ExecuteAsync(V2Sql.UpdateLevyPayerFlag, new { accountIds })
                        .ConfigureAwait(false);
                }

                await Log("Finished writing v2 levy accounts");
            }
        }
    }
}
