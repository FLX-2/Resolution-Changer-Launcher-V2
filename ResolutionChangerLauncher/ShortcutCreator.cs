using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Creates Windows shortcuts (.lnk files) that launch games with custom resolution
    /// </summary>
    public class ShortcutCreator
    {
        #region COM Interop
        
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink
        {
        }
        
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
        
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000010b-0000-0000-C000-000000000046")]
        private interface IPersistFile
        {
            void GetClassID(out Guid pClassID);
            void IsDirty();
            void Load([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);
            void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
        }
        
        #endregion
        
        /// <summary>
        /// Creates a shortcut that will launch a game with the specified resolution
        /// </summary>
        /// <param name="targetPath">Path to the game executable</param>
        /// <param name="shortcutPath">Path where the shortcut will be saved</param>
        /// <param name="resolution">The resolution to use</param>
        /// <param name="revertResolution">Whether to revert the resolution when the game exits</param>
        public void CreateShortcut(string targetPath, string shortcutPath, Resolution resolution, bool revertResolution)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                throw new ArgumentException("Target path cannot be empty", nameof(targetPath));
            }
            
            if (string.IsNullOrWhiteSpace(shortcutPath))
            {
                throw new ArgumentException("Shortcut path cannot be empty", nameof(shortcutPath));
            }
            
            if (!File.Exists(targetPath))
            {
                throw new FileNotFoundException("Target file not found", targetPath);
            }
            
            // Get the path to our application
            string appPath = Assembly.GetEntryAssembly()?.Location ?? 
                throw new InvalidOperationException("Could not determine application path");
            
            // Create the shortcut
            var link = new ShellLink() as IShellLink;
            
            // Set the path to our application (not the game)
            link.SetPath(appPath);
            
            // Set the working directory to the directory of our application
            link.SetWorkingDirectory(Path.GetDirectoryName(appPath));
            
            // Set the arguments to launch the game with the specified resolution
            string args = $"--launch \"{targetPath}\" --width {resolution.Width} --height {resolution.Height}";
            if (revertResolution)
            {
                args += " --revert";
            }
            link.SetArguments(args);
            
            // Set the description
            link.SetDescription($"Launch {Path.GetFileNameWithoutExtension(targetPath)} at {resolution.Width}x{resolution.Height}");
            
            // Set the icon to the game's icon
            link.SetIconLocation(targetPath, 0);
            
            // Save the shortcut
            var file = link as IPersistFile;
            file.Save(shortcutPath, true);
        }
    }
}