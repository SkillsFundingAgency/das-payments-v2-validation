using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using FastMember;
using SFA.DAS.Payments.Verification.Constants;
using SFA.DAS.Payments.Verification.Utilities;

namespace SFA.DAS.Payments.Verification
{
    class Sql
    {
        private static readonly Dictionary<PaymentSystem, string> ConnectionStrings = new Dictionary<PaymentSystem, string>
        {
            {PaymentSystem.V1, ConfigurationManager.ConnectionStrings["V1"].ConnectionString},
            {PaymentSystem.V2, ConfigurationManager.ConnectionStrings["V2"].ConnectionString},
            {PaymentSystem.Output, ConfigurationManager.ConnectionStrings["Output"].ConnectionString},
        };

        public static async Task InitialiseLearnerTables(Inclusions inclusions, List<long> ukprns)
        {
            var restrictUkprns = ukprns.Any() ? 1 : 0;

            var sql = GetInclusionSqlText(PaymentSystem.V1, inclusions);
            var connection = Connection(PaymentSystem.V1);
            await connection.OpenAsync();
            await connection.ExecuteAsync(sql, new {ukprns, restrictUkprns});
            
            sql = GetInclusionSqlText(PaymentSystem.V2, inclusions);
            connection = Connection(PaymentSystem.V2);
            await connection.OpenAsync();
            await connection.ExecuteAsync(sql, new {ukprns, restrictUkprns });
        }

        public static async Task<int> InitialiseJob()
        {
            using (var connection = Connection(PaymentSystem.Output))
            {
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<int>(
                    @"  INSERT INTO [Verification].[Jobs] (CreatedAt) VALUES (@createdAt) 
                            SELECT @@identity",
                    new {createdAt = DateTime.Now});
            }
        }

        public static async Task<List<T>> Read<T>(PaymentSystem database, Script script, List<int> periods)
        {
            //database = PaymentSystem.V1;
            var sql = GetSqlText(database, script);
            
            using (var connection = Connection(database))
            {
                return (await connection.QueryAsync<T>(sql, new {periods}, commandTimeout:600)).ToList();
            }
        }

        public static async Task Write<T>(PaymentSystem database, IEnumerable<T> dataToWrite, string tableName)
        {
            var columns = typeof(T).GetProperties().Select(x => x.Name).ToArray();
            using (var connection = Connection(database))
            using (var bulkCopy = new SqlBulkCopy(connection))
            using (var reader = ObjectReader.Create(dataToWrite, columns))
            {
                bulkCopy.DestinationTableName = $"[Verification].[{tableName}]";
                bulkCopy.BulkCopyTimeout = 1200;
                bulkCopy.BatchSize = 10_000;

                foreach (var column in columns)
                {
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column, column));
                }

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                await bulkCopy.WriteToServerAsync(reader).ConfigureAwait(false);
            }
        }

        private static SqlConnection Connection(PaymentSystem system)
        {
            return new SqlConnection(ConnectionStrings[system]);
        }

        private static string GetInclusionSqlText(PaymentSystem system, Inclusions inclusion)
        {
            var path = Path.Combine(BasePath, "SQL", "Inclusions", system.ToString(), $"{inclusion.Description()}.sql");
            return File.ReadAllText(path);
        }

        private static string GetSqlText(PaymentSystem system, Script script)
        {
            var scriptPath = Path.Combine(BasePath, "SQL", system.ToString(), $"{script.ToString()}.sql");

            var result = new StringBuilder(); 
            result.AppendLine();
            result.AppendLine();
            result.Append(File.ReadAllText(scriptPath));
            return result.ToString();
        }

        private static string BasePath => Path.GetDirectoryName(typeof(Sql).Assembly.Location);
    }
}
