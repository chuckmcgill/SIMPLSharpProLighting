using System;
using System.Collections.Generic;
using Easy.MessageHub;
using SIMPLSharpLightingManager.Events;

namespace SIMPLSharpLightingManager.Entities
{
    public class LightedRoom
    {
        private int _roomID;

        private IList<Scene> _scenes;
        private String _name;
        private IList<LightingLoad> _loads;

        public LightedRoom(int RoomID, String name)
        {
            _roomID = RoomID;
            _loads = new List<LightingLoad>(100);
            _scenes = new List<Scene>(100);

            this._name = name;
        }

        public void AddScene(Scene scene)
        {
            this._scenes.Add(scene);
            //    scene.AssignRoom(this);

        }

        public void RemoveScene(int sceneID)
        {
            return;
        }

        public IList<LightingLoad> GetLoads()
        {
            return this._loads;
        }

        public void AddLoad(LightingLoad load)
        {
            _loads.Add(load);

            /*
            var hub = MessageHub.Instance;
            Action<LoadEvent> loadEvent = new Action<LoadEvent>(x => this.ProcessLoadEvent(x));
            
            //This is a reference to my event
            //Probably need to keep a local copy of all events I'm subscribed to
            var token = hub.Subscribe(loadEvent);
            */
        }

        public void RemoveLoad(LightingLoad load)
        {
            _loads.Remove(load);
            /*
            var hub = MessageHub.Instance;
             */
            //TODO: Unsubscribe

        }

        private void ProcessLoadEvent(LoadEvent loadEvent)
        {
            //Turn Room On/Off based on event

        }

        public void LoadChanged(LightingLoad load, EventArgs args)
        {

            //_EIScom.BooleanInput[isOn].BoolValue = true;

        }

        public String Name
        {
            get
            {
                return _name;
            }
        }

        public void RoomOff()
        {
            foreach (LightingLoad currentLoad in _loads)
            {
                currentLoad.Off();
            }

            var hub = MessageHub.Instance;

            RoomOnEvent message = new RoomOnEvent(this);
            message.IsOn = false;

            hub.Publish<RoomOnEvent>(message);
        }

        public void RoomOn()
        {
            foreach (LightingLoad currentLoad in _loads)
            {
                currentLoad.FullOn();
            }

            var hub = MessageHub.Instance;

            RoomOnEvent message = new RoomOnEvent(this);
            message.IsOn = true;

            hub.Publish<RoomOnEvent>(message);

        }

        public void RoomRaise()
        {
            throw new Exception("Not Implemented");
        }

        public void RoomLower()
        {
            throw new Exception("Not Implemented");
        }

        public int RoomID
        {
            get
            {
                return _roomID;
            }
        }

    }
}