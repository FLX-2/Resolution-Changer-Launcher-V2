using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Main entry point for the application
    /// </summary>
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Create a log file for debugging
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "ResolutionChangerLog.txt");
                
                File.AppendAllText(logPath, $"[{DateTime.Now}] Application starting...\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Args: {string.Join(", ", args)}\n");
                
                // Parse command line arguments
                if (args.Length > 0 && args[0] == "--launch")
                {
                    // This is a shortcut launch
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Handling shortcut launch\n");
                    HandleShortcutLaunch(args);
                }
                else
                {
                    // Normal application launch
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Starting normal application\n");
                    var application = new App();
                    application.InitializeComponent();
                    
                    // Create and show the main window manually
                    var mainWindow = new MainWindow();
                    application.MainWindow = mainWindow;
                    mainWindow.Show();
                    
                    application.Run();
                }
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "ResolutionChangerError.txt");
                
                File.WriteAllText(logPath, $"[{DateTime.Now}] Fatal error:\n{ex.Message}\n\n{ex.StackTrace}");
                
                // Show error message
                MessageBox.Show($"A fatal error occurred: {ex.Message}\n\nCheck {logPath} for details.",
                    "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Handles launching a game from a shortcut with resolution change
        /// </summary>
        private static void HandleShortcutLaunch(string[] args)
        {
            try
            {
                // Parse arguments
                string? gamePath = null;
                int width = 0;
                int height = 0;
                bool revertResolution = false;
                
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--launch":
                            if (i + 1 < args.Length)
                            {
                                gamePath = args[i + 1];
                                i++; // Skip the next argument
                            }
                            break;
                            
                        case "--width":
                            if (i + 1 < args.Length && int.TryParse(args[i + 1], out int w))
                            {
                                width = w;
                                i++; // Skip the next argument
                            }
                            break;
                            
                        case "--height":
                            if (i + 1 < args.Length && int.TryParse(args[i + 1], out int h))
                            {
                                height = h;
                                i++; // Skip the next argument
                            }
                            break;
                            
                        case "--revert":
                            revertResolution = true;
                            break;
                    }
                }
                
                // Validate arguments
                if (string.IsNullOrWhiteSpace(gamePath))
                {
                    ShowError("No game path specified.");
                    return;
                }
                
                if (!File.Exists(gamePath))
                {
                    ShowError($"Game file not found: {gamePath}");
                    return;
                }
                
                if (width <= 0 || height <= 0)
                {
                    ShowError("Invalid resolution specified.");
                    return;
                }
                
                // Change resolution and launch game
                var resolutionManager = new ResolutionManager();
                var gameLauncher = new GameLauncher();
                
                // Store current resolution if we need to revert
                if (revertResolution)
                {
                    resolutionManager.StoreCurrentResolution();
                }
                
                // Change resolution
                var resolution = new Resolution(width, height);
                resolutionManager.ChangeResolution(resolution);
                
                // Set up game exited event handler
                gameLauncher.GameExited += (sender, e) =>
                {
                    if (e.RevertResolution)
                    {
                        try
                        {
                            resolutionManager.RevertResolution();
                        }
                        catch (Exception ex)
                        {
                            ShowError($"Error reverting resolution: {ex.Message}");
                        }
                    }
                };
                
                // Launch game
                gameLauncher.LaunchGame(gamePath, revertResolution);
            }
            catch (Exception ex)
            {
                ShowError($"Error launching game: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Shows an error message box
        /// </summary>
        private static void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}