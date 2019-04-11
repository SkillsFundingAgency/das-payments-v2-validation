using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Payments.Verification.Constants;

namespace SFA.DAS.Payments.Verification
{
    class Sql
    {
        private static readonly Dictionary<PaymentSystem, string> ConnectionStrings = new Dictionary<PaymentSystem, string>
        {
            {PaymentSystem.V1, ConfigurationManager.ConnectionStrings["V1"].ConnectionString},
            {PaymentSystem.V2, ConfigurationManager.ConnectionStrings["V2"].ConnectionString},
        };

        public static async Task<List<T>> Execute<T>(PaymentSystem database, Script script)
        {
            var sql = GetSqlText(database, script);

            using (var connection = Connection(database))
            {
                return (await connection.QueryAsync<T>(sql)).ToList();
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
