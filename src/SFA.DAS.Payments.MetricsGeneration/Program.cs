using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SFA.DAS.Payments.MetricsGeneration
{
    class Program
    {
        internal const string DasQuery = "MetricsQueryJobId.sql";
        internal const string DcQuery = "MetricsDCEarnings.sql";
        internal const string ExcelTemplate = "MetricsTemplate.xlsx";

        static int Main(string[] args)
        {
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

            List<long> validDasUkPrns = GetValidDasUkPrns(validDcJobIds);

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
        

        private static List<long> GetValidDasUkPrns(List<long> validDcJobIds)
        {
            return new List<long>();
        }

        private static List<long> GetValidDcJobIds(short collectionPeriod, short academicYear)
        {
            return new List<long>();
        }
    }
}