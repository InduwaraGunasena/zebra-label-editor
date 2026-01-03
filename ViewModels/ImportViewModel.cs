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

        // The File Path
        [ObservableProperty] // The UI will display this text. When we set it, the UI updates automatically.
        [NotifyPropertyChangedFor(nameof(IsNextEnabled))] // Update button when path changes       
        private string _filePath;

        // Error Message (Red Text)
        [ObservableProperty]
        private string _errorMessage;

        // Controls if the Next button is clickable
        public bool IsNextEnabled => !string.IsNullOrEmpty(FilePath) && string.IsNullOrEmpty(ErrorMessage);

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
            ErrorMessage = string.Empty; // Reset error

            // 1. Open the File Picker
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "ZPL Files (*.prn;*.txt;*.zpl)|*.prn;*.txt;*.zpl|All files (*.*)|*.*",
                Title = "Select Zebra Label Template"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedPath = openFileDialog.FileName;

                // Basic Validation: Check if file exists and is readable
                if (!File.Exists(selectedPath))
                {
                    ErrorMessage = "Error: File does not exist.";
                    FilePath = string.Empty;
                    OnPropertyChanged(nameof(IsNextEnabled));
                    return;
                }
                
                try
                {
                    // Read the content to validate it
                    string content = await File.ReadAllTextAsync(selectedPath);

                    // VALIDATION CHECK 
                    if (!_zplService.IsValidZpl(content))
                    {
                        // File exists, but content is garbage
                        ErrorMessage = "Error: Invalid ZPL file. Content must contain '^XA' command.";
                        FilePath = string.Empty; // Clear path so they can't click Next

                    }
                    else
                    {
                        // Check for editable placeholders
                        if (!_zplService.IsValidZplWithEditablePlaceholders(content))
                        {
                            ErrorMessage = "Error: ZPL file must contain at least one editable placeholder in the format [PLACEHOLDER].";
                            FilePath = string.Empty; // Clear path so they can't click Next
                            OnPropertyChanged(nameof(IsNextEnabled));
                            return;
                        }
                        else
                        {
                            // File is good!
                            FilePath = selectedPath;
                            ErrorMessage = string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error: Could not read file. {ex.Message}";
                    FilePath = string.Empty;
                }
                

                // Refresh the button state manually since ErrorMessage changed
                OnPropertyChanged(nameof(IsNextEnabled));
            }
        }

        // 1. EVENT: "Hey MainViewModel, I'm done! Here is the list of placeholders."
        public Action<List<string>>? NavigateToMappingRequested;

        [RelayCommand]
        private void Next()
        {
            // Validation: Don't move forward if no file is selected
            if (!IsNextEnabled) return;

            try
            {
                var placeholders = _zplService.ExtractPlaceholders(File.ReadAllText(FilePath));
                NavigateToMappingRequested?.Invoke(placeholders);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Critical Error parsing file: " + ex.Message;
                OnPropertyChanged(nameof(IsNextEnabled));
            }
        }
    }
}
