using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

namespace SIMPLSharpLightingManager.Modules
{
    public class LightingManager
    {
        private RoomManager _roomManager;
        private SceneManager _sceneManager;
        private LoadManager _loadManager;
        private IHome _assignedHome;

        public LightingManager()
        {
            this._roomManager = new RoomManager();
            this._sceneManager = new SceneManager();
            this._loadManager = new LoadManager();
        }

        public void InitializeScenes()
        {
            /*
            Scene scene1 = new Scene("Scene 1");
            Load load1 = _loadManager.GetLoad(1);
            Load load2 = _loadManager.GetLoad(2);

            LoadState loadState1 = new LoadState(100, load1);
            LoadState loadState2 = new LoadState(100, load2);
            scene1.AddLoadState(loadState1);
            scene1.AddLoadState(loadState2);
             */
        }

        public void InitializeRooms()
        {
            /*
            LightedRoom room1 = new LightedRoom("Room 1");
            Scene scene1 = this._sceneManager.GetScene(1);
            room1.AddScene(scene1);
            this._roomManager.AddRoom(room1);
             */
        }

        public void InitializeLoads()
        {
            /*
            LightLoad load1 = null;
            LightLoad load2 = null;

            this._loadManager.AddLoad(load1);
            this._loadManager.AddLoad(load2);
            */


        }


        public SceneManager GetSceneManager()
        {
            return this._sceneManager;
        }

        public LoadManager GetLoadManager()
        {
            return this._loadManager;
        }

        public RoomManager GetRoomManager()
        {
            return this._roomManager;
        }
        
    }
}