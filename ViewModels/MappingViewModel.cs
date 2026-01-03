using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using zebra_label_editor.Models;
using zebra_label_editor.Services.Interfaces;

namespace zebra_label_editor.ViewModels
{
    public partial class MappingViewModel : ObservableObject
    {
        private readonly IExcelService _excelService;

        // The list of rows for the UI Grid
        public ObservableCollection<MappingItem> MappingRows { get; } = new ObservableCollection<MappingItem>();

        [ObservableProperty]
        private string _excelFilePath;

        public MappingViewModel(IExcelService excelService)
        {
            _excelService = excelService;
        }

        // We will call this method when navigating to this screen
        public void Initialize(List<string> zplPlaceholders)
        {
            MappingRows.Clear();

            // Initially, we don't have Excel headers yet, so the dropdown is empty 
            // or just has the special options
            var initialOptions = new List<string> { "<Empty>", "<Constant Value>" };

            foreach (var placeholder in zplPlaceholders)
            {
                MappingRows.Add(new MappingItem
                {
                    ZplPlaceholder = placeholder,
                    AvailableSources = initialOptions,
                    SelectedSource = "<Empty>" // Default
                });
            }
        }

        [RelayCommand]
        private void LoadExcel()
        {
            var dialog = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls|CSV Files|*.csv" };

            if (dialog.ShowDialog() == true)
            {
                ExcelFilePath = dialog.FileName;

                try
                {
                    // 1. Get Headers from Excel
                    var headers = _excelService.GetColumnHeaders(ExcelFilePath);

                    // 2. Create the full dropdown list (Headers + Special Options)
                    var fullOptions = new List<string>();
                    fullOptions.AddRange(headers);       // Add Excel Columns
                    fullOptions.Add("<Constant Value>"); // Add Special Option
                    fullOptions.Add("<Empty>");          // Add Special Option

                    // 3. Update every row in the table with these new options
                    foreach (var row in MappingRows)
                    {
                        row.AvailableSources = fullOptions;

                        // Optional: Auto-Match! 
                        // If Excel has a column "Price" and ZPL has "[Price]", auto-select it.
                        string cleanName = row.ZplPlaceholder.Replace("[", "").Replace("]", "");
                        if (headers.Contains(cleanName))
                        {
                            row.SelectedSource = cleanName;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error reading Excel: {ex.Message}");
                }
            }
        }

        // 1. EVENT: "Hey MainViewModel, I want to go back!"
        public Action? NavigateBackRequested;


        // 2. COMMAND: Bound to the UI Button
        [RelayCommand]
        private void GoBack()
        {
            // Fire the event!
            NavigateBackRequested?.Invoke();
        }

        public Action? NavigateNextRequested;

        [RelayCommand]
        private void Next()
        {
            // Optional: Add validation here (e.g., check if all rows are mapped)

            // Trigger the event to tell MainViewModel to move on
            NavigateNextRequested?.Invoke();
        }
    }
}
