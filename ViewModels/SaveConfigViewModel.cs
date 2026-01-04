using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32; // For SaveFileDialog

namespace zebra_label_editor.ViewModels
{
    public partial class SaveConfigViewModel : ObservableObject
    {
        public Action? NavigateBackRequested;
        public Action? NavigateNextRequested;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNextEnabled))]
        private string _savePath;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNextEnabled))]
        private string _fileName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNextEnabled))]
        private string _selectedFormat;

        // Validation Logic: True only if everything is filled in
        public bool IsNextEnabled =>
            !string.IsNullOrWhiteSpace(FileName) &&
            !string.IsNullOrWhiteSpace(SavePath) &&
            !string.IsNullOrEmpty(SelectedFormat);

        public List<string> Formats { get; } = new List<string> { "ZPL Code (.zpl)", "Text File (.txt)", "Print File (.prn)" };

        public SaveConfigViewModel()
        {
            SelectedFormat = Formats[0];
            FileName = "Labels_Batch_01";
        }

        [RelayCommand]
        private void Browse()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "ZPL Files|*.zpl|Text Files|*.txt|Print Files|*.prn",
                FileName = FileName
            };

            if (dialog.ShowDialog() == true)
            {
                SavePath = System.IO.Path.GetDirectoryName(dialog.FileName);
                FileName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }

        [RelayCommand]
        private void GoBack() => NavigateBackRequested?.Invoke();

        [RelayCommand]
        private void Next()
        {
            // Simple validation
            if (string.IsNullOrEmpty(FileName)) return;

            NavigateNextRequested?.Invoke();
        }

        public string GetFileExtension()
        {
            if (string.IsNullOrEmpty(SelectedFormat)) return ".zpl";

            if (SelectedFormat.Contains(".txt"))
            {
                return ".txt";
            }
            else if (SelectedFormat.Contains(".prn"))
            {
                return ".prn";
            }
            else
            {
                return ".zpl"; // Default
            }             
        }
    }
}
