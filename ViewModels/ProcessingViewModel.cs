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
            InitializeProcess();

            await Task.Run(async () =>
            {
                // 1. Determine if we should use Excel or just print 1 label
                // Rule: Must have an Excel path AND at least one column mapped
                bool useExcelData = !string.IsNullOrEmpty(excelPath) &&
                                    mapping.Any(m => !m.IsConstantSelected && m.SelectedSource != "Empty");

                try
                {
                    if (useExcelData)
                    {
                        // --- MULTIPLE LABELS (From Excel) ---
                        var dataTable = ReadExcel(excelPath);
                        int totalRows = dataTable.Rows.Count;

                        for (int i = 0; i < totalRows; i++)
                        {
                            var rowData = GetLabelData(mapping, dataTable.Rows[i]);
                            string labelZpl = _zplService.MergeData(zplTemplate, rowData);

                            PrintZpl(printerName, labelZpl);

                            UpdateProgress(i + 1, totalRows, "Printing");
                            await Task.Delay(50); // Small delay for spooler
                        }
                    }
                    else
                    {
                        // --- SINGLE LABEL (Constant/Empty only) ---
                        // We pass 'null' for the DataRow because we don't need one
                        var rowData = GetLabelData(mapping, null);
                        string labelZpl = _zplService.MergeData(zplTemplate, rowData);

                        PrintZpl(printerName, labelZpl);
                        UpdateProgress(1, 1, "Printing");
                    }

                    FinalizeProcess("Printing Completed!");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                }
            });
        }

        // --- SAVING LOGIC ---
        public async void StartSaving(
            string zplTemplate, 
            string excelPath, 
            List<MappingItem> mapping, 
            string saveFolder, 
            string baseFileName,
            string extension)
        {
            InitializeProcess();

            await Task.Run(async () =>
            {
                bool useExcelData = !string.IsNullOrEmpty(excelPath) &&
                                    mapping.Any(m => !m.IsConstantSelected && m.SelectedSource != "Empty");

                var fullBatchZpl = new System.Text.StringBuilder();

                try
                {
                    if (useExcelData)
                    {
                        // --- MULTIPLE LABELS ---
                        var dataTable = ReadExcel(excelPath);
                        int totalRows = dataTable.Rows.Count;

                        for (int i = 0; i < totalRows; i++)
                        {
                            var rowData = GetLabelData(mapping, dataTable.Rows[i]);
                            string labelZpl = _zplService.MergeData(zplTemplate, rowData);
                            fullBatchZpl.AppendLine(labelZpl);

                            UpdateProgress(i + 1, totalRows, "Generating");
                        }
                    }
                    else
                    {
                        // --- SINGLE LABEL ---
                        var rowData = GetLabelData(mapping, null);
                        string labelZpl = _zplService.MergeData(zplTemplate, rowData);
                        fullBatchZpl.AppendLine(labelZpl);

                        UpdateProgress(1, 1, "Generating");
                    }

                    // Write File
                    string finalPath = Path.Combine(saveFolder, baseFileName + extension);
                    File.WriteAllText(finalPath, fullBatchZpl.ToString());

                    FinalizeProcess($"Saved to {finalPath}");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                }
            });
        }

        // --- HELPER METHODS ---

        private void InitializeProcess()
        {
            IsFinished = false;
            ProgressValue = 0;
            StatusMessage = "Starting...";
        }


        private DataTable ReadExcel(string path)
        {
            // CRITICAL: This line prevents the "Value cannot be null" error
            // We only call this method if path is confirmed not null in the calling method
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

        // Helper to extract data. Row can be null!
        private Dictionary<string, string> GetLabelData(List<MappingItem> mapping, DataRow? row)
        {
            var result = new Dictionary<string, string>();

            foreach (var map in mapping)
            {
                string value = ""; // Default to empty

                if (map.IsConstantSelected)
                {
                    // Case 1: Constant Value (Always used)
                    value = map.ConstantValue ?? "";
                }
                else if (map.SelectedSource == "Empty")
                {
                    // Case 2: Explicitly Empty
                    value = "";
                }
                else if (row != null && row.Table.Columns.Contains(map.SelectedSource))
                {
                    // Case 3: Excel Column (Only if row exists)
                    value = row[map.SelectedSource].ToString() ?? "";
                }

                // If row is null but we mapped a column, value remains "" (Safe fallback)

                result[map.ZplPlaceholder] = value;
            }
            return result;
        }

        private void PrintZpl(string printerName, string zplData)
        {
            bool success = RawPrinterHelper.SendStringToPrinter(printerName, zplData);
            if (!success) throw new Exception("Printer error. Check connection.");
        }

        private void UpdateProgress(int current, int total, string action)
        {
            // Calculate percentage
            double percent = (double)current / total * 100;
            ProgressValue = percent;
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
