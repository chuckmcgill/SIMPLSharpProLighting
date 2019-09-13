using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Easy.MessageHub;
using SIMPLSharpLightingManager.Events;

namespace SIMPLSharpLightingManager.Entities
{
    public class Scene
    {
        private readonly CCriticalSection critSection;
        private IList<LightedRoom> _rooms;
        private IList<LoadState> _storedLoadState;
        private bool _wholeHome;
        //  private IList<LoadState> _currentLoadState;
        private String _name;
        private int _sceneID;
        private bool _atLevel;


        CCriticalSection section = new CCriticalSection();

        Dictionary<LightingLoad, List<SubscribedEvent>> mydict = new Dictionary<LightingLoad, List<SubscribedEvent>>();


        public int SceneID
        {
            get
            {
                return _sceneID;
            }

        }

        public String Name
        {
            get
            {
                return _name;
            }

        }

        public Scene(int sceneID, String name)
        {
            critSection = new CCriticalSection();
            this._sceneID = sceneID;
            this._storedLoadState = new List<LoadState>();
            this._rooms = new List<LightedRoom>(100);
            this._name = name;
            this._atLevel = true;
        }

        public void SubscribeToLoadEvents(LightingLoad _load)
        {
            SubscribeToEvent(_load, typeof(LoadLevelEvent));
        }

        public SubscribedEvent SubscribeToEvent(LightingLoad theLoad, Type eventType)
        {


            var map = MessageHub.Instance;

            SubscribedEvent sEvent = new SubscribedEvent();

            if (eventType == typeof(LoadLevelEvent))
            {
                Action<LoadLevelEvent> loadEvent = null;
                loadEvent = new Action<LoadLevelEvent>(x => this.ProcessLoadLevelEvent(x));
                var loadID = theLoad.LoadID;
                var token = map.Subscribe(loadEvent, x => x.Load.LoadID == loadID);

                sEvent.token = token;
                sEvent.eventType = typeof(LoadLevelEvent);
            }
            else
            {
                throw new Exception("Event Type not found");
            }

            List<SubscribedEvent> list;

            if (!mydict.TryGetValue(theLoad, out list))
            {
                list = new List<SubscribedEvent>();
                mydict.Add(theLoad, list);
            }

            list.Add(sEvent);

            return sEvent;

        }

        public void ProcessLoadLevelEvent(LoadLevelEvent levelEvent)
        {
            LoadState loadState = _storedLoadState.FirstOrDefault(x => x._load.LoadID == levelEvent.Load.LoadID);


            if (loadState.AtLevel())
            {
                var count = _storedLoadState.Where(x => x.AtLevel() != true).Count();
                SceneAtLevel(count == 0);
            }
            else
            {
                SceneAtLevel(false);
            }

        }

        public void SceneAtLevel(bool atLevel)
        {

            if (this._atLevel != atLevel)
            {
                this._atLevel = atLevel;

                var hub = MessageHub.Instance;
                SceneAtLevelEvent atLevelEvent = new SceneAtLevelEvent(this, atLevel);


                //CrestronConsole.PrintLine("Sending Scene At Level");
                hub.Publish<SceneAtLevelEvent>(atLevelEvent);
            }
        }

        public void AddLoadState(LoadState loadState)
        {
            this._storedLoadState.Add(loadState);
            SubscribeToLoadEvents(loadState._load);

            if (loadState.AtLevel())
            {
                var count = _storedLoadState.Where(x => x.AtLevel() != true).Count();
                SceneAtLevel(count == 0);
            }
            else
            {
                SceneAtLevel(false);
            }
        }

        public void RemoveLoadState(int loadID)
        {
            return;
        }

        public void Recall()
        {

            this.critSection.Enter();

            /*
            
            if (_storedLoadState.Count > 10)
            {
                var list1 = ((List<LoadState>)_storedLoadState).GetRange(0, 10);
                var list2 = ((List<LoadState>)_storedLoadState).GetRange(10, 9);

                CrestronInvoke.BeginInvoke(o => RecallLoadStates(list1));
                CrestronInvoke.BeginInvoke(o => RecallLoadStates(list2));

            } else {
            */

            foreach (LoadState loadstate in _storedLoadState)
            {
                loadstate.Recall();
            }

            this.critSection.Leave();

            //    }


            //   SceneAtLevel(true);

        }

        private void RecallLoadStates(IList<LoadState> loadStates)
        {
            // var localLoadStates = ((List<LoadState>) o);



            foreach (LoadState loadstate in loadStates)
            {
                loadstate.Recall();
            }


        }

        /*
        private void RecallLoadStates(IList<LoadState> loadstates)
        {

            foreach (LoadState loadstate in _storedLoadState)
            {
                loadstate.Recall();
            }

        }*/

        public void AssignRoom(LightedRoom room)
        {
            this._rooms.Add(room);
            //   room.AddScene(this);

        }

        public void RemoveRoom(int roomID)
        {
            return;
        }

        public bool WholeHome
        {
            get
            {
                return _wholeHome;
            }

            set
            {

                _wholeHome = value;

            }

        }

        public IList<LightedRoom> Rooms
        {
            get
            {
                return _rooms;
            }

        }

        public bool AtLevel
        {
            get
            {
                return _atLevel;
            }

        }
    }
}