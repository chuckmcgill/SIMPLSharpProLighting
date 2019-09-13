using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class RoomEvent
    {
        public string EventType;
        private LightedRoom _room;


        public RoomEvent(LightedRoom Room)
        {
            _room = Room;
        }

        public LightedRoom Room
        {
            get
            {
                return _room;
            }
        }
    }
}