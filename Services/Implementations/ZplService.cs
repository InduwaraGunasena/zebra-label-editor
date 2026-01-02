using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;
using zebra_label_editor.Services.Interfaces;

namespace zebra_label_editor.Services.Implementations
{
    // " : IZplService" means this class AGREES to follow the IZplService rules
    public class ZplService : IZplService
    {
        // Regex is a pattern matcher. This pattern looks for text inside brackets [LikeThis]
        private readonly Regex _placeholderPattern = new Regex(@"\[(.*?)\]", RegexOptions.Compiled);

        public string LoadZplTemplate(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("The ZPL template file was not found.", filePath);

            return File.ReadAllText(filePath);
        }

        public List<string> ExtractPlaceholders(string zplContent)
        {
            if (string.IsNullOrWhiteSpace(zplContent))
                return new List<string>();

            // Find matches and convert them to a clean list of unique strings
            var matches = _placeholderPattern.Matches(zplContent);
            return matches.Cast<Match>()
                          .Select(m => m.Value)
                          .Distinct()
                          .ToList();
        }

        public string MergeData(string zplTemplate, Dictionary<string, string> dataMap)
        {
            if (string.IsNullOrWhiteSpace(zplTemplate) || dataMap == null)
                return zplTemplate;

            string finalZpl = zplTemplate;

            // Loop through our data pairs and replace them in the text
            foreach (var item in dataMap)
            {
                if (finalZpl.Contains(item.Key))
                {
                    finalZpl = finalZpl.Replace(item.Key, item.Value);
                }
            }

            return finalZpl;
        }
    }
}
