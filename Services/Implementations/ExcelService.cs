using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExcelDataReader;
using System.Data;
using System.IO;
using zebra_label_editor.Services.Interfaces;

namespace zebra_label_editor.Services.Implementations
{
    public class ExcelService : IExcelService
    {
        public List<string> GetColumnHeaders(string filePath)
        {
            // This line is required once for ExcelDataReader to work in .NET Core/.NET 8
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Convert the sheet to a DataSet (Table)
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true // Treat the first row as headers
                        }
                    });

                    // Get the first table (Worksheet 1)
                    DataTable table = result.Tables[0];

                    // Extract the column names into a List<string>
                    List<string> headers = new List<string>();
                    foreach (DataColumn column in table.Columns)
                    {
                        headers.Add(column.ColumnName);
                    }

                    return headers;
                }
            }
        }
    }
}
