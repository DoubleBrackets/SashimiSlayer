using GameInput.InputSources;

namespace GameInput.Interface
{
    /// <summary>
    ///     Interface providing user input for other modules to interact with
    /// </summary>
    public interface IUserInput : IUserInputSource
    {
        ControlSchemes ControlScheme { get; }

        public void AddInputBlocker();
        public void RemoveInputBlocker();
    }
}