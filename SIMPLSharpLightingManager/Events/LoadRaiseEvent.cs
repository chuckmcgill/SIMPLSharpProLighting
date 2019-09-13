using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class LoadRaiseEvent : LoadEvent
    {
        //public bool IsOn;
        public ushort Time;

        public LoadRaiseEvent(LightingLoad load)
            : base(load)
        {
        }

    }
}