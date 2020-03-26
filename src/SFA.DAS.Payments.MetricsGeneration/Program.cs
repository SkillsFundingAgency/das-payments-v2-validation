using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Dapper;
using Kurukuru;
using SFA.DAS.Payments.MetricsGeneration.Resources;

namespace SFA.DAS.Payments.MetricsGeneration
{
    class Program
    {
        internal const string DasValidUKprns = "DasValidUkprns.sql";
        internal const string DCValidJobIds = "DCValidJobIds.sql";
        internal const string DasQuery = "MetricsQueryJobId.sql";
        internal const string DcQuery = "MetricsDCEarnings.sql";
        internal const string ExcelTemplate = "MetricsTemplate.xlsx";

        static int Main(string[] args)
        {
            Console.WriteLine("Metric Generator");
            Console.WriteLine(new String('-', 500));


            Parser argParser = new Parser(settings =>
            {
                settings.AutoHelp = true;
                settings.HelpWriter = Console.Out;
                settings.CaseInsensitiveEnumValues = true;
            });

            return argParser.ParseArguments<Options>(args)
                .MapResult(RunAndReturnExitCode, ShowOptions);
        }

        public static int ShowOptions(IEnumerable<Error> errors)
        {
            return 1;
        }


        public static int RunAndReturnExitCode(Options options)
        {
            try
            {
                GenerateMetrics(options.CollectionPeriod, options.AcademicYear);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }


            return 0;
        }

        private static void GenerateMetrics(short collectionPeriod, short academicYear = 1920)
        {
            //get valid ukprn that have passed on both DC and DAS
            List<long> validDcJobIds = GetValidDcJobIds(collectionPeriod, academicYear);

            List<long> validDasUkPrns = GetValidDasUkPrns(validDcJobIds, collectionPeriod, academicYear);

            decimal dcTotalEarnings = GetTotalDcEarnings(collectionPeriod, academicYear, validDasUkPrns);
            decimal dasTotalEarnings = GetTotalDasEarnings(collectionPeriod, academicYear, validDasUkPrns);

            DataLocksTotals dataLocksTotals = GetDataLocksTotals(collectionPeriod, academicYear, validDasUkPrns);

            DasTotals dasTotals = GetDasTotals(collectionPeriod, academicYear, validDasUkPrns);


        }

        private static DasTotals GetDasTotals(short collectionPeriod, short academicYear, List<long> validDasUkPrns)
        {
            return new DasTotals();
        }

        private static DataLocksTotals GetDataLocksTotals(short collectionPeriod, short academicYear, List<long> validDasUkPrns)
        {
            return new DataLocksTotals();
        }

        private static decimal GetTotalDasEarnings(short collectionPeriod, short academicYear, List<long> validDasUkPrns)
        {
            //query to get total DC earnings
            return 0;
        }

        private static decimal GetTotalDcEarnings(short collectionPeriod, short academicYear, List<long> validDasUkPrns)
        {
            //query to get total DC earnings
            return 0;
        }
        

        private static List<long> GetValidDasUkPrns(List<long> validDcJobIds,short collectionPeriod, short academicYear)
        {
            List<long> ukPrns = null;
            Spinner.Start("Getting valid Das UKprns. ", spinner =>
            {
                var dasConStr =
                    ConfigurationManager.ConnectionStrings["DasConnectionString"].ConnectionString;
                var dcQuery = ResourceHelpers.ReadResource(DasValidUKprns);
                 var replaceToken = "<validDcJobIds>";
                dcQuery =  dcQuery.Replace(replaceToken, String.Join(",", validDcJobIds));
                using (var dcConnection = new SqlConnection(dasConStr))
                {
                    ukPrns = dcConnection.Query<long>(dcQuery,
                        new {collectionperiod = collectionPeriod, academicyear = academicYear},
                        commandTimeout: 5000).ToList();
                }
            });
            return ukPrns;
        }

        private static List<long> GetValidDcJobIds(short collectionPeriod, short academicYear)
        {  
            List<long> jobids = null;
            Spinner.Start("Getting valid DC JobiIds. ", spinner =>
            {
                var jobManagementConnectionString =
                    ConfigurationManager.ConnectionStrings["JobManagementConnectionString"].ConnectionString;
                var dcQuery = ResourceHelpers.ReadResource(DCValidJobIds);

                using (var dcConnection = new SqlConnection(jobManagementConnectionString))
                {
                    jobids = dcConnection.Query<long>(dcQuery,
                        new {collectionperiod = collectionPeriod, academicyear = academicYear},
                        commandTimeout: 5000).ToList();
                }

            });
            return jobids;
        }
    }
}