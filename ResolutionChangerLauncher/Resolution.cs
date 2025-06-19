using System;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Represents a screen resolution with width and height
    /// </summary>
    public class Resolution
    {
        /// <summary>
        /// The width of the resolution in pixels
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// The height of the resolution in pixels
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// Gets a display name for the resolution (e.g., "1920 x 1080")
        /// </summary>
        public string DisplayName => $"{Width} x {Height}";
        
        /// <summary>
        /// Creates a new Resolution instance
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }
        
        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public Resolution()
        {
            Width = 1920;
            Height = 1080;
        }
        
        /// <summary>
        /// Returns a string representation of the resolution
        /// </summary>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}