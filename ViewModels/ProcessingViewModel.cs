using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using ExcelDataReader;
using System.Data;
using System.IO;
using zebra_label_editor.Models;
using zebra_label_editor.Services.Implementations; // Need ZplService and RawPrinterHelper

namespace zebra_label_editor.ViewModels
{
    public partial class ProcessingViewModel : ObservableObject
    {
        public Action? NavigateFinishRequested;

        private readonly ZplService _zplService = new ZplService(); // Or inject via constructor

        [ObservableProperty]
        private string _statusMessage = "Initializing...";

        [ObservableProperty]
        private double _progressValue = 0;

        [ObservableProperty]
        private bool _isFinished = false;

        // --- PRINTING LOGIC ---
        public async void StartPrinting(string zplTemplate, string excelPath, List<MappingItem> mapping, string printerName)
        {
            IsFinished = false;
            ProgressValue = 0;

            await Task.Run(async () =>
            {
                var dataTable = ReadExcel(excelPath);
                int totalRows = dataTable.Rows.Count;

                for (int i = 0; i < totalRows; i++)
                {
                    // 1. Create the Data Dictionary for this row
                    var rowData = GetRowData(dataTable.Rows[i], mapping);

                    // 2. Merge Data
                    string labelZpl = _zplService.MergeData(zplTemplate, rowData);

                    // 3. Send to Printer (Raw ZPL)
                    try
                    {
                        bool success = RawPrinterHelper.SendStringToPrinter(printerName, labelZpl);
                        if (!success) throw new Exception("Printer error.");
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"Error printing label {i + 1}: {ex.Message}";
                        return; // Stop on error?
                    }

                    // 4. Update Progress
                    UpdateProgress(i + 1, totalRows, "Printing");

                    // Small delay to prevent UI freezing / Spooler flooding
                    await Task.Delay(50);
                }

                FinalizeProcess("Printing Completed!");
            });
        }

        // --- SAVING LOGIC ---
        public async void StartSaving(string zplTemplate, string excelPath, List<MappingItem> mapping, string saveFolder, string baseFileName)
        {
            IsFinished = false;
            ProgressValue = 0;

            await Task.Run(async () =>
            {
                var dataTable = ReadExcel(excelPath);
                int totalRows = dataTable.Rows.Count;

                // We will create one big text file containing all labels (Standard for ZPL batches)
                // OR you can save individual files. Let's do one big file for now.
                var fullBatchZpl = new System.Text.StringBuilder();

                for (int i = 0; i < totalRows; i++)
                {
                    var rowData = GetRowData(dataTable.Rows[i], mapping);
                    string labelZpl = _zplService.MergeData(zplTemplate, rowData);

                    fullBatchZpl.AppendLine(labelZpl);

                    UpdateProgress(i + 1, totalRows, "Generating");
                    await Task.Delay(10); // Tiny delay for UI
                }

                // Write to File
                string finalPath = Path.Combine(saveFolder, baseFileName + ".zpl");
                File.WriteAllText(finalPath, fullBatchZpl.ToString());

                FinalizeProcess($"Saved to {finalPath}");
            });
        }

        // --- HELPER METHODS ---

        private DataTable ReadExcel(string path)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });
                return result.Tables[0];
            }
        }

        private Dictionary<string, string> GetRowData(DataRow row, List<MappingItem> mapping)
        {
            var result = new Dictionary<string, string>();
            foreach (var map in mapping)
            {
                string value = "";

                if (map.IsConstantSelected)
                {
                    value = map.ConstantValue;
                }
                else if (map.SelectedSource != "<Empty>" && row.Table.Columns.Contains(map.SelectedSource))
                {
                    value = row[map.SelectedSource].ToString();
                }

                // Add to dictionary (e.g., "[Product]" -> "Wireless Mouse")
                result[map.ZplPlaceholder] = value;
            }
            return result;
        }

        private void UpdateProgress(int current, int total, string action)
        {
            ProgressValue = (double)current / total * 100;
            StatusMessage = $"{action}: {current} of {total} labels...";
        }

        private void FinalizeProcess(string message)
        {
            ProgressValue = 100;
            StatusMessage = message;
            IsFinished = true;
        }

        [RelayCommand]
        private void Finish()
        {
            NavigateFinishRequested?.Invoke();
        }
    }
}
