using System;
using System.Windows;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Set up global exception handling
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                MessageBox.Show($"An unhandled exception occurred: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            
            // Note: We don't create the MainWindow here because Program.cs handles that
            // This OnStartup method is only called when the application is launched normally
            // without command-line arguments
        }
    }
}