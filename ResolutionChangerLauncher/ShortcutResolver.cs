using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Resolves Windows shortcut (.lnk) files to their target paths
    /// </summary>
    public class ShortcutResolver
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
        /// Resolves a shortcut file to its target path
        /// </summary>
        /// <param name="shortcutPath">Path to the .lnk file</param>
        /// <returns>The target path of the shortcut</returns>
        public string ResolveShortcutTarget(string shortcutPath)
        {
            // First try the COM method
            try
            {
                var link = new ShellLink() as IShellLink;
                var file = link as IPersistFile;
                
                file?.Load(shortcutPath, 0);
                
                var targetPath = new StringBuilder(260);
                link?.GetPath(targetPath, targetPath.Capacity, out _, 0);
                
                string result = targetPath.ToString();
                
                // If we got a valid result, return it
                if (!string.IsNullOrWhiteSpace(result) && File.Exists(result))
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"COM shortcut resolution failed: {ex.Message}");
                // Continue to alternative method
            }
            
            // If COM method failed, try alternative approach - just launch the shortcut directly
            try
            {
                // For UWP apps or problematic shortcuts, just return the shortcut path itself
                // The launcher will use ShellExecute which can handle shortcuts directly
                return shortcutPath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error resolving shortcut: {ex.Message}", ex);
            }
        }
    }
}