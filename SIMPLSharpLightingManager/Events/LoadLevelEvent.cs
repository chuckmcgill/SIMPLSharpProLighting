using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class LoadLevelEvent : LoadEvent
    {
        public ushort Level;

        public LoadLevelEvent(LightingLoad load)
            : base(load)
        {
        }
    }
}