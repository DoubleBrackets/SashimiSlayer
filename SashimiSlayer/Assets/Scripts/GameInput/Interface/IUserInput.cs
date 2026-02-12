using GameInput.InputSources;

namespace GameInput.Interface
{
    /// <summary>
    ///     Interface providing user input for other modules to interact with
    /// </summary>
    public interface IUserInput : IUserInputSource
    {
        public bool FlipParryDirection { get; }
        ControlSchemes ControlScheme { get; }

        public void AddInputBlocker();
        public void RemoveInputBlocker();
    }
}