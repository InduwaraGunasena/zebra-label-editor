using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zebra_label_editor.Services.Interfaces
{
    // The "I" at the start is a naming convention for "Interface"
    public interface IZplService
    {
        // 1. Definition: We need a method that takes a file path and returns the text content
        string LoadZplTemplate(string filePath);

        // 2. Definition: We need a method that finds [Placeholders]
        List<string> ExtractPlaceholders(string zplContent);

        // 3. Definition: We need a method that swaps [Placeholders] for real data
        string MergeData(string zplTemplate, Dictionary<string, string> dataMap);

        // 4. Definition: We need a method that validates ZPL content
        bool IsValidZpl(string zplContent);

        // 5. Definition: We need a method that validates ZPL content for correct editing placeholders
        bool IsValidZplWithEditablePlaceholders(string zplContent);
    }
}
