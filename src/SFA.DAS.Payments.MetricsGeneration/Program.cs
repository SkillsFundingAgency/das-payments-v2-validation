using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CommandLine;
using Dapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Kurukuru;
using SFA.DAS.Payments.MetricsGeneration.Resources;

namespace SFA.DAS.Payments.MetricsGeneration
{
    class Program
    {
        public const int CommandTimeout = 0; //wait indefinitely
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
                GenerateMetrics(options);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }


            return 0;
        }

        private static void GenerateMetrics(Options options)
        {
            var collectionPeriod = options.CollectionPeriod;
            var academicYear = options.AcademicYear ?? 1920;
            List<long> validDcJobIds = null;
            List<long> validDasUkPrns = null;
            if (options.ProcessingFilterMode == FilterMode.OnlySuccessful)
            {
                validDcJobIds = GetValidDcJobIds(collectionPeriod, academicYear);
                validDasUkPrns = GetValidDasUkPrns(validDcJobIds, collectionPeriod, academicYear);


            }

            var dcTotalEarnings = GetTotalDcEarnings(collectionPeriod, academicYear, validDasUkPrns);
            var dasTotalEarnings = GetTotalDasEarnings(collectionPeriod, academicYear, validDasUkPrns);

            SaveReport(dcTotalEarnings, dasTotalEarnings, validDasUkPrns, validDcJobIds, options);


        }

        private static void SaveReport(decimal dcTotalEarnings, (decimal earnings, DasTotals totals, DataLocksTotals dataLocksTotals) dasTotalEarnings, List<long> ukPrns, List<long> dcJobids ,Options options)
        {
            Spinner.Start("Writing Excel Sheet", spinner =>
            {

                var outputPath = ConfigurationManager.AppSettings["OutputPath"];
                using (var templateStream = ResourceHelpers.OpenResource(ExcelTemplate))
                {
                    using (var spreadsheet = new XLWorkbook(templateStream))
                    {
                        var sheet = spreadsheet.Worksheet("Das Payments");

                        AddFilterSheet(spreadsheet, options, dcJobids, ukPrns);

                        AddSummaryInfo(sheet, options.CollectionPeriod, options.AcademicYear);

                        //earnings
                        sheet.Cell(2, 1).Value = dasTotalEarnings.earnings; //DAS earnings
                        sheet.Cell(2, 2).Value = dcTotalEarnings; //DC earnings

                        //totals
                        sheet.Cell(5, 1).Value = dasTotalEarnings.totals.RpsThisMonth;
                        sheet.Cell(5, 2).Value = dasTotalEarnings.totals.PaymentPriorThisMonth;
                        sheet.Cell(5, 3).Value = dasTotalEarnings.totals.ExpectedPaymentAfterMonthEnd;
                        sheet.Cell(5, 4).Value = dasTotalEarnings.totals.TotalPaymentsThisMonth;
                        sheet.Cell(5, 5).Value = dasTotalEarnings.totals.TotalAct1;
                        sheet.Cell(5, 6).Value = dasTotalEarnings.totals.TotalAct2;
                        sheet.Cell(5, 7).Value = dasTotalEarnings.totals.TotalYtd;
                        sheet.Cell(5, 8).Value = dasTotalEarnings.totals.HeldBackCompletionPayments;

                        //datalocks
                        sheet.Cell(9, 1).Value = dasTotalEarnings.dataLocksTotals.DataLockedEarnings;
                        sheet.Cell(9, 2).Value = dasTotalEarnings.dataLocksTotals.DataLockedPayments;
                        sheet.Cell(9, 3).Value = dasTotalEarnings.dataLocksTotals.AdjustedDataLocks;

                        sheet.AdjustToContent();

                        var saveText = $"Saving data to spreadsheet to: {outputPath}";
                        spinner.Text = saveText;
                       string filename =  ExcelHelpers.SaveWorksheet(spreadsheet, outputPath);
                       spinner.Text = $"Saved report to: {filename}";
                    }
                }
            });
        }


        private static void AddSummaryInfo(IXLWorksheet sheet, short collectionPeriod,short? academicYear)
        {
            sheet.Cell(20, 1)
                .SetValue(
                    $"Report params: Collection Period:{collectionPeriod},Academic Year;" +
                    $": {academicYear}");
        }

        private static void AddFilterSheet(XLWorkbook spreadsheet, Options options, List<long> dcJobids, List<long> ukPrns)
       
        {
            if (options.ProcessingFilterMode == FilterMode.None)
                return;

            var ukprnSheet = spreadsheet.AddWorksheet("Included UKPRNS");
            ukprnSheet.AddRowData(ukPrns);

            var jobIdSheet = spreadsheet.AddWorksheet("Included DcJobIds");
            jobIdSheet.AddRowData(dcJobids);
        }

        private static (decimal earnings, DasTotals totals, DataLocksTotals dataLocksTotals) GetTotalDasEarnings(short collectionPeriod, short academicYear,
            List<long> validDasUkPrns)
        {

            string filterInsertStatement = CreateFilterInsertStatement(validDasUkPrns);

            decimal earnings = default;
            DasTotals totals = null;
            DataLocksTotals dataLocksTotals = null;

            Spinner.Start("Getting Das totals  ", spinner =>
            {
                var dasConStr =
                    ConfigurationManager.ConnectionStrings["DasConnectionString"].ConnectionString;

                var dasQuery = ResourceHelpers.ReadResource(DasQuery);
                dasQuery = dasQuery.Replace("<##InsertTemplate##>", filterInsertStatement);

                using (var dcConnection = new SqlConnection(dasConStr))
                {
                    var results  = dcConnection.QueryMultiple(dasQuery,
                        new {collectionperiod = collectionPeriod, academicyear = academicYear},
                        commandTimeout: CommandTimeout);
                     totals = results.ReadFirst<DasTotals>();
                     earnings = results.ReadFirst<decimal>();
                     dataLocksTotals = results.ReadFirst<DataLocksTotals>();
                }
            });
            //query to get total DC earnings
            return (earnings, totals, dataLocksTotals);
        }

        private static decimal GetTotalDcEarnings(short collectionPeriod, short academicYear, List<long> validDasUkPrns)
        {
            string filterInsertStatement = CreateFilterInsertStatement(validDasUkPrns);

            decimal totalEarnings = 0m;
            //query to get total DC earnings
            Spinner.Start("Getting DC total earnings ", spinner =>
            {
                var dasConStr =
                    ConfigurationManager.ConnectionStrings["DcConnectionString"].ConnectionString;
                var dcQuery = ResourceHelpers.ReadResource(DcQuery);
                dcQuery = dcQuery.Replace("<##InsertTemplate##>", filterInsertStatement);
                //var replaceToken = "<validDcJobIds>";
                //dcQuery = dcQuery.Replace(replaceToken, String.Join(",", validDcJobIds));
                using (var dcConnection = new SqlConnection(dasConStr))
                {
                    totalEarnings = dcConnection.Query<decimal>(dcQuery,
                        new {collectionperiod = collectionPeriod, academicyear = academicYear},
                        commandTimeout: CommandTimeout).FirstOrDefault();
                }
            });
            return totalEarnings;
        }



        private static List<long> GetValidDasUkPrns(List<long> validDcJobIds, short collectionPeriod,
            short academicYear)
        {

            List<long> ukPrns = null;
            Spinner.Start("Getting valid Das UKprns. ", spinner =>
            {
                var dasConStr =
                    ConfigurationManager.ConnectionStrings["DasConnectionString"].ConnectionString;
                var dcQuery = ResourceHelpers.ReadResource(DasValidUKprns);
                var replaceToken = "<validDcJobIds>";
                dcQuery = dcQuery.Replace(replaceToken, String.Join(",", validDcJobIds));
                using (var dcConnection = new SqlConnection(dasConStr))
                {
                    ukPrns = dcConnection.Query<long>(dcQuery,
                        new {collectionperiod = collectionPeriod, academicyear = academicYear},
                        commandTimeout: 5000).ToList();

                    if(ukPrns.Any())
                      ukPrns = ukPrns.Distinct().ToList();
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
                if(jobids.Any())
                    jobids = jobids.Distinct().ToList();
            });
            return jobids;
        }


        
        private static string CreateFilterInsertStatement(List<long> validDasUkPrns)
        {
            if (validDasUkPrns == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            int batchSize = 90;
            int skip = 0;

            var currentBatch =new  List<long>();
            while ((currentBatch = validDasUkPrns.Skip(skip).Take(batchSize).ToList()).Count > 0)
            {
                var ukPrns =String.Join(",", CreateValuesString(currentBatch));

                var template = $@"
                INSERT INTO @ukprnList 
                VALUES
                {ukPrns}
                ";

                sb.Append(template);

                skip += batchSize;
            }

            return sb.ToString();
        }

        private static List<string> CreateValuesString(List<long> currentBatch)
        {
            return currentBatch.Select(x => $"({x})").ToList();
        }
    }
}