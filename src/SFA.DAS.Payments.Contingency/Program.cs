using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using SFA.DAS.Payments.Contingency.CongingencyStrategies;
using SFA.DAS.Payments.Contingency.DTO;

using SC = System.Data.SqlClient;
using AD = Microsoft.IdentityModel.Clients.ActiveDirectory;
using TT = System.Threading.Tasks;

namespace SFA.DAS.Payments.Contingency
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                //var provider = new ActiveDirectoryAuthProvider();
                //SC.SqlAuthenticationProvider.SetProvider(
                //    SC.SqlAuthenticationMethod.ActiveDirectoryInteractive, provider);
                //ConnectToMfaSql();

                var period = GetPeriod();

                var contingencyStrategies = new List<IProduceContingencyPayments>
                {
                    new Act2WithPayments(),
                };

                foreach (var contingencyStrategy in contingencyStrategies)
                {
                    await contingencyStrategy.GenerateContingencyPayments(period);
                }

                Console.WriteLine("Finished - press enter to exit...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        

        private static int GetPeriod()
        {
            while (true)
            {
                Console.WriteLine(
                    "Please enter the collection period: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 or 14");
                var chosenPeriod = Console.ReadLine();
                if (!int.TryParse(chosenPeriod, out var collectionPeriod) || collectionPeriod < 1 ||
                    collectionPeriod > 14)
                {
                    Console.WriteLine($"Invalid collection period: '{chosenPeriod}'.");
                    continue;
                }

                return collectionPeriod;
            }
        }

        public static void WriteRawResults(IXLWorksheet sheet, List<Earning> earnings)
        {
            var row = 2;
            foreach (var earning in earnings.OrderBy(x => x.Ukprn).ThenBy(x => x.Uln))
            {
                sheet.Cell(row, "A").Value = earning.Ukprn;
                sheet.Cell(row, "B").Value = earning.Uln;
                sheet.Cell(row, "C").Value = earning.FundingLineType;

                sheet.Cell(row, "D").Value = earning.Amount;
                sheet.Cell(row, "E").Value = earning.OneToThree;
                sheet.Cell(row, "F").Value = earning.Incentives;
                row++;
            }
        }

        public static void WriteRawResults(IXLWorksheet sheet, List<CalculatedPayment> payments)
        {
            var row = 2;
            foreach (var payment in payments.OrderBy(x => x.Ukprn).ThenBy(x => x.Uln))
            {
                sheet.Cell(row, "A").Value = payment.Ukprn;
                sheet.Cell(row, "B").Value = payment.Uln;
                sheet.Cell(row, "C").Value = payment.FundingLineType;

                sheet.Cell(row, "D").Value = payment.TotalAmount;
                sheet.Cell(row, "E").Value = payment.OnProgPayments;
                sheet.Cell(row, "F").Value = payment.IncentivePayments;
                row++;
            }
        }

        public static void WriteToTable(IXLWorksheet sheet, List<Earning> earnings)
        {
            var row = 2;
            var groupedEarnings = earnings.GroupBy(x => new {x.Ukprn, x.FundingLineType}).OrderBy(x => x.Key.Ukprn);
            foreach (var groupedEarning in groupedEarnings)
            {
                sheet.Cell(row, "A").Value = groupedEarning.Key.Ukprn;
                sheet.Cell(row, "B").Value = groupedEarning.Key.FundingLineType;
                sheet.Cell(row, "C").Value = groupedEarning.Sum(x => x.Amount);
                sheet.Cell(row, "D").Value = groupedEarning.Sum(x => x.OneToThree);
                sheet.Cell(row, "E").Value = groupedEarning.Sum(x => x.Incentives);
                row++;
            }
        }

        public static void WriteToSummarisedTable(IXLWorksheet sheet, List<CalculatedPayment> payments)
        {
            var row = 2;
            var groupedPayments = payments.GroupBy(x => new {x.Ukprn, x.FundingLineType}).OrderBy(x => x.Key.Ukprn);
            foreach (var groupedEarning in groupedPayments)
            {
                sheet.Cell(row, "A").Value = groupedEarning.Key.Ukprn;
                sheet.Cell(row, "B").Value = groupedEarning.Key.FundingLineType;
                sheet.Cell(row, "C").Value = groupedEarning.Sum(x => x.TotalAmount);
                sheet.Cell(row, "D").Value = groupedEarning.Sum(x => x.OnProgPayments);
                sheet.Cell(row, "E").Value = groupedEarning.Sum(x => x.IncentivePayments);
                row++;
            }


        }
        private static void ConnectToMfaSql()
        {
            var serverName = "";
            var databaseName = "DASPayments";
            var builder = new SC.SqlConnectionStringBuilder();

            builder["Data Source"] = serverName;
            builder["Initial Catalog"] = databaseName;
            builder["TrustServerCertificate"] = true;

            builder.Authentication = SC.SqlAuthenticationMethod.ActiveDirectoryInteractive;
            var sqlConnection = new SC.SqlConnection(builder.ConnectionString);
            sqlConnection.Open();
        }

        public static string ClientId = "";

    }


   public class ActiveDirectoryAuthProvider : SC.SqlAuthenticationProvider
    {
        public override async TT.Task<SC.SqlAuthenticationToken>
            AcquireTokenAsync(SC.SqlAuthenticationParameters parameters)
        {
            AD.AuthenticationContext authContext =
                new AD.AuthenticationContext(parameters.Authority);
            authContext.CorrelationId = parameters.ConnectionId;
            AD.AuthenticationResult result;

            result = await authContext.AcquireTokenAsync(
                parameters.Resource, // "https://database.windows.net/"
                Program.ClientId,
                new Uri("https://ldatabase.windows.net"),
                new AD.PlatformParameters(AD.PromptBehavior.SelectAccount)
                );
            return new SC.SqlAuthenticationToken(result.AccessToken, result.ExpiresOn);
        }

        public override bool IsSupported(SC.SqlAuthenticationMethod authenticationMethod)
        {
            return authenticationMethod == SC.SqlAuthenticationMethod.ActiveDirectoryIntegrated
                   || authenticationMethod == SC.SqlAuthenticationMethod.ActiveDirectoryInteractive
                   || authenticationMethod == SC.SqlAuthenticationMethod.ActiveDirectoryPassword;
        }
    }
}
