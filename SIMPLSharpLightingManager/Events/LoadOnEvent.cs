using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class LoadOnEvent : LoadEvent
    {
        //public bool IsOn;
        public uint Time;

        public LoadOnEvent(LightingLoad load)
            : base(load)
        {
        }

    }
}