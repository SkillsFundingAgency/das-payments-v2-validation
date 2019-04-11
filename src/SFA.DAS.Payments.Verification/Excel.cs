using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using FastMember;

namespace SFA.DAS.Payments.Verification
{
    static class Excel
    {
        /// <summary>
        /// Creates an stream containing an excel document with one sheet per rows of <param name="inputs"></param>
        /// </summary>
        /// <param name="inputs">a list of enumerable rows with the name of the worksheet</param>
        public static Stream CreateExcelDocumentWithSheets(params (object rows, string name)[] inputs)
        {
            var stream = new MemoryStream();
            var document = new XLWorkbook();
            foreach (var data in inputs)
            {
                var type = data.rows.GetType();
                
                var innerType = type.GetGenericArguments().First();
                var dataTable = data.rows.TransformToDataTable(innerType);
                document.AddWorksheet(dataTable, data.name);
                Log.Write("Completed worksheet");
            }

            document.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        static DataTable TransformToDataTable(this object source, Type type)
        {
            var output = new DataTable();
            var properties = type.GetProperties();
            var accessor = TypeAccessor.Create(type);

            foreach (var property in properties)
            {
                output.Columns.Add(property.Name);
            }

            var enumerableSource = source as IEnumerable;

            if (enumerableSource == null)
            {
                throw new ArgumentException("Please ensure that types are all IEnumerables");
            }

            var progress = 0;
            foreach (var line in (dynamic)source)
            {
                var row = output.NewRow();
                foreach (var property in properties)
                {
                    row[property.Name] = accessor[line, property.Name];
                }

                output.Rows.Add(row);
                progress++;
                if (progress % 1000 == 0)
                {
                    Log.Write($"Processed: {progress} rows");
                }
            }

            return output;
        }
    }
}
