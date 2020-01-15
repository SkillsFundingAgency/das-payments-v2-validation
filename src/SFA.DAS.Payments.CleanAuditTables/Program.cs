using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Payments.CleanAuditTables.Constants;

namespace SFA.DAS.Payments.CleanAuditTables
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var collectionPeriod = await GetPeriod();
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
            {
                await connection.OpenAsync();
                var numberOfActiveJobs = await connection.ExecuteScalarAsync<int>(Sql.CheckIfJobIsRunning);
                if (numberOfActiveJobs > 0)
                {
                    await Log("There are active jobs running - aborting cleanup");
                }
                else
                {
                    await connection.ExecuteAsync(Sql.CleanAuditForPeriod, new { collectionPeriod });
                    await Log("Completed");
                }
            }

            await Log("Press enter to quit");
            Console.ReadLine();
        }

        static async Task Log(string message)
        {
            using (var file = File.AppendText("log.txt"))
            {
                await file.WriteLineAsync(message);
            }
            Console.WriteLine(message);
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
    }
}
