using System.Configuration;
using System.Data;
using System.Windows;

using zebra_label_editor.Services.Implementations;
using zebra_label_editor.Services.Interfaces;
using zebra_label_editor.ViewModels;

namespace zebra_label_editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 1. Create Services
            IZplService zplService = new ZplService();
            IExcelService excelService = new ExcelService();

            // 2. Create Child ViewModels
            var importVm = new ImportViewModel(zplService);
            var mappingVm = new MappingViewModel(excelService);
            var selectionVm = new SelectionViewModel();
            var printConfigVm = new PrintConfigViewModel(); // Assumes empty constructor for now
            var saveConfigVm = new SaveConfigViewModel();   
            var processingVm = new ProcessingViewModel();

            // 3. Create MainViewModel (The Traffic Cop)
            var mainVm = new MainViewModel(
                importVm, 
                mappingVm, 
                selectionVm,
                printConfigVm,
                saveConfigVm,
                processingVm
                );

            // 4. Create and Show the Main Window
            MainWindow window = new MainWindow();
            window.DataContext = mainVm; // Give the Brain to the Window
            window.Show();
        }
    }
}
