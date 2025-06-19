using System;

namespace ResolutionChangerLauncher
{
    /// <summary>
    /// Event arguments for when a game process exits
    /// </summary>
    public class GameExitedEventArgs : EventArgs
    {
        /// <summary>
        /// The process ID of the game that exited
        /// </summary>
        public int ProcessId { get; }
        
        /// <summary>
        /// Whether to revert the resolution when the game exits
        /// </summary>
        public bool RevertResolution { get; }
        
        /// <summary>
        /// Creates a new GameExitedEventArgs instance
        /// </summary>
        /// <param name="processId">The process ID of the game that exited</param>
        /// <param name="revertResolution">Whether to revert the resolution</param>
        public GameExitedEventArgs(int processId, bool revertResolution)
        {
            ProcessId = processId;
            RevertResolution = revertResolution;
        }
    }
}