using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Gateways;
using Crestron.SimplSharpPro.Lighting;
using SIMPLSharpLightingManager.Configuration;
using SIMPLSharpLightingManager.Entities;
using SIMPLSharpLightingManager.UserInterfaces;


namespace SIMPLSharpLightingManager.System
{
    public class LightingSystem : ILightingSystem
    {

        private IDictionary<int, GatewayBase> _gateways;
        private IDictionary<int, LightingBase> _lightDevices;

        private ControlSystem _system;
        private SettingsManager _settingsManager;

        private RoomManager _roomManager;
        private SceneManager _sceneManager;
        private LoadManager _loadManager;
        private TouchPanelManager _touchPanelManager;

        private LightDeviceSettings _deviceSettings;
        private LightSystemSettings _systemSettings;



        public LightingSystem(ControlSystem system, SettingsManager settingsManager, RoomManager roomManager, SceneManager sceneManager, LoadManager loadManager, TouchPanelManager touchPanelManager)
        {
            _system = system;
            _settingsManager = settingsManager;
            _roomManager = roomManager;
            _sceneManager = sceneManager;
            _loadManager = loadManager;
            _touchPanelManager = touchPanelManager;
        }

        public void ParseConfigFile()
        {
            var device = _settingsManager.ReadSettingsFile(@"\NVRAM\Settings\HE_Devices.dat");
            _deviceSettings = _settingsManager.GetDeviceConfiguration(device);

            var system = _settingsManager.ReadSettingsFile(@"\NVRAM\Settings\HE_Lighting.dat");
            _systemSettings = _settingsManager.GetLightSystemConfiguration(system);

        }

        public void InitializeDevices(LightDeviceSettings deviceSettings)
        {
            InitializeGateways(deviceSettings);
            InitializeLightingDevices(deviceSettings);
            InitializeTouchPanels(deviceSettings);
        }

        public void InitializeSystem(LightSystemSettings systemSettings)
        {
            InitializeLightingLoads(systemSettings);
            InitializeScenes(systemSettings);
            InitializeRooms(systemSettings);
        }

        public void InitializeGateways(LightDeviceSettings deviceSettings)
        {
            _gateways = new Dictionary<int, GatewayBase>(10);

            foreach (GatewayDeviceInformation gateway in deviceSettings.gateways)
            {

                Assembly myAssembly = Assembly.LoadFrom(@"Crestron.SimplSharpPro.Gateways.dll");

                CType myType = myAssembly.GetType("Crestron.SimplSharpPro.Gateways." + gateway.type);

                var ipid = SettingsManager.ConvertStringNumberToInt(gateway.ipid);
                var id = SettingsManager.ConvertStringNumberToInt(gateway.id);

                ConstructorInfo ctor = myType.GetConstructor(new CType[] { typeof(UInt32), typeof(ControlSystem) });
                object myInstance = ctor.Invoke(new object[] { (UInt32)ipid, this._system });

                _gateways.Add(id, (GatewayBase)myInstance);

            }
        }

        public void InitializeLightingDevices(LightDeviceSettings deviceSettings)
        {
            _lightDevices = new Dictionary<int, LightingBase>(100);

            foreach (LightingDeviceInformation device in deviceSettings.devices)
            {


                Assembly myAssembly = Assembly.LoadFrom(@"Crestron.SimplSharpPro.Lighting.dll");

                CType myType = myAssembly.GetType("Crestron.SimplSharpPro.Lighting." + device.type);

                var rfid = SettingsManager.ConvertStringNumberToInt(device.rfid);
                var gatewayId = SettingsManager.ConvertStringNumberToInt(device.gateway);
                var id = SettingsManager.ConvertStringNumberToInt(device.id);

                ConstructorInfo ctor = myType.GetConstructor(new CType[] { typeof(UInt32), typeof(GatewayBase) });

                GatewayBase gateway;


                if (_gateways.TryGetValue(gatewayId, out gateway))
                {

                    object myInstance = ctor.Invoke(new object[] { (UInt32)rfid, gateway });

                    _lightDevices.Add(id, (LightingBase)myInstance);


                }
                else
                {
                    CrestronConsole.PrintLine("Gateway not found with id of: " + device.gateway);
                }

            }

        }

        public void InitializeTouchPanels(LightDeviceSettings deviceSettings)
        {

            foreach (TouchPanelDeviceInformation touchpanel in deviceSettings.touchpanels)
            {

                Assembly myAssembly = Assembly.LoadFrom(@"Crestron.SimplSharpPro.EthernetCommunications.dll");

                CType myType = myAssembly.GetType("Crestron.SimplSharpPro.EthernetCommunication." + touchpanel.type);

                var ipid = SettingsManager.ConvertStringNumberToInt(touchpanel.ipid);
                var id = SettingsManager.ConvertStringNumberToInt(touchpanel.id);

                ConstructorInfo ctor = myType.GetConstructor(new CType[] { typeof(UInt32), typeof(string), typeof(ControlSystem) });
                object myInstance = ctor.Invoke(new object[] { (UInt32)ipid, "127.0.0.2", this._system });

                var touchPanelView = new TouchPanelView((BasicTriList)myInstance);
                var touchPanel1 = new TouchPanelPresenter(touchPanelView, _roomManager, _sceneManager);

                _touchPanelManager.AddTouchPanel(touchPanel1);

            }

        }

        public void InitializeLightingLoads(LightSystemSettings systemSettings)
        {

            foreach (SerializationLightLoad load in systemSettings.loads)
            {


                var deviceId = SettingsManager.ConvertStringNumberToInt(load.lightDeviceId);
                var loadId = SettingsManager.ConvertStringNumberToInt(load.id);
                var loadNumber = SettingsManager.ConvertStringNumberToInt(load.loadNumber);

                LightingBase lightDevice;

                if (_lightDevices.TryGetValue(deviceId, out lightDevice))
                {

                    if (lightDevice is Dimmer)
                    {

                        DimmingLoad dimmingLoad;

                        if (((Dimmer)lightDevice).DimmingLoads.TryGetValue((uint)loadNumber, out dimmingLoad))
                        {
                            _loadManager.AddLoadsFromCrestronLoad(loadId, load.name, dimmingLoad);
                        }
                        else
                        {
                            CrestronConsole.PrintLine("Unable to get load");
                        }

                    }
                    else if (lightDevice is Switch)
                    {

                        SwitchedLoad switchedLoad;

                        if (((Switch)lightDevice).SwitchedLoads.TryGetValue((uint)loadNumber, out switchedLoad))
                        {
                            _loadManager.AddLoadsFromCrestronLoad(loadId, load.name, switchedLoad);
                        }
                        else
                        {
                            CrestronConsole.PrintLine("Unable to get load");
                        }
                    }
                    else
                    {
                        CrestronConsole.PrintLine("Unknown light device type!");
                    }

                }
                else
                {
                    CrestronConsole.PrintLine("Cresron device not found with id of: " + load.lightDeviceId);
                }

            }
        }

        public void InitializeScenes(LightSystemSettings systemSettings)
        {

            foreach (SerializationScene scene in systemSettings.scenes)
            {

                var id = SettingsManager.ConvertStringNumberToInt(scene.id);

                Scene lightScene = new Scene(id, scene.name);

                if (scene.wholeHome)
                    lightScene.WholeHome = true;

                foreach (SerializationLoadState loadState in scene.loadStates)
                {
                    var level = SettingsManager.ConvertUnsignedStringNumber(loadState.level);

                    List<int> loadIds = null;

                    if (loadState.loadList == "all")
                    {
                        loadIds = _loadManager.GetLoads().Select(x => x.LoadID).ToList<int>();
                    }
                    else
                    {
                        loadIds = new List<int>(loadState.loadList.Split(',').Select(s => int.Parse(s)));
                    }

                    foreach (int loadID in loadIds)
                    {
                        LoadState state = new LoadState((ushort)level, LoadManager.GetLoad(loadID));
                        lightScene.AddLoadState(state);

                    }

                }

                _sceneManager.AddScene(lightScene);

            }
        }

        public void InitializeRooms(LightSystemSettings systemSettings)
        {

            foreach (SerializationRoom room in systemSettings.rooms)
            {

                var roomId = SettingsManager.ConvertStringNumberToInt(room.id);


                // var loadId = SettingsManager.ConvertStringNumberToInt(load.id);

                LightedRoom lightedRoom = new LightedRoom(roomId, room.name);

                List<int> loadIds = null;

                if (!String.IsNullOrEmpty(room.loadList))
                {

                    if (room.loadList == "all")
                    {
                        loadIds = _loadManager.GetLoads().Select(x => x.LoadID).ToList<int>();
                    }
                    else
                    {
                        loadIds = new List<int>(room.loadList.Split(',').Select(s => int.Parse(s)));
                    }

                    foreach (int loadId in loadIds)
                    {
                        lightedRoom.AddLoad(_loadManager.GetLoad(loadId));
                    }
                }

                if (!String.IsNullOrEmpty(room.sceneList))
                {
                    var sceneIds = new List<int>(room.sceneList.Split(',').Select(s => int.Parse(s)));

                    foreach (int sceneId in sceneIds)
                    {
                        Scene scene = _sceneManager.GetScene(sceneId);
                        scene.AssignRoom(lightedRoom);
                        lightedRoom.AddScene(scene);
                    }
                }

                _roomManager.AddRoom(lightedRoom);

            }
        }


        public void Startup()
        {
            ParseConfigFile();
            InitializeDevices(_deviceSettings);
            InitializeSystem(_systemSettings);
            RegisterGateways();
            RegisterTouchPanels();

        }

        public void RegisterGateways()
        {
            foreach (CenRfgwExEthernetSharable gateway in _gateways.Values)
            {
                gateway.Register();

            }

        }

        public void RegisterTouchPanels()
        {
            foreach (TouchPanelPresenter tp in _touchPanelManager.GetTouchPanels())
            {

                tp.BindPanel();
                tp.BindRooms();
                tp.BindScenesToView();
            }
        }

        public ControlSystem System
        {

            get
            {
                return _system;
            }

        }

        public RoomManager RoomManager
        {
            get
            {
                return _roomManager;
            }

        }


        public SceneManager SceneManager
        {

            get
            {
                return _sceneManager;
            }

        }

        public LoadManager LoadManager
        {
            get
            {
                return _loadManager;
            }

        }

        public TouchPanelManager TouchPanelManager
        {

            get
            {
                return _touchPanelManager;
            }

        }

    }
}