using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using ClosedXML.Excel;
using CommandLine;
using Dapper;
using Kurukuru;
using MoreLinq.Extensions;

namespace SFA.DAS.Payments.EarningsComparer
{
    class Program
    {
        internal const string DasQuery = "DasQuery.sql";
        internal const string LegacyDasQuery = "DasQueryLegacy.sql";
        internal const string DcQuery = "DcQuery.sql";
        internal const string ExcelTemplate = "Template.xlsx";
        internal const string BlackListFile = "blacklist.json";
        internal const string WhiteListFile = "whitelist.json";

        private static int Main(string[] args)
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
            if (options.UseLegacyMode && options.ProcessingStartTime == DateTime.MinValue)
            {
                Console.WriteLine("Processing start time is required when running in legacy mode.");
                return 1;
            }

            if (options.UseLegacyMode && options.ProcessingStartTime.TimeOfDay == TimeSpan.Zero)
            {
                Console.WriteLine("Time component required on Processing Start Time");
                return 1;
            }

            try
            {
                CalculateEarningComparisonMetric(options.CollectionPeriod, options.ProcessingStartTime,
                    options.ProcessingFilterMode, options.UseLegacyMode, options.AcademicYear);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }

            return 0;
        }

        private static void CalculateEarningComparisonMetric(short collectionPeriod, DateTime processingStartTime,
            FilterMode filterMode, bool useLegacyMode, short academicYear)
        {
            Spinner.Start("Getting required data. ", spinner =>
            {
                var dasConnectionString =
                    ConfigurationManager.ConnectionStrings["DasConnectionString"].ConnectionString;
                var dcConnectionString = ConfigurationManager.ConnectionStrings[$"DcEarnings{academicYear}ConnectionString"].ConnectionString;
                var outputPath = ConfigurationManager.AppSettings["OutputPath"];

                var dasQuery = ResourceHelpers.ReadResource(DasQuery);
                var legacyDasQuery = ResourceHelpers.ReadResource(LegacyDasQuery);
                var dcQuery = ResourceHelpers.ReadResource(DcQuery);

                IEnumerable<EarningsRow> dcData;
                IEnumerable<EarningsRow> dasData;

                spinner.Text = "Getting DC data";


                using (var dcConnection = new SqlConnection(dcConnectionString))
                {
                    dcData = dcConnection.Query<EarningsRow>(dcQuery,
                        new {collectionperiod = collectionPeriod},
                        commandTimeout: 5000);
                }
                spinner.Text = "Getting DAS data";
               

                using (var dasConnection = new SqlConnection(dasConnectionString))
                {
                    if (useLegacyMode)
                    {
                        dasData = dasConnection.Query<EarningsRow>(legacyDasQuery,
                            new
                            {
                                collectionperiod = collectionPeriod,
                                monthendStartTime = processingStartTime
                            },
                            commandTimeout: 0);
                    }
                    else
                    {
                        dasData = dasConnection.Query<EarningsRow>(dasQuery,
                            new
                            {
                                collectionperiod = collectionPeriod
                            },
                            commandTimeout: 0);
                    }
                }

                spinner.Text = "Calculating values";
              


                var joinedValues = dasData.FullJoin(
                        dcData,
                        earningsRow => new {earningsRow.Ukprn, earningsRow.ApprenticeshipContractType},
                        dasRow => new CombinedRow(dasRow.Ukprn, dasRow.ApprenticeshipContractType)
                            {DasRow = dasRow, DcRow = null},
                        dcRow => new CombinedRow(dcRow.Ukprn, dcRow.ApprenticeshipContractType)
                            {DasRow = null, DcRow = dcRow},
                        (dasRow, dcRow
                        ) => new CombinedRow(dasRow.Ukprn, dasRow.ApprenticeshipContractType)
                            {DasRow = dasRow, DcRow = dcRow}
                    )
                    .OrderBy(row => row.Ukprn)
                    .ThenBy(row => row.ApprenticeshipContractType)
                    .ToList();


                var filteredResults = FilterValues(filterMode, joinedValues, spinner);


                using (var templateStream = ResourceHelpers.OpenResource(ExcelTemplate))
                {
                    using (var spreadsheet = new XLWorkbook(templateStream))
                    {
                        var sheet = spreadsheet.Worksheet("Earnings Comparison");

                        AddFilterSheet(spreadsheet, filterMode, filteredResults.Item2);

                        AddSummaryInfo(sheet, collectionPeriod, processingStartTime);

                        WriteDataToSheet(sheet, filteredResults.Item1);

                        sheet.AdjustToContent();
                        sheet.SetAsTable(9, 1);

                        spinner.Text = $"Saving data to spreadsheet to: {outputPath}";
                      

                        ExcelHelpers.SaveWorksheet(spreadsheet, outputPath);

                        spinner.Text = "Spreadsheet saved.";
                        
                    }
                }
            });
        }

        private static void AddFilterSheet(XLWorkbook spreadsheet, FilterMode filterMode, List<long> filterItems)
        {
            if (filterMode == FilterMode.None)
                return;

            var filterSheet = filterMode == FilterMode.Whitelist
                ? spreadsheet.AddWorksheet("Whitelist")
                : spreadsheet.AddWorksheet("BlackList");
            filterSheet.AddRowData(filterItems);
        }


        private static (List<CombinedRow>, List<long>) FilterValues(FilterMode filterMode,
            List<CombinedRow> joinedValues, Spinner spinner)
        {
            if (filterMode == FilterMode.None)
            {
                spinner.Text = "No filtering configured";

                return (joinedValues, null);
            }
            spinner.Text = $"Filtering using: {filterMode}";

            var filterItems = GetFilterItems(filterMode);

            return filterMode == FilterMode.Blacklist
                ? (joinedValues.Where(jv => !filterItems.Contains(jv.Ukprn)).ToList(), filterItems)
                : (joinedValues.Where(jv => filterItems.Contains(jv.Ukprn)).ToList(), filterItems);
        }

        private static List<long> GetFilterItems(FilterMode filterMode)
        {
            return filterMode == FilterMode.Blacklist
                ? ResourceHelpers.LoadFilterValues(BlackListFile)
                : ResourceHelpers.LoadFilterValues(WhiteListFile);
        }


        private static void AddSummaryInfo(IXLWorksheet sheet, short collectionPeriod,
            DateTime processingStartTime)
        {
            sheet.Cell(1, 1)
                .SetValue(
                    $"Report params: Collection Period:{collectionPeriod}, processingStartTime:{processingStartTime:G}");
        }

        private static void WriteDataToSheet(IXLWorksheet sheet, List<CombinedRow> test)
        {
            var row = 10;
            foreach (var combinedRow in test)
            {
                sheet.Cell(row, 1).SetValue(combinedRow.Ukprn);
                sheet.Cell(row, 2).SetValue(combinedRow.ApprenticeshipContractType);
                sheet.Cell(row, 3).SetValue(combinedRow.DcRow?.AllTypes ?? 0m);
                sheet.Cell(row, 4).SetValue(combinedRow.DasRow?.AllTypes ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 5).SetValue(combinedRow.EarningsPercentage).SetAsPercentage();
                sheet.Cell(row, 6).SetValue(combinedRow.TotalsDifference);

                sheet.Cell(row, 7).SetValue(combinedRow.DcRow?.TT1 ?? 0m);
                sheet.Cell(row, 8).SetValue(combinedRow.DasRow?.TT1 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 9).SetValue(combinedRow.DcRow?.TT2 ?? 0m);
                sheet.Cell(row, 10).SetValue(combinedRow.DasRow?.TT2 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 11).SetValue(combinedRow.DcRow?.TT3 ?? 0m);
                sheet.Cell(row, 12).SetValue(combinedRow.DasRow?.TT3 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 13).SetValue(combinedRow.DcRow?.TT4 ?? 0m);
                sheet.Cell(row, 14).SetValue(combinedRow.DasRow?.TT4 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 15).SetValue(combinedRow.DcRow?.TT5 ?? 0m);
                sheet.Cell(row, 16).SetValue(combinedRow.DasRow?.TT5 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 17).SetValue(combinedRow.DcRow?.TT6 ?? 0m);
                sheet.Cell(row, 18).SetValue(combinedRow.DasRow?.TT6 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 19).SetValue(combinedRow.DcRow?.TT7 ?? 0m);
                sheet.Cell(row, 20).SetValue(combinedRow.DasRow?.TT7 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 21).SetValue(combinedRow.DcRow?.TT8 ?? 0m);
                sheet.Cell(row, 22).SetValue(combinedRow.DasRow?.TT8 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 23).SetValue(combinedRow.DcRow?.TT9 ?? 0m);
                sheet.Cell(row, 24).SetValue(combinedRow.DasRow?.TT9 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 25).SetValue(combinedRow.DcRow?.TT10 ?? 0m);
                sheet.Cell(row, 26).SetValue(combinedRow.DasRow?.TT10 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 27).SetValue(combinedRow.DcRow?.TT11 ?? 0m);
                sheet.Cell(row, 28).SetValue(combinedRow.DasRow?.TT11 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 29).SetValue(combinedRow.DcRow?.TT12 ?? 0m);
                sheet.Cell(row, 30).SetValue(combinedRow.DasRow?.TT12 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 31).SetValue(combinedRow.DcRow?.TT13 ?? 0m);
                sheet.Cell(row, 32).SetValue(combinedRow.DasRow?.TT13 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 33).SetValue(combinedRow.DcRow?.TT14 ?? 0m);
                sheet.Cell(row, 34).SetValue(combinedRow.DasRow?.TT14 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 35).SetValue(combinedRow.DcRow?.TT15 ?? 0m);
                sheet.Cell(row, 36).SetValue(combinedRow.DasRow?.TT15 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                sheet.Cell(row, 37).SetValue(combinedRow.DcRow?.TT16 ?? 0m);
                sheet.Cell(row, 38).SetValue(combinedRow.DasRow?.TT16 ?? 0m)
                    .SetRedIfNotEqualToPrevious()
                    .SetGreenIfEqualToPrevious();

                row++;
            }
        }
    }
}