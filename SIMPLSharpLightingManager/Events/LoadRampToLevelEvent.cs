using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class LoadRampToLevelEvent : LoadEvent
    {
        public ushort Level;
        public ushort Time;

        public LoadRampToLevelEvent(LightingLoad load)
            : base(load)
        {
        }

    }
}