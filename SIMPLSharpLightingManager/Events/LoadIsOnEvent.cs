using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class LoadIsOnEvent : LoadEvent
    {
        //public bool IsOn;

        public LoadIsOnEvent(LightingLoad load)
            : base(load)
        {
        }

    }
}