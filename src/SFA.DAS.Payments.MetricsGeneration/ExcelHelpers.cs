using System;
using System.Collections;
using ClosedXML.Excel;

namespace SFA.DAS.Payments.MetricsGeneration
{
    internal static class ExcelHelpers
    {

        internal static void AddRowData<T>(this IXLWorksheet sheet, T filterItems, int row = 1, int column = 1) where T : IList
        { 
            foreach (var filterItem in filterItems)
            {
                sheet.Cell(row, column).SetValue(filterItem);
                row++;
            }
        }

        internal static void AdjustToContent(this IXLWorksheet sheet)
        {
            sheet.Columns().AdjustToContents();
            sheet.Rows().AdjustToContents();
        }
        internal static string SaveWorksheet(XLWorkbook workbook, string path)
        {
            workbook.CalculationOnSave = true;
            var date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");

            var filename = $"{path}\\MetricsReport-{date}.xlsx";
           
            workbook.SaveAs(filename);

            return filename;
        }

    }
}
