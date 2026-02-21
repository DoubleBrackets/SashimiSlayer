using Framework.Services;

namespace Framework.GameModes
{
    public class MenuGameMode
    {
        private readonly LevelLoader _levelLoader;

        public MenuGameMode(LevelLoader levelLoader)
        {
            levelLoader = levelLoader;
        }
    }
}