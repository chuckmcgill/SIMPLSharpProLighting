using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class RoomOnEvent : RoomEvent
    {
        public bool IsOn;

        public RoomOnEvent(LightedRoom Room)
            : base(Room)
        {
        }
    }
}