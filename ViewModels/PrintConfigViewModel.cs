using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Drawing.Printing; // Built-in .NET Namespace for printers

namespace zebra_label_editor.ViewModels
{
    public partial class PrintConfigViewModel : ObservableObject
    {
        // Navigation Events
        public Action? NavigateBackRequested;
        public Action? NavigateNextRequested;

        [ObservableProperty]
        private string _selectedPrinter;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextCommand))]
        private ObservableCollection<string> _availablePrinters;

        public PrintConfigViewModel()
        {
            // Load installed printers
            AvailablePrinters = new ObservableCollection<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                AvailablePrinters.Add(printer);
            }

            // Select default printer if available
            if (AvailablePrinters.Count > 0)
            {
                // Try to find the actual system default, or just pick the first one
                SelectedPrinter = new PrinterSettings().PrinterName;
                if (!AvailablePrinters.Contains(SelectedPrinter))
                    SelectedPrinter = AvailablePrinters.FirstOrDefault();
            }
        }

        [RelayCommand]
        private void GoBack() => NavigateBackRequested?.Invoke();

        [RelayCommand]
        private void Next()
        {
            if (string.IsNullOrEmpty(SelectedPrinter)) return;

            // Navigate to Processing Screen (We will implement the actual printing logic there)
            NavigateNextRequested?.Invoke();
        }
    }
}
