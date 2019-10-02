using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Payments.Verification.Earnings.Constants;

namespace SFA.DAS.Payments.Verification.Earnings
{
    class Sql
    {
        private static readonly Dictionary<PaymentSystem, string> ConnectionStrings = new Dictionary<PaymentSystem, string>
        {
            {PaymentSystem.Earnings, ConfigurationManager.ConnectionStrings["Earnings"].ConnectionString},
            {PaymentSystem.Payments, ConfigurationManager.ConnectionStrings["Payments"].ConnectionString},
        };

        public static async Task<List<T>> Read<T>(
            PaymentSystem database, Script script)
        {
            var deliveryPeriods = new List<int> {1, 2};
        

        var sql = GetSqlText(script);

            using (var connection = Connection(database))
            {
                return (await connection.QueryAsync<T>(sql, new
                {
                    deliveryPeriods,
                }, commandTimeout:3600)).ToList();
            }
        }

        private static SqlConnection Connection(PaymentSystem system)
        {
            return new SqlConnection(ConnectionStrings[system]);
        }

        private static string GetSqlText(Script script)
        {
            var scriptPath = Path.Combine(BasePath, "SQL", $"{script.ToString()}.sql");
            var fileSql = File.ReadAllText(scriptPath);

            return fileSql;
        }

        private static string BasePath => Path.GetDirectoryName(typeof(Sql).Assembly.Location);
    }
}
