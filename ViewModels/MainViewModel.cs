using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace zebra_label_editor.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // The Child ViewModels
        private readonly ImportViewModel _importVm;
        private readonly MappingViewModel _mappingVm;
        private readonly SelectionViewModel _selectionVm;
        private readonly PrintConfigViewModel _printConfigVm; 
        private readonly SaveConfigViewModel _saveConfigVm;  
        private readonly ProcessingViewModel _processingVm;

        // This property determines what is shown on screen!
        [ObservableProperty]
        private object _currentViewModel;

        // Constructor: We ask for the child VMs (Dependency Injection)
        public MainViewModel(
            ImportViewModel importVm,
            MappingViewModel mappingVm,
            SelectionViewModel selectionVm,
            PrintConfigViewModel printVm,
            SaveConfigViewModel saveVm,
            ProcessingViewModel processingVm)
        {
            _importVm = importVm;
            _mappingVm = mappingVm;
            _selectionVm = selectionVm;
            _printConfigVm = printVm;
            _saveConfigVm = saveVm;
            _processingVm = processingVm;

            // "Next" Logic
            _importVm.NavigateToMappingRequested += OnNavigateToMapping;

            // "Back" Logic
            _mappingVm.NavigateBackRequested += () => CurrentViewModel = _importVm;
            _mappingVm.NavigateNextRequested += OnNavigateToSelection;

            _selectionVm.NavigateBackRequested += () => CurrentViewModel = _mappingVm;
            _selectionVm.NavigateNextRequested += OnSelectionMade; // The Branching Logic!

            // 1. Wire Print Config
            _printConfigVm.NavigateBackRequested += () => CurrentViewModel = _selectionVm;
            _printConfigVm.NavigateNextRequested += StartPrintProcess;

            // 2. Wire Save Config
            _saveConfigVm.NavigateBackRequested += () => CurrentViewModel = _selectionVm;
            _saveConfigVm.NavigateNextRequested += StartSaveProcess;

            // 3. Wire Processing (Finish Button)
            _processingVm.NavigateFinishRequested += () =>
            {
                // Reset everything and go back to start
                CurrentViewModel = _importVm;
            };

            // Start the application on the Import Screen
            CurrentViewModel = _importVm;
        }

        // This runs when ImportVM says "Next"
        private void OnNavigateToMapping(List<string> placeholders)
        {
            // Pass the data to the Mapping Screen
            _mappingVm.Initialize(placeholders);

            // Switch the view!
            CurrentViewModel = _mappingVm;
        }

        private void OnNavigateToSelection()
        {
            // We might need to pass data here later, but for now just switch
            CurrentViewModel = _selectionVm;
        }

        private void OnSelectionMade(bool isPrint)
        {
            if (isPrint)
            {
                // Go to Print Config
                CurrentViewModel = _printConfigVm;
            }
            else
            {
                // Go to Save Config
                CurrentViewModel = _saveConfigVm;
            }
        }

        private void StartPrintProcess()
        {
            // GATHER DATA
            string zplTemplate = System.IO.File.ReadAllText(_importVm.FilePath); // Get the raw ZPL
            string excelPath = _mappingVm.ExcelFilePath;
            var mapping = _mappingVm.MappingRows.ToList(); // The Map
            string printerName = _printConfigVm.SelectedPrinter;

            // SWITCH VIEW
            CurrentViewModel = _processingVm;

            // START WORK
            _processingVm.StartPrinting(zplTemplate, excelPath, mapping, printerName);
        }

        private void StartSaveProcess()
        {
            // GATHER DATA
            string zplTemplate = System.IO.File.ReadAllText(_importVm.FilePath);
            string excelPath = _mappingVm.ExcelFilePath;
            var mapping = _mappingVm.MappingRows.ToList();
            string savePath = _saveConfigVm.SavePath;
            string fileName = _saveConfigVm.FileName;

            // SWITCH VIEW
            CurrentViewModel = _processingVm;

            // START WORK
            _processingVm.StartSaving(zplTemplate, excelPath, mapping, savePath, fileName);
        }
    }
}
