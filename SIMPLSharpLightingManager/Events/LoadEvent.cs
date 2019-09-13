using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class LoadEvent
    {
        public string EventType;
        private LightingLoad _load;

        public LoadEvent(LightingLoad load)
        {
            _load = load;
        }

        public LightingLoad Load
        {
            get
            {
                return _load;
            }
        }
    }
}