using System;
using System.Runtime.InteropServices;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Manages screen resolution changes using Windows API
    /// </summary>
    public class ResolutionManager
    {
        #region Windows API Declarations
        
        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(
            string deviceName, int modeNum, ref DEVMODE devMode);
            
        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(
            ref DEVMODE devMode, int flags);
            
        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
        
        // Constants for ChangeDisplaySettings
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const int DISP_CHANGE_BADMODE = -2;
        private const int DISP_CHANGE_FAILED = -1;
        private const int DISP_CHANGE_RESTART = 1;
        private const int DISP_CHANGE_BADDUALVIEW = -6;
        
        // Display settings flags
        private const int DM_PELSWIDTH = 0x80000;
        private const int DM_PELSHEIGHT = 0x100000;
        private const int DM_BITSPERPEL = 0x40000;
        private const int DM_DISPLAYFREQUENCY = 0x400000;
        
        // CDS flags
        private const int CDS_UPDATEREGISTRY = 0x01;
        private const int CDS_TEST = 0x02;
        private const int CDS_FULLSCREEN = 0x04;
        
        #endregion
        
        private Resolution _originalResolution;
        private bool _hasStoredResolution = false;
        
        /// <summary>
        /// Gets the current screen resolution
        /// </summary>
        /// <returns>The current resolution</returns>
        public Resolution GetCurrentResolution()
        {
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode))
            {
                return new Resolution(devMode.dmPelsWidth, devMode.dmPelsHeight);
            }
            
            throw new InvalidOperationException("Failed to get current resolution");
        }
        
        /// <summary>
        /// Stores the current resolution so it can be reverted later
        /// </summary>
        public void StoreCurrentResolution()
        {
            _originalResolution = GetCurrentResolution();
            _hasStoredResolution = true;
        }
        
        /// <summary>
        /// Changes the screen resolution
        /// </summary>
        /// <param name="resolution">The new resolution to set</param>
        public void ChangeResolution(Resolution resolution)
        {
            // Store current resolution if not already stored
            if (!_hasStoredResolution)
            {
                StoreCurrentResolution();
            }
            
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode))
            {
                // Set the new resolution
                devMode.dmPelsWidth = resolution.Width;
                devMode.dmPelsHeight = resolution.Height;
                
                // Specify which fields to change
                devMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT;
                
                // Apply the changes
                int result = ChangeDisplaySettings(ref devMode, CDS_UPDATEREGISTRY);
                
                if (result != DISP_CHANGE_SUCCESSFUL)
                {
                    string errorMessage = result switch
                    {
                        DISP_CHANGE_BADMODE => "The specified graphics mode is not supported.",
                        DISP_CHANGE_FAILED => "The display driver failed the specified graphics mode.",
                        DISP_CHANGE_RESTART => "The computer must be restarted to apply these changes.",
                        DISP_CHANGE_BADDUALVIEW => "The settings change was unsuccessful because the system is DualView capable.",
                        _ => $"Unknown error changing resolution: {result}"
                    };
                    
                    throw new InvalidOperationException(errorMessage);
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to get current display settings");
            }
        }
        
        /// <summary>
        /// Reverts to the original resolution that was stored
        /// </summary>
        public void RevertResolution()
        {
            if (_hasStoredResolution)
            {
                ChangeResolution(_originalResolution);
                _hasStoredResolution = false;
            }
        }
    }
}