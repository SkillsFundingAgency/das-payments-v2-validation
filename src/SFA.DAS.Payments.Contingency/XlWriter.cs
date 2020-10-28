using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using SFA.DAS.Payments.Contingency.DTO;

namespace SFA.DAS.Payments.Contingency
{
    public static class XlWriter
    {
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
    }
}
