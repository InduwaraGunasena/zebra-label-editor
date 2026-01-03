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
        private string _savePath;

        [ObservableProperty]
        private string _fileName;

        [ObservableProperty]
        private string _selectedFormat;

        public List<string> Formats { get; } = new List<string> { "ZPL Code (.zpl)", "Text File (.txt)" };

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
                Filter = "ZPL Files|*.zpl|Text Files|*.txt",
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
    }
}
