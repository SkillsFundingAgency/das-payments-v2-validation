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


        public const int CommandTimeout = 0; //wait indefinitely
        static async Task<int> Main(string[] args)
        {
            var collectionPeriod = await GetPeriod();
            await Log("Please enter the academic year");
            var academicYearAsString = Console.ReadLine();
            GETYEAR:
            if (!int.TryParse(academicYearAsString, out var academicYear))
            {
                await Log("Couldn't understand the academic year, please enter in the form 1920, 2021 etc");
                goto GETYEAR;
            }

            try
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["V2"].ConnectionString))
                {
                    await connection.OpenAsync();
                    var numberOfActiveJobs = await connection.ExecuteScalarAsync<int>(Sql.CheckIfJobIsRunning, commandTimeout:CommandTimeout);
                    if (numberOfActiveJobs > 0)
                    {
                        await Log("There are active jobs running - aborting cleanup");
                    }
                    else
                    {
                        await connection.ExecuteAsync(Sql.CleanAuditForPeriod, new { collectionPeriod, academicYear }, commandTimeout:CommandTimeout);
                        await Log("Completed");
                    }
                }
            }
            catch (Exception e)
            {
                await Log("An exception occurred. The details can be found below.");
                Console.WriteLine();
                await Log(e.Message);
                Console.WriteLine();

                await Log("Unrecoverable exception occurred. Please view exception details above and press any key.");
                
                Console.ReadKey();
                return 1;
            }

            await Log("Press enter to quit");
            Console.ReadLine();
            return 0;
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
