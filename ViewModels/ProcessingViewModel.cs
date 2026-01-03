using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zebra_label_editor.ViewModels
{
    public partial class ProcessingViewModel : ObservableObject
    {
        public Action? NavigateFinishRequested;

        [ObservableProperty]
        private string _statusMessage = "Ready to start...";

        [ObservableProperty]
        private double _progressValue = 0;

        [ObservableProperty]
        private bool _isFinished = false;

        // This method will be called by MainViewModel to start the work
        public async Task StartProcessing(bool isPrint, string configData)
        {
            StatusMessage = "Initializing...";
            ProgressValue = 0;
            IsFinished = false;

            // Simulate work for now (We will add real logic later)
            for (int i = 0; i <= 100; i += 10)
            {
                await Task.Delay(200); // Fake delay
                ProgressValue = i;
                StatusMessage = $"Processing label {i / 10} of 10...";
            }

            StatusMessage = "Completed Successfully!";
            IsFinished = true;
        }

        [RelayCommand]
        private void Finish()
        {
            NavigateFinishRequested?.Invoke();
        }
    }
}
