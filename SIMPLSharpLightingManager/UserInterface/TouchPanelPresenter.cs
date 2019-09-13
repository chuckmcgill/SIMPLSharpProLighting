using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Easy.MessageHub;
using SIMPLSharpLightingManager.Entities;
using SIMPLSharpLightingManager.Events;
using SIMPLSharpLightingManager.System;

namespace SIMPLSharpLightingManager.UserInterfaces
{
    public class TouchPanelPresenter
    {

        //   private KuleszaHome _home;

        ITouchPanelView _view;

        private LightedRoom _attachedRoom;

        private RoomManager _roomManager;
        private SceneManager _sceneManager;

        private PagedList<LightingLoad> pagedLoads;
        private PagedList<LightedRoom> pagedRooms;
        private PagedList<Scene> pagedScenes;

        Dictionary<LightingLoad, List<SubscribedEvent>> loadEvents = new Dictionary<LightingLoad, List<SubscribedEvent>>();
        Dictionary<Scene, List<SubscribedEvent>> sceneEvents = new Dictionary<Scene, List<SubscribedEvent>>();

        public TouchPanelPresenter(ITouchPanelView touchPanelView, RoomManager roomManager, SceneManager sceneManager)
        {

            _view = touchPanelView;
            _roomManager = roomManager;
            _sceneManager = sceneManager;

            this._view.RoomSelected += OnRoomSelected;
            this._view.SceneSelected += OnSceneSelected;
            this._view.NextPageSelected += OnNextPageSelected;
            this._view.PreviousPageSelected += OnPreviousPageSelected;
            this._view.LoadToggleSelected += OnLoadToggleSelected;
            this._view.LoadRaiseSelected += OnLoadRaiseSelected;
            this._view.LoadRaiseDeselected += OnLoadRaiseDeselected;
            this._view.LoadLowerSelected += OnLoadLowerSelected;
            this._view.LoadLowerDeselected += OnLoadLowerDeselected;

        }

        public void BindPanel()
        {
            pagedRooms = new PagedList<LightedRoom>(_roomManager.GetRooms());
            pagedRooms.PageSize = 20;
            pagedRooms.GetFirstPage();

            pagedScenes = new PagedList<Scene>(_sceneManager.GetWholeHomeScenes());
            pagedScenes.PageSize = 10;
            pagedScenes.GetFirstPage();
        }

        public void BindRooms()
        {
            if (pagedRooms != null)
            {
                this._view.SetRoomCount((ushort)pagedRooms.TotalCount);

                for (int i = 0; i <= pagedRooms.Count - 1; i++)
                {
                    this._view.SetRoomName((uint)i, pagedRooms[i].Name);

                    //Room Status
                    //_EIScom.StringInput[startindex_room_name + (uint)index].StringValue = _roomManager.GetRooms()[index].Name;

                }
            }

        }

        public void BindScenesToView()
        {

            for (int j = pagedScenes.Count; j <= pagedScenes.PageSize - 1; j++)
            {
                _view.SetSceneVisible((uint)j, false);
                _view.SetSceneName((uint)j, " ");
                _view.SetSceneOff((uint)j);
            }

            UnsubscribeToNonVisibleEvents(pagedScenes, sceneEvents);

            for (int i = 0; i <= pagedScenes.Count - 1; i++)
            {

                var currentScene = pagedScenes[i];

                this._view.SetSceneName((uint)i, currentScene.Name);
                this._view.SetSceneVisible((uint)i, true);
                if (currentScene.AtLevel)
                {
                    this._view.SetSceneOn((uint)i);
                }
                else
                {
                    this._view.SetSceneOff((uint)i);
                }

                if (!sceneEvents.ContainsKey(currentScene))
                {
                    SubscribeToEvent(currentScene, typeof(SceneAtLevelEvent));
                }

            }

        }

        public void OnRoomSelected(uint index)
        {
            _attachedRoom = pagedRooms[(int)index];

            _view.SetSelectedRoom(index);
            _view.SetSelectedRoomName(index, _attachedRoom.Name);

            pagedLoads = new PagedList<LightingLoad>(_attachedRoom.GetLoads());
            pagedLoads.PageSize = 4;
            pagedLoads.GetFirstPage();

            pagedScenes = new PagedList<Scene>(_sceneManager.GetScenesByRoomID(_attachedRoom.RoomID));
            pagedScenes.PageSize = 10;
            pagedScenes.GetFirstPage();

            BindLoadsToView();
            BindScenesToView();

        }

        public void OnSceneSelected(uint index)
        {
            var scene = pagedScenes[(int)index];
            scene.Recall();
            //_view.SetSelectedScene(index);
        }

        public void OnPreviousPageSelected()
        {
            if (_attachedRoom != null)
            {
                if (pagedLoads != null)
                {
                    if (pagedLoads.HasPreviousPage)
                    {
                        pagedLoads.GetPreviousPage();
                        BindLoadsToView();
                    }
                }

            }
        }

        public void OnNextPageSelected()
        {

            if (_attachedRoom != null)
            {
                if (pagedLoads != null)
                {
                    if (pagedLoads.HasNextPage)
                    {
                        pagedLoads.GetNextPage();
                        BindLoadsToView();
                    }
                }

            }

        }

        public void OnLoadToggleSelected(uint index)
        {

            var load = pagedLoads[(int)index];
            load.Toggle();
        }

        public void OnLoadRaiseSelected(uint index)
        {
            var load = pagedLoads[(int)index];
            load.Raise();
            //  this._view.RaiseLoad(index, load.RaiseLowerRate);
        }

        public void OnLoadRaiseDeselected(uint index)
        {
            var load = pagedLoads[(int)index];
            load.StopRaise();
            //     this._view.StopRampLoad(index);
        }

        public void OnLoadLowerSelected(uint index)
        {
            var load = pagedLoads[(int)index];
            load.Lower();
            //   this._view.LowerLoad(index, load.RaiseLowerRate);
        }

        public void OnLoadLowerDeselected(uint index)
        {
            var load = pagedLoads[(int)index];
            load.StopLower();
            //    this._view.StopRampLoad(index);
        }

        public void BindLoadsToView()
        {

            for (int j = pagedLoads.Count; j <= pagedLoads.PageSize - 1; j++)
            {
                _view.HideLoad((uint)j);
                _view.SetLoadName((uint)j, "");
                _view.SetLoadOff((uint)j);
                _view.SetLoadLevel((uint)j, 0);
            }

            UnsubscribeToNonVisibleEvents(pagedLoads, loadEvents);

            for (int i = 0; i <= pagedLoads.Count - 1; i++)
            {

                var currentLoad = pagedLoads[i];

                _view.SetLoadType((uint)i, currentLoad.Type);
                _view.SetLoadName((uint)i, currentLoad.Name);
                _view.SetLoadLevel((uint)i, currentLoad.Level);
                if (currentLoad.IsOn())
                    _view.SetLoadOn((uint)i);
                else
                    _view.SetLoadOff((uint)i);

                if (!loadEvents.ContainsKey(currentLoad))
                {
                    SubscribeToEvent(currentLoad, typeof(LoadLevelEvent));
                    SubscribeToEvent(currentLoad, typeof(LoadOnEvent));
                    SubscribeToEvent(currentLoad, typeof(LoadOffEvent));
                    SubscribeToEvent(currentLoad, typeof(LoadIsOnEvent));
                    SubscribeToEvent(currentLoad, typeof(LoadIsOffEvent));
                    SubscribeToEvent(currentLoad, typeof(LoadRaiseEvent));
                    SubscribeToEvent(currentLoad, typeof(LoadLowerEvent));
                    SubscribeToEvent(currentLoad, typeof(LoadRampToLevelEvent));
                }

            }
        }

        public void ProcessSceneAtLevelEvent(SceneAtLevelEvent sceneEvent)
        {


            if (pagedScenes != null)
            {

                var index = pagedScenes.FindIndex(x => x.SceneID == sceneEvent.Scene.SceneID);

                if (index >= 0)
                {
                    if (sceneEvent.AtLevel)
                        this._view.SetSceneOn((uint)index);
                    else
                        this._view.SetSceneOff((uint)index);
                }

            }

        }

        public void ProcessLoadIsOnEvent(LoadIsOnEvent onEvent)
        {

            if (pagedLoads != null)
            {

                var index = pagedLoads.FindIndex(x => x.LoadID == onEvent.Load.LoadID);

                if (index >= 0)
                {
                    this._view.SetLoadOn((uint)index);
                }

            }

        }

        public void ProcessLoadIsOffEvent(LoadIsOffEvent onEvent)
        {

            if (pagedLoads != null)
            {

                var index = pagedLoads.FindIndex(x => x.LoadID == onEvent.Load.LoadID);

                if (index >= 0)
                {
                    this._view.SetLoadOff((uint)index);
                }

            }

        }

        public void ProcessLoadOnEvent(LoadOnEvent onEvent)
        {

            var index = pagedLoads.FindIndex(x => x.LoadID == onEvent.Load.LoadID);

            if (index >= 0)
            {
                this._view.SetLoadOn((uint)index);

                if (onEvent.Load.Type == eLoadType.Dimmer)
                {
                    this._view.RampLoadToLevel((uint)index, 65535, onEvent.Time);
                }
            }
        }

        public void ProcessLoadOffEvent(LoadOffEvent offEvent)
        {

            var index = pagedLoads.FindIndex(x => x.LoadID == offEvent.Load.LoadID);

            if (index >= 0)
            {
                this._view.SetLoadOff((uint)index);

                if (offEvent.Load.Type == eLoadType.Dimmer)
                {
                    this._view.RampLoadToLevel((uint)index, 0, offEvent.Time);
                }
            }
        }

        public void ProcessLoadRampToLevelEvent(LoadRampToLevelEvent levelEvent)
        {

            if (pagedLoads != null)
            {

                var index = pagedLoads.FindIndex(x => x.LoadID == levelEvent.Load.LoadID);

                if (index >= 0)
                {
                    this._view.RampLoadToLevel((uint)index, levelEvent.Level, levelEvent.Time);

                }

            }

        }

        public void ProcessLoadLowerEvent(LoadLowerEvent lowerEvent)
        {

            if (pagedLoads != null)
            {

                var index = pagedLoads.FindIndex(x => x.LoadID == lowerEvent.Load.LoadID);

                if (index >= 0)
                {
                    var load = pagedLoads[index];
                    if (load.IsRamping)
                    {
                        this._view.LowerLoad((uint)index, load.RaiseLowerRate);
                    }
                    if (!load.IsRamping)
                    {
                        this._view.StopRampLoad((uint)index);
                    }
                }

            }
        }

        public void ProcessLoadRaiseEvent(LoadRaiseEvent raiseEvent)
        {
            if (pagedLoads != null)
            {

                var index = pagedLoads.FindIndex(x => x.LoadID == raiseEvent.Load.LoadID);

                if (index >= 0)
                {
                    var load = pagedLoads[index];

                    if (load.IsRamping)
                    {
                        this._view.RaiseLoad((uint)index, load.RaiseLowerRate);
                    }
                    if (!load.IsRamping)
                    {
                        this._view.StopRampLoad((uint)index);
                    }
                }


            }

        }

        public void ProcessLoadLevelEvent(LoadLevelEvent levelEvent)
        {

        }

        public void SubscribeToEvents(IList<LightingLoad> loadList)
        {
            foreach (LightingLoad load in loadList)
            {
                SubscribeToEvent(load, typeof(LoadLevelEvent));
            }

        }

        public SubscribedEvent SubscribeToEvent(LightingLoad theLoad, Type eventType)
        {


            var map = MessageHub.Instance;

            SubscribedEvent sEvent = new SubscribedEvent();

            List<SubscribedEvent> sEvents;

            if (!loadEvents.TryGetValue(theLoad, out sEvents))
            {
                sEvents = new List<SubscribedEvent>();
                loadEvents.Add(theLoad, sEvents);
            }

            if (!sEvents.Any(x => x.eventType == eventType))
            {

                if (eventType == typeof(LoadLevelEvent))
                {
                    Action<LoadLevelEvent> loadEvent = null;
                    loadEvent = new Action<LoadLevelEvent>(x => this.ProcessLoadLevelEvent(x));
                    var loadID = theLoad.LoadID;
                    var token = map.Subscribe(loadEvent, x => x.Load.LoadID == loadID);

                    sEvent.token = token;
                    sEvent.eventType = typeof(LoadLevelEvent);
                }
                else if (eventType == typeof(LoadIsOnEvent))
                {
                    Action<LoadIsOnEvent> loadEvent = null;
                    loadEvent = new Action<LoadIsOnEvent>(x => this.ProcessLoadIsOnEvent(x));

                    var loadID = theLoad.LoadID;
                    var token = map.Subscribe(loadEvent, x => x.Load.LoadID == loadID);

                    sEvent.token = token;
                    sEvent.eventType = typeof(LoadIsOnEvent);
                }
                else if (eventType == typeof(LoadIsOffEvent))
                {
                    Action<LoadIsOffEvent> loadEvent = null;
                    loadEvent = new Action<LoadIsOffEvent>(x => this.ProcessLoadIsOffEvent(x));

                    var loadID = theLoad.LoadID;
                    var token = map.Subscribe(loadEvent, x => x.Load.LoadID == loadID);

                    sEvent.token = token;
                    sEvent.eventType = typeof(LoadIsOffEvent);
                }
                else if (eventType == typeof(LoadOnEvent))
                {
                    Action<LoadOnEvent> loadEvent = null;
                    loadEvent = new Action<LoadOnEvent>(x => this.ProcessLoadOnEvent(x));

                    var loadID = theLoad.LoadID;
                    var token = map.Subscribe(loadEvent, x => x.Load.LoadID == loadID);

                    sEvent.token = token;
                    sEvent.eventType = typeof(LoadOnEvent);
                }
                else if (eventType == typeof(LoadOffEvent))
                {
                    Action<LoadOffEvent> loadEvent = null;
                    loadEvent = new Action<LoadOffEvent>(x => this.ProcessLoadOffEvent(x));

                    var loadID = theLoad.LoadID;
                    var token = map.Subscribe(loadEvent, x => x.Load.LoadID == loadID);

                    sEvent.token = token;
                    sEvent.eventType = typeof(LoadOffEvent);
                }
                else if (eventType == typeof(LoadRaiseEvent))
                {
                    Action<LoadRaiseEvent> loadEvent = null;
                    loadEvent = new Action<LoadRaiseEvent>(x => this.ProcessLoadRaiseEvent(x));

                    var loadID = theLoad.LoadID;
                    var token = map.Subscribe(loadEvent, x => x.Load.LoadID == loadID);

                    sEvent.token = token;
                    sEvent.eventType = typeof(LoadRaiseEvent);
                }
                else if (eventType == typeof(LoadLowerEvent))
                {
                    Action<LoadLowerEvent> loadEvent = null;
                    loadEvent = new Action<LoadLowerEvent>(x => this.ProcessLoadLowerEvent(x));

                    var loadID = theLoad.LoadID;
                    var token = map.Subscribe(loadEvent, x => x.Load.LoadID == loadID);

                    sEvent.token = token;
                    sEvent.eventType = typeof(LoadLowerEvent);
                }
                else if (eventType == typeof(LoadRampToLevelEvent))
                {
                    Action<LoadRampToLevelEvent> levelSetEvent = null;
                    levelSetEvent = new Action<LoadRampToLevelEvent>(x => this.ProcessLoadRampToLevelEvent(x));

                    var loadID = theLoad.LoadID;
                    var token = map.Subscribe(levelSetEvent, x => x.Load.LoadID == loadID);

                    sEvent.token = token;
                    sEvent.eventType = typeof(LoadRampToLevelEvent);
                }
                else
                {
                    throw new Exception("Event Type not found");
                }

                sEvents.Add(sEvent);
            }
            else
            {
                CrestronConsole.PrintLine("EventType: " + eventType + " already subscribed for LoadID: " + theLoad.LoadID);
            }

            return sEvent;

        }

        public SubscribedEvent SubscribeToEvent(Scene theScene, Type eventType)
        {


            var map = MessageHub.Instance;

            SubscribedEvent sEvent = new SubscribedEvent();

            if (eventType == typeof(SceneAtLevelEvent))
            {
                Action<SceneAtLevelEvent> sceneEvent = null;
                sceneEvent = new Action<SceneAtLevelEvent>(x => this.ProcessSceneAtLevelEvent(x));
                var sceneID = theScene.SceneID;
                var token = map.Subscribe(sceneEvent, x => x.Scene.SceneID == sceneID);

                sEvent.token = token;
                sEvent.eventType = typeof(SceneAtLevelEvent);
            }
            else
            {
                throw new Exception("Event Type not found");
            }


            //Need To Add Subscription Logic for Scene


            List<SubscribedEvent> list;

            if (!sceneEvents.TryGetValue(theScene, out list))
            {
                list = new List<SubscribedEvent>();
                sceneEvents.Add(theScene, list);
            }

            list.Add(sEvent);

            return sEvent;

        }

        void UnsubscribeToNonVisibleEvents(IEnumerable<LightingLoad> a, Dictionary<LightingLoad, List<SubscribedEvent>> b)
        {
            var result = b.Where(p => a.All(p2 => p2.LoadID != p.Key.LoadID)).ToList();

            for (int i = result.Count - 1; i >= 0; i--)
            {
                UnsubscribeToEvents(result[i].Key);
            }

        }


        void UnsubscribeToNonVisibleEvents(IEnumerable<Scene> a, Dictionary<Scene, List<SubscribedEvent>> b)
        {
            var result = b.Where(p => a.All(p2 => p2.SceneID != p.Key.SceneID)).ToList();

            for (int i = result.Count - 1; i >= 0; i--)
            {
                UnsubscribeToEvents(result[i].Key);
            }

        }

        void UnsubscribeToEvents(LightingLoad theLoad)
        {
            List<SubscribedEvent> list;

            if (loadEvents.TryGetValue(theLoad, out list))
            {
                if (list != null)
                {
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        var type = list[i].eventType;
                        UnsubscribeToEventByType(theLoad, type);

                        //Reset if we remove more than 1
                        if (i > list.Count)
                            i = list.Count;
                    }
                }

            }

        }

        void UnsubscribeToEvents(Scene theScene)
        {
            List<SubscribedEvent> list;

            if (sceneEvents.TryGetValue(theScene, out list))
            {
                if (list != null)
                {

                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        var type = list[i].eventType;
                        UnsubscribeToEventByType(theScene, type);

                        //Reset if we remove more than 1
                        if (i > list.Count)
                            i = list.Count;
                    }
                }

            }

        }


        void UnsubscribeToEvents(IList<LightingLoad> loadList)
        {
            for (int i = loadList.Count - 1; i >= 0; i--)
            {
                UnsubscribeToEvents(loadList[i]);
            }

        }

        void UnsubscribeToEvents(IList<Scene> sceneList)
        {
            for (int i = sceneList.Count - 1; i >= 0; i--)
            {
                UnsubscribeToEvents(sceneList[i]);
            }

        }

        void UnsubscribeToEventByType(LightingLoad theLoad, Type eventType)
        {
            List<SubscribedEvent> list;

            if (loadEvents.TryGetValue(theLoad, out list))
            {
                //list = new List<SubscribedEvent>();
                //  mydict.Add(theLoad, list);


                // list.Remove(loadEvent);

                var map = MessageHub.Instance;
                foreach (SubscribedEvent subEvent in list.Where(x => x.eventType == eventType))
                {
                    map.Unsubscribe(subEvent.token);
                }

                var countRemoved = list.RemoveAll(x => x.eventType == eventType);


                if (list.Count > 0)
                {
                    loadEvents[theLoad] = list;
                }
                else
                {
                    loadEvents.Remove(theLoad);
                }

            }
        }

        void UnsubscribeToEventByType(Scene theScene, Type eventType)
        {
            List<SubscribedEvent> list;

            if (sceneEvents.TryGetValue(theScene, out list))
            {
                //list = new List<SubscribedEvent>();
                //  mydict.Add(theLoad, list);


                // list.Remove(loadEvent);

                var map = MessageHub.Instance;
                foreach (SubscribedEvent subEvent in list.Where(x => x.eventType == eventType))
                {
                    map.Unsubscribe(subEvent.token);
                }

                var countRemoved = list.RemoveAll(x => x.eventType == eventType);

                if (list.Count > 0)
                {
                    sceneEvents[theScene] = list;
                }
                else
                {
                    sceneEvents.Remove(theScene);
                }

            }
        }


    }
}