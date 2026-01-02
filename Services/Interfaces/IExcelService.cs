using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zebra_label_editor.Services.Interfaces
{
    public interface IExcelService
    {
        // Opens the file and returns the list of column headers (e.g., "Product Name", "Price")
        List<string> GetColumnHeaders(string filePath);
    }
}
