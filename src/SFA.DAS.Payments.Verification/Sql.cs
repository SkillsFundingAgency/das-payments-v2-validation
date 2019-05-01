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

        private static string learnerSetup = string.Empty;

        public static async Task<List<long>> IncludedLearners(Inclusions inclusions)
        {
            var sql = GetInclusionSqlText(Inclusions.Act2BasicDay);
            using (var connection = Connection(PaymentSystem.V1))
            {
                var learners = (await connection.QueryAsync<long>(sql)).ToList();
                return learners;
            }
        }

        public static async Task<List<T>> Read<T>(PaymentSystem database, Script script)
        {
            database = PaymentSystem.V1;
            var sql = GetSqlText(database, script);
            
            using (var connection = Connection(database))
            {
                return (await connection.QueryAsync<T>(sql, commandTimeout:600)).ToList();
            }
        }

        public static async Task Write<T>(PaymentSystem database, IEnumerable<T> dataToWrite, string tableName)
        {
            var columns = typeof(T).GetProperties().Select(x => x.Name).ToArray();
            using (var connection = Connection(database))
            using (var bulkCopy = new SqlBulkCopy(connection) { DestinationTableName = $"[Verification].[{tableName}]" })
            using (var reader = ObjectReader.Create(dataToWrite, columns))
            {
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

        private static string GetInclusionSqlText(Inclusions inclusion)
        {
            var path = Path.Combine(BasePath, "SQL", "Inclusions", $"{inclusion.Description()}.sql");
            learnerSetup = File.ReadAllText(path);
            return learnerSetup;
        }

        private static string GetSqlText(PaymentSystem system, Script script)
        {
            var scriptPath = Path.Combine(BasePath, "SQL", system.ToString(), $"{script.ToString()}.sql");

            var result = new StringBuilder(learnerSetup);
            result.AppendLine();
            result.AppendLine();
            result.Append(File.ReadAllText(scriptPath));
            return result.ToString();
        }

        private static string BasePath => Path.GetDirectoryName(typeof(Sql).Assembly.Location);
    }
}
