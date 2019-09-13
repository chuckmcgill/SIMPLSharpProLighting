using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Lighting;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Gateways;
using Crestron.SimplSharpPro.EthernetCommunication;
using SIMPLSharpLightingManager.View;

namespace SIMPLSharpLightingManager.Modules
{
    public class KuleszaHome :  ILightingSystem
    {

        private IList<CenRfgwExEthernetSharable> gateways;

        private ControlSystem _system;

     //   public LightingManager _lightingManager;
        private RoomManager _roomManager;
        private SceneManager _sceneManager;
        private LoadManager _loadManager;
        private TouchPanelManager _touchPanelManager;


        public KuleszaHome(ControlSystem system, RoomManager roomManager, SceneManager sceneManager, LoadManager loadManager, TouchPanelManager touchPanelManager)
        {
            _system = system;
            _roomManager = roomManager;
            _sceneManager = sceneManager;
            _loadManager = loadManager;
            _touchPanelManager = touchPanelManager;

        }

        public void RegisterDevices()
        {
            RegisterGateways();
            RegisterLoads();
            RegisterRooms();
            RegisterLoadsToRooms();
            RegisterScenes();
            RegisterUIDevice();
        }


        public void RegisterGateways()
        {
            gateways = new List<CenRfgwExEthernetSharable>();

            CenRfgwExEthernetSharable gateway1 = new CenRfgwExEthernetSharable(0x20, _system);
            CenRfgwExEthernetSharable gateway2 = new CenRfgwExEthernetSharable(0x21, _system);

         //   gateway2.Register();
            
            gateways.Add(gateway1);
            gateways.Add(gateway2);
            
        }

        public void RegisterUIDevice()
        {
            var eisCOM = new EthernetIntersystemCommunications(0x28, "127.0.0.2", _system);

            var touchPanelView = new TouchPanelView(eisCOM);
            var touchPanel1 = new TouchPanelPresenter(touchPanelView, _roomManager, _sceneManager);

            _touchPanelManager.AddTouchPanel(touchPanel1);

            
            touchPanel1.BindRooms();
            touchPanel1.BindScenes();
          
        }


        public void RegisterRooms()
        {
            LightedRoom lightedRoom1 = new LightedRoom(1, "Room 1");

            LightedRoom lightedRoom2 = new LightedRoom(2, "Room 2");

            LightedRoom lightedRoom3 = new LightedRoom(3, "Room 3");

            LightedRoom lightedRoom4 = new LightedRoom(4, "Room 4");

            LightedRoom lightedRoom5 = new LightedRoom(5, "Entire Home");

            _roomManager.AddRoom(lightedRoom1);
            _roomManager.AddRoom(lightedRoom2);
            _roomManager.AddRoom(lightedRoom3);
            _roomManager.AddRoom(lightedRoom4);
            _roomManager.AddRoom(lightedRoom5);
        }

        public void RegisterLoadsToRooms()
        {
            var lightedRoom1 = _roomManager.GetRoom(1);
         //   lightedRoom1.AddLoad(_loadManager.GetLoad(1));
            lightedRoom1.AddLoad(_loadManager.GetLoad(2));
         //   lightedRoom1.AddLoad(_loadManager.GetLoad(3));

            var lightedRoom2 = _roomManager.GetRoom(2);
         //   lightedRoom2.AddLoad(_loadManager.GetLoad(1));
         //   lightedRoom2.AddLoad(_loadManager.GetLoad(3));
            lightedRoom2.AddLoad(_loadManager.GetLoad(6));
       //     lightedRoom2.AddLoad(_loadManager.GetLoad(7));

            var lightedRoom3 = _roomManager.GetRoom(3);
            lightedRoom3.AddLoad(_loadManager.GetLoad(2));
            lightedRoom3.AddLoad(_loadManager.GetLoad(6));
        //    lightedRoom3.AddLoad(_loadManager.GetLoad(7));
            lightedRoom3.AddLoad(_loadManager.GetLoad(8));

            var lightedRoom4 = _roomManager.GetRoom(4);
         //   lightedRoom4.AddLoad(_loadManager.GetLoad(1));
            lightedRoom4.AddLoad(_loadManager.GetLoad(2));
         //   lightedRoom4.AddLoad(_loadManager.GetLoad(3));
            lightedRoom4.AddLoad(_loadManager.GetLoad(4));
        //    lightedRoom4.AddLoad(_loadManager.GetLoad(5));

            var lightedRoom5 = _roomManager.GetRoom(5);
          //  lightedRoom5.AddLoad(_loadManager.GetLoad(1));
            lightedRoom5.AddLoad(_loadManager.GetLoad(2));
         //   lightedRoom5.AddLoad(_loadManager.GetLoad(3));
            lightedRoom5.AddLoad(_loadManager.GetLoad(4));
          //  lightedRoom5.AddLoad(_loadManager.GetLoad(5));
            lightedRoom5.AddLoad(_loadManager.GetLoad(6));
          //  lightedRoom5.AddLoad(_loadManager.GetLoad(7));
            lightedRoom5.AddLoad(_loadManager.GetLoad(8));
          //  lightedRoom5.AddLoad(_loadManager.GetLoad(9));
            lightedRoom5.AddLoad(_loadManager.GetLoad(10));
            lightedRoom5.AddLoad(_loadManager.GetLoad(11));
            lightedRoom5.AddLoad(_loadManager.GetLoad(12));
            lightedRoom5.AddLoad(_loadManager.GetLoad(13));
            lightedRoom5.AddLoad(_loadManager.GetLoad(14));
            lightedRoom5.AddLoad(_loadManager.GetLoad(15));
            lightedRoom5.AddLoad(_loadManager.GetLoad(16));
     //       lightedRoom5.AddLoad(_loadManager.GetLoad(17));
     //       lightedRoom5.AddLoad(_loadManager.GetLoad(18));
     //       lightedRoom5.AddLoad(_loadManager.GetLoad(19));
        }

        public void RegisterLoads()
        {
            CenRfgwExEthernetSharable gateway1 = gateways.First(x => x.ID == 0x20);

            
                ClwSwexP switch1 = new ClwSwexP(0x21, gateway1);
                switch1.Description = "Switch ID:1,21";

                ClwSwexP switch2 = new ClwSwexP(0x22, gateway1);
                switch2.Description = "Switch ID:1,22";

                ClwSwexP switch3 = new ClwSwexP(0x23, gateway1);
                switch3.Description = "Switch ID:1,23";

                ClwSwexP switch4 = new ClwSwexP(0x24, gateway1);
                switch4.Description = "Switch ID:1,24";

                ClwSwexP switch5 = new ClwSwexP(0x25, gateway1);
                switch5.Description = "Switch ID:1,25";

                ClwDimexP dimmer1 = new ClwDimexP(0x20, gateway1);
                dimmer1.Description = "Dimmer ID:1,20";


                //Master Hallway
                ClwDimexP dimmer2 = new ClwDimexP(0x26, gateway1);
                dimmer2.Description = "Dimmer ID:1,26";
                dimmer2.DimmerUISettings.ParameterButtonLogic = eButtonLogic.Local;


                //Main Hallway
                ClwDimexP dimmer3 = new ClwDimexP(0x27, gateway1);
                dimmer3.Description = "Dimmer ID:1,27";
                dimmer3.DimmerUISettings.ParameterButtonLogic = eButtonLogic.Local;
                


                ClwDimexP dimmer4 = new ClwDimexP(0x28, gateway1);
                dimmer4.Description = "Dimmer ID:1,28";

                ClwDimexP dimmer5 = new ClwDimexP(0x29, gateway1);
                dimmer5.Description = "Dimmer ID:1,29";

                ClwDimexP dimmer6 = new ClwDimexP(0x30, gateway1);
                dimmer6.Description = "Dimmer ID:1,30";



                
                CenRfgwExEthernetSharable gateway2 = gateways.FirstOrDefault(x => x.ID == 0x21);

                ClwDimexP dimmer21 = new ClwDimexP(0x20, gateway2);
                dimmer21.Description = "Dimmer ID:2,20";

                ClwDimexP dimmer22 = new ClwDimexP(0x21, gateway2);
                dimmer22.Description = "Dimmer ID:2,21";

                ClwDimexP dimmer23 = new ClwDimexP(0x25, gateway2);
                dimmer23.Description = "Dimmer ID:2,25";

                ClwDimexP dimmer24 = new ClwDimexP(0x26, gateway2);
                dimmer24.Description = "Dimmer ID:2,26";

                ClwDimexP dimmer25 = new ClwDimexP(0x27, gateway2);
                dimmer25.Description = "Dimmer ID:2,27";

                ClwDimexP dimmer26 = new ClwDimexP(0x28, gateway2);
                dimmer26.Description = "Dimmer ID:2,28";


                ClwSwexP switch21 = new ClwSwexP(0x22, gateway2);
                switch21.Description = "Switch ID:2,22";

                ClwSwexP switch22 = new ClwSwexP(0x23, gateway2);
                switch22.Description = "Switch ID:2,23";

                ClwSwexP switch23 = new ClwSwexP(0x24, gateway2);
                switch23.Description = "Switch ID:2,24";
                 


                //FanDelvEx 0B, 12, 18, 19
                /*

                ClcFandelvex fan1 = new ClcFandelvex(0x0B, gateway1);
                ClcFandelvex fan2 = new ClcFandelvex(0x12, gateway1);
                ClcFandelvex fan3 = new ClcFandelvex(0x18, gateway1);
                ClcFandelvex fan4 = new ClcFandelvex(0x19, gateway1);

               // fan1.
                 */



                // dimmer1.DimmerUISettings.SetPresetDefaultLevel(1, 0);

             //   _loadManager.AddLoadsFromCrestronDevice(1, switch1);
                _loadManager.AddLoadsFromCrestronDevice(2, dimmer1);
             //   _loadManager.AddLoadsFromCrestronDevice(3, switch2);
                _loadManager.AddLoadsFromCrestronDevice(4, dimmer2);
             //   _loadManager.AddLoadsFromCrestronDevice(5, switch3);
                _loadManager.AddLoadsFromCrestronDevice(6, dimmer3);
             //   _loadManager.AddLoadsFromCrestronDevice(7, switch4);
                _loadManager.AddLoadsFromCrestronDevice(8, dimmer4);
             //   _loadManager.AddLoadsFromCrestronDevice(9, switch5);
                _loadManager.AddLoadsFromCrestronDevice(10, dimmer5);
                _loadManager.AddLoadsFromCrestronDevice(11, dimmer6);

                gateway1.Register();

                _loadManager.AddLoadsFromCrestronDevice(12, dimmer21);
                _loadManager.AddLoadsFromCrestronDevice(13, dimmer22);
                _loadManager.AddLoadsFromCrestronDevice(14, dimmer23);
                _loadManager.AddLoadsFromCrestronDevice(15, dimmer24);
                _loadManager.AddLoadsFromCrestronDevice(16, dimmer25);
             //   _loadManager.AddLoadsFromCrestronDevice(17, switch21);
             //   _loadManager.AddLoadsFromCrestronDevice(18, switch22);
             //   _loadManager.AddLoadsFromCrestronDevice(19, switch23);

                gateway2.Register();
        }

        public void RegisterScenes()
        {
            Scene scene1 = new Scene(1, "Scene 1");
            Scene scene2 = new Scene(2, "Scene 2");
            Scene scene3 = new Scene(3, "Scene 3");
            Scene scene4 = new Scene(4, "Scene 4");
            Scene scene5 = new Scene(5, "Whole House On");
            Scene scene6 = new Scene(6, "Whole House Off");

            _sceneManager.AddScene(scene1);
            _sceneManager.AddScene(scene2);
            _sceneManager.AddScene(scene3);
            _sceneManager.AddScene(scene4);
            _sceneManager.AddScene(scene5);
            _sceneManager.AddScene(scene6);


        //    LoadState loadState1 = new LoadState(32000, _loadManager.GetLoad(1));
            LoadState loadState2 = new LoadState(32000, _loadManager.GetLoad(2));
        //    LoadState loadState3 = new LoadState(32000, _loadManager.GetLoad(3));
            LoadState loadState4 = new LoadState(32000, _loadManager.GetLoad(4));

            LoadState loadState5 = new LoadState(32000, _loadManager.GetLoad(6));
         //   LoadState loadState6 = new LoadState(32000, _loadManager.GetLoad(7));
            LoadState loadState7 = new LoadState(32000, _loadManager.GetLoad(8));

            LoadState loadState8 = new LoadState(0, _loadManager.GetLoad(6));
          //  LoadState loadState9 = new LoadState(0, _loadManager.GetLoad(7));
            LoadState loadState10 = new LoadState(0, _loadManager.GetLoad(8));


            LoadState loadState11 = new LoadState(65535, _loadManager.GetLoad(6));
          //  LoadState loadState12 = new LoadState(65535, _loadManager.GetLoad(7));
            LoadState loadState13 = new LoadState(65535, _loadManager.GetLoad(8));

          //  LoadState loadState14 = new LoadState(65535, _loadManager.GetLoad(1));
            LoadState loadState15 = new LoadState(65535, _loadManager.GetLoad(2));
         //   LoadState loadState16 = new LoadState(65535, _loadManager.GetLoad(3));
            LoadState loadState17 = new LoadState(65535, _loadManager.GetLoad(4));
          //  LoadState loadState18 = new LoadState(65535, _loadManager.GetLoad(5));
            LoadState loadState19 = new LoadState(65535, _loadManager.GetLoad(6));
          //  LoadState loadState20 = new LoadState(65535, _loadManager.GetLoad(7));
            LoadState loadState21 = new LoadState(65535, _loadManager.GetLoad(8));
          //  LoadState loadState22 = new LoadState(65535, _loadManager.GetLoad(9));
            LoadState loadState23 = new LoadState(65535, _loadManager.GetLoad(10));
            LoadState loadState24 = new LoadState(65535, _loadManager.GetLoad(11));
            LoadState loadState25 = new LoadState(65535, _loadManager.GetLoad(12));
            LoadState loadState26 = new LoadState(65535, _loadManager.GetLoad(13));
            LoadState loadState27 = new LoadState(65535, _loadManager.GetLoad(14));
            LoadState loadState28 = new LoadState(65535, _loadManager.GetLoad(15));
            LoadState loadState29 = new LoadState(65535, _loadManager.GetLoad(16));
          //  LoadState loadState30 = new LoadState(65535, _loadManager.GetLoad(17));
          //  LoadState loadState31 = new LoadState(65535, _loadManager.GetLoad(18));
          //  LoadState loadState32 = new LoadState(65535, _loadManager.GetLoad(19));


          //  LoadState loadState33 = new LoadState(0, _loadManager.GetLoad(1));
            LoadState loadState34 = new LoadState(0, _loadManager.GetLoad(2));
          //  LoadState loadState35 = new LoadState(0, _loadManager.GetLoad(3));
            LoadState loadState36 = new LoadState(0, _loadManager.GetLoad(4));
          //  LoadState loadState37 = new LoadState(0, _loadManager.GetLoad(5));
            LoadState loadState38 = new LoadState(0, _loadManager.GetLoad(6));
          //  LoadState loadState39 = new LoadState(0, _loadManager.GetLoad(7));
            LoadState loadState40 = new LoadState(0, _loadManager.GetLoad(8));
          //  LoadState loadState41 = new LoadState(0, _loadManager.GetLoad(9));
            LoadState loadState42 = new LoadState(0, _loadManager.GetLoad(10));
            LoadState loadState43 = new LoadState(0, _loadManager.GetLoad(11));
            LoadState loadState44 = new LoadState(0, _loadManager.GetLoad(12));
            LoadState loadState45 = new LoadState(0, _loadManager.GetLoad(13));
            LoadState loadState46 = new LoadState(0, _loadManager.GetLoad(14));
            LoadState loadState47 = new LoadState(0, _loadManager.GetLoad(15));
            LoadState loadState48 = new LoadState(0, _loadManager.GetLoad(16));
          //  LoadState loadState49 = new LoadState(0, _loadManager.GetLoad(17));
          //  LoadState loadState50 = new LoadState(0, _loadManager.GetLoad(18));
          //  LoadState loadState51 = new LoadState(0, _loadManager.GetLoad(19));


       //     scene1.AddLoadState(loadState1);
            scene1.AddLoadState(loadState2);
        //    scene1.AddLoadState(loadState3);
            scene1.AddLoadState(loadState4);

            scene2.AddLoadState(loadState5);
         //   scene2.AddLoadState(loadState6);
            scene2.AddLoadState(loadState7);

            scene3.AddLoadState(loadState8);
          //  scene3.AddLoadState(loadState9);
            scene3.AddLoadState(loadState10);

            scene4.AddLoadState(loadState11);
          //  scene4.AddLoadState(loadState12);
            scene4.AddLoadState(loadState13);

          //  scene5.AddLoadState(loadState14);
            scene5.AddLoadState(loadState15);
           // scene5.AddLoadState(loadState16);
            scene5.AddLoadState(loadState17);
         //   scene5.AddLoadState(loadState18);
            scene5.AddLoadState(loadState19);
         //   scene5.AddLoadState(loadState20);
            scene5.AddLoadState(loadState21);
        //    scene5.AddLoadState(loadState22);
            scene5.AddLoadState(loadState23);
            scene5.AddLoadState(loadState24);
            scene5.AddLoadState(loadState25);
            scene5.AddLoadState(loadState26);
            scene5.AddLoadState(loadState27);
            scene5.AddLoadState(loadState28);
            scene5.AddLoadState(loadState29);
        //    scene5.AddLoadState(loadState30);
        //    scene5.AddLoadState(loadState31);
        //    scene5.AddLoadState(loadState32);

        //    scene6.AddLoadState(loadState33);
            scene6.AddLoadState(loadState34);
        //    scene6.AddLoadState(loadState35);
            scene6.AddLoadState(loadState36);
        //    scene6.AddLoadState(loadState37);
            scene6.AddLoadState(loadState38);
      //      scene6.AddLoadState(loadState39);
            scene6.AddLoadState(loadState40);
       //     scene6.AddLoadState(loadState41);
            scene6.AddLoadState(loadState42);
            scene6.AddLoadState(loadState43);
            scene6.AddLoadState(loadState44);
            scene6.AddLoadState(loadState45);
            scene6.AddLoadState(loadState46);
            scene6.AddLoadState(loadState47);
            scene6.AddLoadState(loadState48);
        //    scene6.AddLoadState(loadState49);
         //   scene6.AddLoadState(loadState50);
         //   scene6.AddLoadState(loadState51);




        }


        public void test()
        {
            Assembly myAssembly = Assembly.LoadFrom(@"Crestron.SimplSharpPro.UI.dll");


            CType myType = myAssembly.GetType("Crestron.SimplSharpPro.UI.Tsw1050");
            MethodInfo myMethod1 = myType.GetMethod("Register");


            ConstructorInfo ctor = myType.GetConstructor(new CType[] { typeof(UInt32), typeof(ControlSystem) });
            object myInstance = ctor.Invoke(new object[] { (UInt32)0x03, this });

            if ((eDeviceRegistrationUnRegistrationResponse)myMethod1.Invoke(myInstance, null) != eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Error("Error Registering touchpanel");
            else
                CrestronConsole.PrintLine("Succes");
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