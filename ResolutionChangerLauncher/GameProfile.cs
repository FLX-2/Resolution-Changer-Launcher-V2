using System;
using System.Text.Json.Serialization;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Represents a saved game profile with path and resolution settings
    /// </summary>
    public class GameProfile
    {
        /// <summary>
        /// The display name of the game
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// The path to the game executable or shortcut
        /// </summary>
        public string Path { get; set; } = string.Empty;
        
        /// <summary>
        /// The resolution to use when launching the game
        /// </summary>
        public Resolution Resolution { get; set; } = new Resolution(1920, 1080);
        
        /// <summary>
        /// Whether to revert the resolution when the game exits
        /// </summary>
        public bool RevertResolution { get; set; } = true;
    }
}