using Streaming_Muesic_WPF.Model;
using Streaming_Muesic_WPF.Utils;
using System.Windows;

namespace Streaming_Muesic_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();

            var inputModules = ReflectionHelper.CreateAllInstancesOf<IInputModule>();

            var vm = new VmMainWindow(inputModules);
            mainWindow.DataContext = vm;
            mainWindow.Closing += (s, args) => vm.SelectedInputModule.Deactivate();
            mainWindow.Show();
        }
    }
}
