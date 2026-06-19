using DevSpace.Database;
using DevSpace.Helpers;
using DevSpace.Views;
using System.Windows;

namespace DevSpace
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DatabaseManager.InitializeDatabase();
            ThemeManager.Initialize();

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
