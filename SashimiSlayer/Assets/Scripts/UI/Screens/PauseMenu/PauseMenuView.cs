using GameJuice.ScreenShake;
using Saving;
using UI.MenuViews;

namespace UI.Screens.PauseMenu
{
    public abstract class PauseMenuView : MenuView
    {
        protected SaveService SaveService;
        protected ScreenShakeService ScreenShakeService;

        public void Initialize(SaveService saveService, ScreenShakeService screenShakeService)
        {
            SaveService = saveService;
            ScreenShakeService = screenShakeService;
        }
    }
}