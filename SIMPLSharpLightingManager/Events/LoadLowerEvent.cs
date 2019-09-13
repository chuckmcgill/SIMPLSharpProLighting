using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class LoadLowerEvent : LoadEvent
    {
        //public bool IsOn;
        public ushort Time;

        public LoadLowerEvent(LightingLoad load)
            : base(load)
        {
        }

    }
}