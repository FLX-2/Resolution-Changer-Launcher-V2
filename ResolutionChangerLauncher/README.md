# Resolution Changer Game Launcher

A lightweight Windows application that allows you to launch games and applications at specific screen resolutions. The application automatically reverts to your original resolution when the game closes.

## Features

- Launch games and applications at custom screen resolutions
- Automatically revert to original resolution when the game closes
- Save game profiles for quick access
- Create desktop shortcuts that launch games at specific resolutions
- Support for UWP (Windows Store) applications

## Requirements

- Windows 10 or later
- .NET 9.0 Runtime

## Building the Application

1. Ensure you have the .NET 9.0 SDK installed
2. Open a command prompt in the project directory
3. Run the following command to build the application:

```
dotnet build
```

4. To create a standalone executable, run:

```
dotnet publish -c Release -r win-x64 --self-contained
```

## Usage

### Launching Games

1. Click the "Browse" button to select a game executable or shortcut
   - Or click the "UWP App" button to select a Windows Store application
2. Enter a name for the game (automatically filled if not provided)
3. Select a resolution from the dropdown or enter custom width and height
4. Check "Revert resolution when game closes" if you want the original resolution restored
5. Click "Launch Game" to start the game at the selected resolution

### Saving Game Profiles

1. Configure the game and resolution settings
2. Click "Save Profile" to save the configuration
3. Saved profiles appear in the list on the left side

### Creating Shortcuts

1. Configure the game and resolution settings
2. Click "Create Shortcut" to create a desktop shortcut
3. Choose a location to save the shortcut
4. The shortcut will launch the game at the specified resolution when double-clicked

## How It Works

The application uses Windows API calls to change the screen resolution before launching the game. It then monitors the game process and reverts to the original resolution when the game exits.

## Troubleshooting

- If the application fails to change the resolution, try running it as administrator
- Some games may override the system resolution settings
- For UWP apps, the resolution won't automatically revert when the app closes
  - A dialog will appear asking you to click OK when you're done using the app
- If you encounter issues with shortcuts, try entering the path directly in the text field

## License

This software is provided as-is with no warranty. Use at your own risk.