using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace zebra_label_editor.ViewModels
{
    public partial class SelectionViewModel : ObservableObject
    {
        // Output Actions (Events)
        public Action? NavigateBackRequested;
        public Action<bool>? NavigateNextRequested; // Bool: True = Print, False = Save

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNextEnabled))] // Update button when selection changes
        private bool _isPrintSelected;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNextEnabled))]
        private bool _isSaveSelected;

        // Logic: The Next button is enabled only if ONE option is picked
        public bool IsNextEnabled => IsPrintSelected || IsSaveSelected;

        // Command for the "Print" Big Button
        [RelayCommand]
        private void SelectPrint()
        {
            IsPrintSelected = true;
            IsSaveSelected = false; // Mutually exclusive
        }

        // Command for the "Save" Big Button
        [RelayCommand]
        private void SelectSave()
        {
            IsPrintSelected = false;
            IsSaveSelected = true;
        }

        [RelayCommand]
        private void GoBack() => NavigateBackRequested?.Invoke();

        [RelayCommand]
        private void Next()
        {
            // Tell MainViewModel which path we chose (True = Print)
            NavigateNextRequested?.Invoke(IsPrintSelected);
        }
    }
}
