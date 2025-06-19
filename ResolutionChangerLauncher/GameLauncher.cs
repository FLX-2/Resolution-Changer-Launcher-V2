using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Handles launching games and monitoring their processes
    /// </summary>
    public class GameLauncher
    {
        private Process? _gameProcess;
        
        /// <summary>
        /// Event that fires when the game process exits
        /// </summary>
        public event EventHandler<GameExitedEventArgs>? GameExited;
        
        /// <summary>
        /// Launches a game and optionally monitors it for exit
        /// </summary>
        /// <param name="gamePath">Path to the game executable or shortcut</param>
        /// <param name="monitorForExit">Whether to monitor the process for exit</param>
        public void LaunchGame(string gamePath, bool monitorForExit)
        {
            if (string.IsNullOrWhiteSpace(gamePath))
            {
                throw new ArgumentException("Game path cannot be empty", nameof(gamePath));
            }
            
            // Check if this is a URI protocol (UWP app)
            bool isUwpUri = gamePath.Contains(":") && !Path.IsPathRooted(gamePath);
            
            // Only check if file exists for non-URI paths
            if (!isUwpUri && !File.Exists(gamePath))
            {
                throw new FileNotFoundException("Game file not found", gamePath);
            }
            
            try
            {
                // Check if this is a shortcut (.lnk file)
                bool isShortcut = !isUwpUri && Path.GetExtension(gamePath).Equals(".lnk", StringComparison.OrdinalIgnoreCase);
                
                // For shortcuts, we'll use explorer.exe to launch them directly
                // This is more reliable than trying to resolve the target
                if (isShortcut)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{gamePath}\"",
                        UseShellExecute = true
                    };
                    
                    _gameProcess = Process.Start(startInfo);
                    
                    if (_gameProcess == null)
                    {
                        throw new InvalidOperationException("Failed to start shortcut through explorer");
                    }
                    
                    if (monitorForExit)
                    {
                        // For shortcuts launched through explorer, we need to monitor differently
                        // We'll wait a bit and then show a dialog to the user
                        Task.Run(() =>
                        {
                            try
                            {
                                // Wait a bit to let the app start
                                System.Threading.Thread.Sleep(5000);
                                
                                // Raise the GameExited event to revert resolution
                                GameExited?.Invoke(this, new GameExitedEventArgs(_gameProcess.Id, monitorForExit));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error in shortcut monitoring: {ex.Message}");
                                // Still try to revert resolution
                                GameExited?.Invoke(this, new GameExitedEventArgs(-1, monitorForExit));
                            }
                        });
                    }
                    
                    return; // Exit early, we've handled the shortcut
                }
                
                // For regular executables
                var executableStartInfo = new ProcessStartInfo
                {
                    FileName = gamePath,
                    UseShellExecute = true
                };
                
                try
                {
                    _gameProcess = Process.Start(executableStartInfo);
                }
                catch (Exception)
                {
                    // If direct launch fails, try launching through explorer.exe
                    executableStartInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{gamePath}\"",
                        UseShellExecute = true
                    };
                    
                    _gameProcess = Process.Start(executableStartInfo);
                }
                
                if (_gameProcess == null)
                {
                    throw new InvalidOperationException("Failed to start game process");
                }
                
                if (monitorForExit && !isUwpUri)
                {
                    // Only monitor non-UWP apps
                    // UWP apps are handled differently in the MainWindow class
                    int processId = _gameProcess.Id;
                    Task.Run(() => MonitorGameProcess(processId, monitorForExit));
                }
            }
            catch (Exception ex)
            {
                // Add more context to the exception
                throw new InvalidOperationException($"Error launching game: {ex.Message}", ex);
            }
        }
        
        private void MonitorGameProcess(int processId, bool revertResolution)
        {
            try
            {
                // Try to get the process by ID
                using (var process = Process.GetProcessById(processId))
                {
                    // Wait for the process to exit
                    process.WaitForExit();
                    
                    // Raise the GameExited event
                    GameExited?.Invoke(this, new GameExitedEventArgs(processId, revertResolution));
                }
            }
            catch (ArgumentException)
            {
                // Process already exited or doesn't exist
                GameExited?.Invoke(this, new GameExitedEventArgs(processId, revertResolution));
            }
            catch (Exception ex)
            {
                // Log the error but still try to revert resolution
                Debug.WriteLine($"Error monitoring game process: {ex.Message}");
                GameExited?.Invoke(this, new GameExitedEventArgs(processId, revertResolution));
            }
        }
    }
}