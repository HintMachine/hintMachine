using System.Windows;
using HintMachine.Models;

namespace HintMachine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Exit(object sender, ExitEventArgs e)
        {
            HintMachineService.OnAppExit();
        }
    }
}
