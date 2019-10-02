using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using SFA.DAS.Payments.Contingency.CongingencyStrategies;
using SFA.DAS.Payments.Contingency.DTO;

namespace SFA.DAS.Payments.Contingency
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var contingencyStrategies = new List<IProduceContingencyPayments>
                {
                    new R13Payments(),
                    new UsingLiveDatalocksAct1Tt1To3(),
                    new UsingLiveDatalocksAct1Tt4To16(),
                    new Act2FromEarnings(),
                };

                foreach (var contingencyStrategy in contingencyStrategies)
                {
                    await contingencyStrategy.GenerateContingencyPayments();
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

        public static void WriteToTable(IXLWorksheet sheet, List<Earning> earnings)
        {
            var row = 2;
            var groupedEarnings = earnings.GroupBy(x => new { x.Ukprn, x.FundingLineType }).OrderBy(x => x.Key.Ukprn);
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
    }
}
