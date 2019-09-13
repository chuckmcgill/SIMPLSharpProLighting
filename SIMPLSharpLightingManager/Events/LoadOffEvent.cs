using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class LoadOffEvent : LoadEvent
    {
        //public bool IsOn;
        public ushort Time;

        public LoadOffEvent(LightingLoad load)
            : base(load)
        {
        }

    }
}