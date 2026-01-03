using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32; // Needed for the File Dialog
using System.IO;
using System.Windows;
using zebra_label_editor.Services.Interfaces;

namespace zebra_label_editor.ViewModels
{
    // "ObservableObject" is magic from the Toolkit. It automatically notifies the UI when data changes.
    public partial class ImportViewModel : ObservableObject
    {
        private readonly IZplService _zplService;

        // The UI will display this text. When we set it, the UI updates automatically.
        [ObservableProperty]
        private string _filePath;

        [ObservableProperty]
        private string _previewContent;

        // Constructor: We ask for the ZplService here (Dependency Injection)
        public ImportViewModel(IZplService zplService)
        {
            _zplService = zplService;
        }

        // The Design Time constructor (Optional: helps see data in Visual Studio Designer)
        public ImportViewModel() { }

        // This attribute [RelayCommand] automatically creates a "BrowseCommand" we can bind to a Button
        [RelayCommand]
        private async Task Browse()
        {
            // 1. Open the File Picker
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "ZPL Files (*.prn;*.txt;*.zpl)|*.prn;*.txt;*.zpl|All files (*.*)|*.*",
                Title = "Select Zebra Label Template"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // 2. Save the path so the UI shows it
                FilePath = openFileDialog.FileName;

                // 3. Use our Service to read the file
                try
                {
                    string content = _zplService.LoadZplTemplate(FilePath);

                    // Show the first 500 characters as a preview
                    PreviewContent = content.Length > 500 ? content.Substring(0, 500) + "..." : content;

                    // (Later we will extract placeholders here)
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}");
                }
            }
        }

        // 1. EVENT: "Hey MainViewModel, I'm done! Here is the list of placeholders."
        public Action<List<string>>? NavigateToMappingRequested;

        [RelayCommand]
        private void Next()
        {
            // Validation: Don't move forward if no file is selected
            if (string.IsNullOrEmpty(FilePath))
            {
                MessageBox.Show("Please select a file first.");
                return;
            }

            // 2. Extract Placeholders using the service
            var placeholders = _zplService.ExtractPlaceholders(File.ReadAllText(FilePath));

            // 3. Trigger the event to move to the next screen
            NavigateToMappingRequested?.Invoke(placeholders);
        }
    }
}
