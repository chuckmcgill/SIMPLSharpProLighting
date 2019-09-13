using System.Collections.Generic;
using SIMPLSharpLightingManager.UserInterfaces;

namespace SIMPLSharpLightingManager.System
{
    public class TouchPanelManager
    {
        private IList<TouchPanelPresenter> _touchPanels;
        // private LoadManager _loadManager;

        public TouchPanelManager()
        {
            _touchPanels = new List<TouchPanelPresenter>(100);
        }

        public void AddTouchPanel(TouchPanelPresenter panel)
        {
            this._touchPanels.Add(panel);
        }

        public void DeleteTouchPanel(int panelID)
        {
            return;
        }

        public TouchPanelPresenter GetTouchPanel(int panelID)
        {
            return null;
            // return _touchPanels.FirstOrDefault(x => x. == RoomID);
        }

        public IList<TouchPanelPresenter> GetTouchPanels()
        {
            return this._touchPanels;
        }
    }
}