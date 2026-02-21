using Framework.Services;
using Saving;
using UI.MenuViews;

namespace GlobalUI.PauseMenu
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