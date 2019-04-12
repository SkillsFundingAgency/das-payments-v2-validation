using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FastMember;
using SFA.DAS.Payments.Verification.Constants;

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

        public static async Task<List<T>> Read<T>(PaymentSystem database, Script script)
        {
            var sql = GetSqlText(database, script);

            using (var connection = Connection(database))
            {
                return (await connection.QueryAsync<T>(sql)).ToList();
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

        private static string GetSqlText(PaymentSystem system, Script script)
        {
            string scriptPath;

            if (script == Script.Inclusions)
            {
                scriptPath = Path.Combine(BasePath, "SQL",  "Inclusions", "BasicDayACT2.sql");
            }
            else
            {
                scriptPath = Path.Combine(BasePath, "SQL", system.ToString(), $"{script.ToString()}.sql");
            }

            return File.ReadAllText(scriptPath);
        }

        private static string BasePath => Path.GetDirectoryName(typeof(Sql).Assembly.Location);

    }
}
