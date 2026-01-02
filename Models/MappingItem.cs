using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace zebra_label_editor.Models
{
    public partial class MappingItem : ObservableObject
    {
        // 1. The Left Side: The ZPL Placeholder (Static)
        [ObservableProperty]
        private string _zplPlaceholder;

        // 2. The Right Side: The list of options (Excel Headers + "Constant" + "Empty")
        [ObservableProperty]
        private List<string> _availableSources;

        // 3. What the user actually selected in the dropdown
        [ObservableProperty]
        private string _selectedSource;

        // 4. If they chose "Constant", what is the value?
        [ObservableProperty]
        private string _constantValue;

        // 5. Helper boolean to show/hide the "Constant" text box in the UI
        // We update this whenever SelectedSource changes
        public bool IsConstantSelected => SelectedSource == "<Constant Value>";

        // When the user changes the Dropdown, we check if we need to show the input box
        partial void OnSelectedSourceChanged(string value)
        {
            OnPropertyChanged(nameof(IsConstantSelected));
        }
    }
}
