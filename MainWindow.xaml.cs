using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using zebra_label_editor.Services.Implementations;
using zebra_label_editor.ViewModels;

namespace zebra_label_editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 1. Create the Service
            var zplService = new ZplService();

            // 2. Create the ViewModel and give it the Service
            var viewModel = new ImportViewModel(zplService);

            // 3. Connect the View to the ViewModel
            MyImportView.DataContext = viewModel;
        }
    }
}