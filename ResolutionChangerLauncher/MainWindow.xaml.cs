using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<GameProfile> _gameProfiles;
        private ResolutionManager _resolutionManager;
        private GameLauncher _gameLauncher;
        private ShortcutCreator _shortcutCreator;
        private string _profilesFilePath;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize managers
            _resolutionManager = new ResolutionManager();
            _gameLauncher = new GameLauncher();
            _shortcutCreator = new ShortcutCreator();
            
            // Set up profiles storage
            _profilesFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ResolutionChangerLauncher",
                "profiles.json");
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_profilesFilePath));
            
            // Load profiles
            LoadProfiles();
            
            // Populate resolution dropdown with common resolutions
            PopulateResolutionComboBox();
            
            // Set up event handlers for game process monitoring
            _gameLauncher.GameExited += OnGameExited;
            
            // Disable buttons until a game is selected
            UpdateButtonStates();
        }

        private void PopulateResolutionComboBox()
        {
            // Add common resolutions
            var resolutions = new List<Resolution>
            {
                new Resolution(1280, 720),   // 720p
                new Resolution(1366, 768),   // Common laptop resolution
                new Resolution(1600, 900),   // 900p
                new Resolution(1920, 1080),  // 1080p
                new Resolution(2560, 1440),  // 1440p
                new Resolution(3440, 1440),  // Ultrawide 1440p
                new Resolution(3840, 2160)   // 4K
            };
            
            // Add current resolution
            var currentResolution = _resolutionManager.GetCurrentResolution();
            if (!resolutions.Any(r => r.Width == currentResolution.Width && r.Height == currentResolution.Height))
            {
                resolutions.Insert(0, currentResolution);
            }
            
            ResolutionComboBox.ItemsSource = resolutions;
            ResolutionComboBox.DisplayMemberPath = "DisplayName";
            ResolutionComboBox.SelectedIndex = 0;
        }

        private void LoadProfiles()
        {
            _gameProfiles = new ObservableCollection<GameProfile>();
            
            if (File.Exists(_profilesFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_profilesFilePath);
                    var profiles = JsonSerializer.Deserialize<List<GameProfile>>(json);
                    if (profiles != null)
                    {
                        foreach (var profile in profiles)
                        {
                            _gameProfiles.Add(profile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading profiles: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
            ProfilesListBox.ItemsSource = _gameProfiles;
            ProfilesListBox.DisplayMemberPath = "Name";
        }

        private void SaveProfiles()
        {
            try
            {
                string json = JsonSerializer.Serialize(_gameProfiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_profilesFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving profiles: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButtonStates()
        {
            bool hasGamePath = !string.IsNullOrWhiteSpace(GamePathTextBox.Text);
            bool hasGameName = !string.IsNullOrWhiteSpace(GameNameTextBox.Text);
            
            LaunchButton.IsEnabled = hasGamePath;
            CreateShortcutButton.IsEnabled = hasGamePath;
            SaveProfileButton.IsEnabled = hasGamePath && hasGameName;
            DeleteProfileButton.IsEnabled = ProfilesListBox.SelectedItem != null;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Applications (*.exe;*.lnk)|*.exe;*.lnk|All files (*.*)|*.*",
                Title = "Select a game or application"
            };
            
            if (dialog.ShowDialog() == true)
            {
                GamePathTextBox.Text = dialog.FileName;
                
                // If game name is empty, use the filename without extension
                if (string.IsNullOrWhiteSpace(GameNameTextBox.Text))
                {
                    GameNameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
                
                // Check if this is a shortcut file and show a warning
                if (Path.GetExtension(dialog.FileName).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "You've selected a shortcut (.lnk) file. The application will try to launch it directly.\n\n" +
                        "If you experience issues, try selecting the actual executable file instead.",
                        "Shortcut Selected",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                
                UpdateButtonStates();
            }
        }
        
        private void UwpButton_Click(object sender, RoutedEventArgs e)
        {
            // Show a dialog to help users enter UWP app URIs
            var dialog = new Window
            {
                Title = "Enter UWP App URI",
                Width = 500,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };
            
            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Instructions
            var instructionsText = new TextBlock
            {
                Text = "Enter the URI for the UWP app you want to launch:",
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(instructionsText, 0);
            
            // URI input
            var uriTextBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(5)
            };
            Grid.SetRow(uriTextBox, 1);
            
            // Common UWP apps list
            var commonAppsText = new TextBlock
            {
                Text = "Common UWP App URIs:",
                Margin = new Thickness(0, 10, 0, 5),
                FontWeight = FontWeights.Bold
            };
            Grid.SetRow(commonAppsText, 2);
            
            var commonAppsListBox = new ListBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                Height = 100
            };
            Grid.SetRow(commonAppsListBox, 3);
            
            // Add common UWP apps
            var commonApps = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Microsoft Store", "ms-windows-store:"),
                new KeyValuePair<string, string>("Xbox Game Bar", "xbox-gamebar:"),
                new KeyValuePair<string, string>("Settings", "ms-settings:"),
                new KeyValuePair<string, string>("Calculator", "calculator:"),
                new KeyValuePair<string, string>("Photos", "ms-photos:"),
                new KeyValuePair<string, string>("Mail", "mailto:"),
                new KeyValuePair<string, string>("Maps", "bingmaps:"),
                new KeyValuePair<string, string>("Xbox", "xbox:")
            };
            
            foreach (var app in commonApps)
            {
                commonAppsListBox.Items.Add(app);
            }
            commonAppsListBox.DisplayMemberPath = "Key";
            
            // When an item is selected, populate the URI textbox
            commonAppsListBox.SelectionChanged += (s, args) =>
            {
                if (commonAppsListBox.SelectedItem is KeyValuePair<string, string> selectedApp)
                {
                    uriTextBox.Text = selectedApp.Value;
                }
            };
            
            // Help text
            var helpText = new TextBlock
            {
                Text = "Note: UWP apps don't have direct .exe files. Instead, they use URI protocols to launch. " +
                      "Select from the list above or enter a custom URI.",
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(helpText, 4);
            
            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(buttonPanel, 5);
            
            var okButton = new Button
            {
                Content = "OK",
                Padding = new Thickness(20, 5, 20, 5),
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            
            var cancelButton = new Button
            {
                Content = "Cancel",
                Padding = new Thickness(20, 5, 20, 5),
                IsCancel = true
            };
            
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            
            // Add all controls to the grid
            grid.Children.Add(instructionsText);
            grid.Children.Add(uriTextBox);
            grid.Children.Add(commonAppsText);
            grid.Children.Add(commonAppsListBox);
            grid.Children.Add(helpText);
            grid.Children.Add(buttonPanel);
            
            dialog.Content = grid;
            
            // Handle OK button click
            okButton.Click += (s, args) =>
            {
                if (!string.IsNullOrWhiteSpace(uriTextBox.Text))
                {
                    GamePathTextBox.Text = uriTextBox.Text;
                    
                    // Set a default name if empty
                    if (string.IsNullOrWhiteSpace(GameNameTextBox.Text))
                    {
                        // Try to get a name from the selected item
                        if (commonAppsListBox.SelectedItem is KeyValuePair<string, string> selectedApp)
                        {
                            GameNameTextBox.Text = selectedApp.Key;
                        }
                        else
                        {
                            // Use the URI protocol as the name
                            string protocol = uriTextBox.Text.Split(':')[0];
                            GameNameTextBox.Text = $"{char.ToUpper(protocol[0])}{protocol.Substring(1)} App";
                        }
                    }
                    
                    dialog.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please enter a URI for the UWP app.", "Missing URI",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            
            dialog.ShowDialog();
            
            UpdateButtonStates();
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GameNameTextBox.Text) || string.IsNullOrWhiteSpace(GamePathTextBox.Text))
            {
                MessageBox.Show("Please enter a game name and select a game path.", "Missing Information", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Check if width and height are valid
            if (!int.TryParse(WidthTextBox.Text, out int width) || !int.TryParse(HeightTextBox.Text, out int height))
            {
                MessageBox.Show("Please enter valid resolution values.", "Invalid Resolution", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Create or update profile
            var existingProfile = _gameProfiles.FirstOrDefault(p => p.Name == GameNameTextBox.Text);
            if (existingProfile != null)
            {
                // Update existing profile
                existingProfile.Path = GamePathTextBox.Text;
                existingProfile.Resolution = new Resolution(width, height);
                existingProfile.RevertResolution = RevertResolutionCheckBox.IsChecked ?? true;
                
                // Refresh the list
                int index = _gameProfiles.IndexOf(existingProfile);
                _gameProfiles.Remove(existingProfile);
                _gameProfiles.Insert(index, existingProfile);
            }
            else
            {
                // Create new profile
                var newProfile = new GameProfile
                {
                    Name = GameNameTextBox.Text,
                    Path = GamePathTextBox.Text,
                    Resolution = new Resolution(width, height),
                    RevertResolution = RevertResolutionCheckBox.IsChecked ?? true
                };
                
                _gameProfiles.Add(newProfile);
            }
            
            SaveProfiles();
            StatusTextBlock.Text = $"Profile '{GameNameTextBox.Text}' saved.";
        }

        private void CreateShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GamePathTextBox.Text))
            {
                MessageBox.Show("Please select a game first.", "No Game Selected", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Check if width and height are valid
            if (!int.TryParse(WidthTextBox.Text, out int width) || !int.TryParse(HeightTextBox.Text, out int height))
            {
                MessageBox.Show("Please enter valid resolution values.", "Invalid Resolution", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var saveDialog = new SaveFileDialog
            {
                Filter = "Shortcut (*.lnk)|*.lnk",
                Title = "Save Shortcut",
                FileName = $"{GameNameTextBox.Text} ({width}x{height}).lnk"
            };
            
            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var resolution = new Resolution(width, height);
                    bool revertResolution = RevertResolutionCheckBox.IsChecked ?? true;
                    
                    _shortcutCreator.CreateShortcut(
                        GamePathTextBox.Text, 
                        saveDialog.FileName, 
                        resolution,
                        revertResolution);
                    
                    StatusTextBlock.Text = "Shortcut created successfully.";
                    MessageBox.Show("Shortcut created successfully.", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating shortcut: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GamePathTextBox.Text))
            {
                MessageBox.Show("Please select a game first.", "No Game Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Check if width and height are valid
            if (!int.TryParse(WidthTextBox.Text, out int width) || !int.TryParse(HeightTextBox.Text, out int height))
            {
                MessageBox.Show("Please enter valid resolution values.", "Invalid Resolution",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Store original resolution before doing anything else
            _resolutionManager.StoreCurrentResolution();
            bool resolutionChanged = false;
            
            try
            {
                var resolution = new Resolution(width, height);
                bool revertResolution = RevertResolutionCheckBox.IsChecked ?? true;
                
                // Change resolution
                _resolutionManager.ChangeResolution(resolution);
                resolutionChanged = true;
                
                // Check if this is a UWP app (URI format or shell: format)
                bool isUwpApp = GamePathTextBox.Text.Contains(":") &&
                               !Path.IsPathRooted(GamePathTextBox.Text);
                
                // Check if this is a shortcut (.lnk file)
                bool isShortcut = !isUwpApp &&
                                 Path.GetExtension(GamePathTextBox.Text).Equals(".lnk", StringComparison.OrdinalIgnoreCase);
                
                if (isUwpApp)
                {
                    // Launch UWP app using URI protocol
                    LaunchUwpApp(GamePathTextBox.Text, revertResolution);
                }
                else
                {
                    // Launch regular game or app (including shortcuts)
                    _gameLauncher.LaunchGame(GamePathTextBox.Text, revertResolution);
                }
                
                StatusTextBlock.Text = $"Launched {GameNameTextBox.Text} at {resolution.DisplayName}.";
            }
            catch (Exception ex)
            {
                // If there was an error, revert the resolution
                if (resolutionChanged)
                {
                    try
                    {
                        _resolutionManager.RevertResolution();
                    }
                    catch (Exception revertEx)
                    {
                        // Log the revert error but don't show another message box
                        Debug.WriteLine($"Error reverting resolution: {revertEx.Message}");
                    }
                }
                
                MessageBox.Show($"Error launching application: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LaunchUwpApp(string uri, bool revertResolution)
        {
            bool launchSuccessful = false;
            
            try
            {
                // First try direct launch
                var startInfo = new ProcessStartInfo
                {
                    FileName = uri,
                    UseShellExecute = true
                };
                
                try
                {
                    // For UWP apps, Process.Start might return null even if the launch is successful
                    // This is because the UWP app is launched by the Windows shell, not directly by our process
                    Process.Start(startInfo);
                    launchSuccessful = true;  // Assume success unless an exception is thrown
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Direct UWP launch failed: {ex.Message}");
                    
                    // If direct launch fails, try launching through explorer.exe
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = uri,
                        UseShellExecute = true
                    };
                    
                    try
                    {
                        Process.Start(startInfo);
                        launchSuccessful = true;  // Assume success unless an exception is thrown
                    }
                    catch (Exception ex2)
                    {
                        Debug.WriteLine($"Explorer UWP launch failed: {ex2.Message}");
                        launchSuccessful = false;
                    }
                }
                
                if (!launchSuccessful)
                {
                    throw new InvalidOperationException("Failed to start UWP application");
                }
                
                // For UWP apps, we need to monitor a different way since Process.WaitForExit won't work
                if (revertResolution)
                {
                    // Start a background task to check if the app is still running
                    Task.Run(() =>
                    {
                        try
                        {
                            // Wait a bit to let the app start
                            System.Threading.Thread.Sleep(5000);
                            
                            // Show a message to the user about how to revert resolution
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(
                                    "The application has been launched at the selected resolution.\n\n" +
                                    "Since this is a UWP app, the resolution won't automatically revert when closed.\n\n" +
                                    "Please click OK when you're done using the app to revert the resolution.",
                                    "UWP App Launched",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                                
                                // Revert resolution when user clicks OK
                                _resolutionManager.RevertResolution();
                                StatusTextBlock.Text = "Resolution reverted.";
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in UWP monitoring: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error launching UWP app: {ex.Message}", ex);
            }
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = ProfilesListBox.SelectedItem as GameProfile;
            if (selectedProfile != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the profile '{selectedProfile.Name}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _gameProfiles.Remove(selectedProfile);
                    SaveProfiles();
                    StatusTextBlock.Text = $"Profile '{selectedProfile.Name}' deleted.";
                    UpdateButtonStates();
                }
            }
        }

        private void ProfilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProfile = ProfilesListBox.SelectedItem as GameProfile;
            if (selectedProfile != null)
            {
                GameNameTextBox.Text = selectedProfile.Name;
                GamePathTextBox.Text = selectedProfile.Path;
                WidthTextBox.Text = selectedProfile.Resolution.Width.ToString();
                HeightTextBox.Text = selectedProfile.Resolution.Height.ToString();
                RevertResolutionCheckBox.IsChecked = selectedProfile.RevertResolution;
                
                // Find and select the matching resolution in the combo box
                var matchingResolution = ResolutionComboBox.Items.Cast<Resolution>()
                    .FirstOrDefault(r => r.Width == selectedProfile.Resolution.Width && 
                                        r.Height == selectedProfile.Resolution.Height);
                
                if (matchingResolution != null)
                {
                    ResolutionComboBox.SelectedItem = matchingResolution;
                }
                else
                {
                    // If no match, select custom
                    ResolutionComboBox.SelectedIndex = -1;
                }
            }
            
            UpdateButtonStates();
        }

        private void ResolutionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedResolution = ResolutionComboBox.SelectedItem as Resolution;
            if (selectedResolution != null)
            {
                WidthTextBox.Text = selectedResolution.Width.ToString();
                HeightTextBox.Text = selectedResolution.Height.ToString();
            }
        }

        private void ResolutionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // When manually editing resolution, deselect the combo box
            if (int.TryParse(WidthTextBox.Text, out int width) && 
                int.TryParse(HeightTextBox.Text, out int height))
            {
                var matchingResolution = ResolutionComboBox.Items.Cast<Resolution>()
                    .FirstOrDefault(r => r.Width == width && r.Height == height);
                
                if (matchingResolution != null)
                {
                    ResolutionComboBox.SelectedItem = matchingResolution;
                }
                else
                {
                    ResolutionComboBox.SelectedIndex = -1;
                }
            }
        }

        private void OnGameExited(object sender, GameExitedEventArgs e)
        {
            // This is called from a different thread, so we need to use Dispatcher
            Dispatcher.Invoke(() =>
            {
                if (e.RevertResolution)
                {
                    try
                    {
                        _resolutionManager.RevertResolution();
                        StatusTextBlock.Text = "Game exited. Resolution reverted.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reverting resolution: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    StatusTextBlock.Text = "Game exited.";
                }
            });
        }
    }
}