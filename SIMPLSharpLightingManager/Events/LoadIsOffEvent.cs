using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class LoadIsOffEvent : LoadEvent
    {
        //public bool IsOn;


        public LoadIsOffEvent(LightingLoad load)
            : base(load)
        {
        }

    }
}