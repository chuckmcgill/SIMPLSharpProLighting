using System.Collections.Generic;
using System.Linq;
using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.System
{
    public class RoomManager
    {
        private IList<LightedRoom> _rooms;
        // private LoadManager _loadManager;

        public RoomManager()
        {
            _rooms = new List<LightedRoom>(100);
        }

        public void AddRoom(LightedRoom room)
        {
            this._rooms.Add(room);
        }

        public void DeleteRoom(int RoomID)
        {
            return;
        }

        public LightedRoom GetRoom(int RoomID)
        {
            return _rooms.FirstOrDefault(x => x.RoomID == RoomID);
        }

        public IList<LightedRoom> GetRooms()
        {
            return this._rooms;
        }
    }
}