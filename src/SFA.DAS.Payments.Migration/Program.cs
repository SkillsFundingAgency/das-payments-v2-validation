using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FastMember;
using SFA.DAS.Payments.Migration.Constants;
using SFA.DAS.Payments.Migration.DTO;

namespace SFA.DAS.Payments.Migration
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var period = 0;
                while (period == 0)
                {
                    Console.WriteLine("Please enter the period to initialise");
                    Console.WriteLine("");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out var inputAsInteger))
                    {
                        Console.WriteLine("Please enter a number between 1 and 11");
                    }
                    else
                    {
                        if (CollectionPeriods.CollectionPeriodDates.ContainsKey(inputAsInteger))
                        {
                            period = inputAsInteger;
                        }
                        else
                        {
                            Console.WriteLine("Please enter a number between 1 and 11");
                        }
                    }
                }

                using (var connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["V1Commitments"].ConnectionString))
                {
                    var collectionPeriodDate = CollectionPeriods.CollectionPeriodDates[period];
                    var commitments = await connection
                        .QueryAsync<Commitment>(V1Sql.Commitments, new {inputDate = collectionPeriodDate})
                        .ConfigureAwait(false);

                    var commitmentsById = commitments.GroupBy(x => x.CommitmentId);
                    var apprenticeships = new List<Apprenticeship>();
                    var apprenticeshipPriceEpisodes = new List<ApprenticeshipPriceEpisode>();
                    var apprenticeshipId = 1;

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
                            Id = apprenticeshipId,
                            IsLevyPayer = true,
                            AgreedOnDate = new DateTime(2019, 1, 1),
                        });

                        foreach (var commitment in commitmentGroup)
                        {
                            apprenticeshipPriceEpisodes.Add(new ApprenticeshipPriceEpisode
                            {
                                ApprenticeshipId = apprenticeshipId,
                                Cost = commitment.AgreedCost,
                                EndDate = commitment.EffectiveToDate,
                                Removed = false,
                                StartDate = commitment.EffectiveFromDate,
                            });
                        }

                        apprenticeshipId++;
                    }

                    Console.WriteLine($"Loaded {apprenticeships.Count} commitments");

                    using (var v2Connection =
                        new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                    {
                        await v2Connection.OpenAsync().ConfigureAwait(false);

                        using (var bulkCopy = new SqlBulkCopy(v2Connection))
                        using (var reader = ObjectReader.Create(apprenticeships))
                        {
                            await v2Connection.ExecuteAsync(V2Sql.DeleteData).ConfigureAwait(false);
                            Console.WriteLine("Deleted old data");
                            
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

                            await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                        }

                        Console.WriteLine("Saved apprenticeships");

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

                            await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                        }

                        Console.WriteLine("Saved apprenticeship price episodes");
                    }
                }

                using (var connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["V1Accounts"].ConnectionString))
                {
                    // Data already deleted and identity insert is on
                    var v1Accounts = await connection.QueryAsync<V1Account>(V1Sql.Accounts);
                    var accounts = new List<LevyAccount>();
                    var sequence = 1;

                    foreach (var v1Account in v1Accounts)
                    {
                        accounts.Add(new LevyAccount
                        {
                            AccountId = v1Account.AccountId,
                            AccountName = v1Account.AccountName,
                            Balance = v1Account.Balance,
                            IsLevyPayer = v1Account.IsLevyPayer,
                            TransferAllowance = v1Account.TransferAllowance,
                            SequenceId = sequence++,
                            AccountHashId = "NOTUSED",
                        });
                    }

                    Console.WriteLine("Finished loading v1 accounts");

                    using (var v2Connection =
                        new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                    {
                        await v2Connection.OpenAsync().ConfigureAwait(false);

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
                            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SequenceId", "SequenceId"));
                            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AccountHashId", "AccountHashId"));

                            await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
                        }

                        // Update the IsLevyPayer flag on the commitments
                        var accountIds = accounts.Where(x => x.IsLevyPayer == false).Select(x => x.AccountId);
                        await v2Connection.ExecuteAsync(V2Sql.UpdateLevyPayerFlag, new {accountIds})
                            .ConfigureAwait(false);
                    }

                    Console.WriteLine("Finished writing v2 levy accounts");
                }

                Console.WriteLine("Finished - press any key to exit");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }
    }
}
