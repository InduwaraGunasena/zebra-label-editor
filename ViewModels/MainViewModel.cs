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
        private readonly PrintConfigViewModel _printConfigVm; // You need to create this class!
        private readonly SaveConfigViewModel _saveConfigVm;   // You need to create this class!

        // This property determines what is shown on screen!
        [ObservableProperty]
        private object _currentViewModel;

        // Constructor: We ask for the child VMs (Dependency Injection)
        public MainViewModel(
            ImportViewModel importVm,
            MappingViewModel mappingVm,
            SelectionViewModel selectionVm,
            PrintConfigViewModel printVm,
            SaveConfigViewModel saveVm)
        {
            _importVm = importVm;
            _mappingVm = mappingVm;
            _selectionVm = selectionVm;
            _printConfigVm = printVm;
            _saveConfigVm = saveVm;

            // "Next" Logic
            _importVm.NavigateToMappingRequested += OnNavigateToMapping;

            // "Back" Logic
            _mappingVm.NavigateBackRequested += () => CurrentViewModel = _importVm;
            _mappingVm.NavigateNextRequested += OnNavigateToSelection;

            _selectionVm.NavigateBackRequested += () => CurrentViewModel = _mappingVm;
            _selectionVm.NavigateNextRequested += OnSelectionMade; // The Branching Logic!

            // 2. Start with the Import Screen
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
    }
}
