using CommonTypes;
using Framework;
using Saving;
using UI.MenuViews;

namespace Menus.PauseMenu
{
    public abstract class PauseMenuView : MenuView, IFindsDependencies
    {
        protected SaveService SaveService;

        public void FindDependencies()
        {
            SaveService = ServiceLocator.GetGameSaveService();
        }
    }
}