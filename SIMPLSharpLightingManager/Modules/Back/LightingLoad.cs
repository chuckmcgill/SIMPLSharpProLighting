using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Lighting;
using Crestron.SimplSharpPro;
using Easy.MessageHub;
using SIMPLSharpLightingManager.MessageEvents;

namespace SIMPLSharpLightingManager.Modules
{
    public class LightingLoad
    {
        public int LoadID;

        private LightLoad _crestronLoad;

        public eLoadType Type;

        //private ushort levelFB = 0;

        public String _name;

        //private bool _ison = false;

        //private bool _isRamping = false;

        private loadState currentState;

        private ushort _level;

        private ushort _levelFB;

        Stopwatch priorWatch;

        double sequence = 0;

        

       // public Action<Load,EventArgs> OnChange { get; set; }

        Dictionary<int, string> EventIds = new Dictionary<int, string>()
		{
			{1, "LowerEventId"},
			{2, "RaiseEventId"},
			{3, "OnPressEventId"},
			{4, "OnReleaseEventId"},
			{5, "OffPressEventId"},
			{6, "OffReleaseEventId"},
			{7, "LevelChangeEventId"},
			{8, "FullOnPressEventId"},
			{9, "FullOnReleaseEventId"},
			{10, "FastOffEventId"},
			{11, "FastFullOnEventId"},
			{12, "DelayedOffEventId"},
			{13, "IsOnEventId"},
			{14, "IsFullOnEventId"},
			{15, "PresetLoadIsAtEventId"},
			{16, "LastPresetCalledEventId"},
			{17, "NonDimmingFeedbackEventId"},
			{18, "LevelInputChangedEventId"},
			{19, "LoadAttachedEventId"},
			{20, "ToggleEventId"}
		};

        enum loadState
        {
            Steady,
            Raise,
            StopRaise,
            Lower,
            StopLower,
            Toggle
        }

        private int expectedMessageCount = 0;



        public LightingLoad(int LoadID, LightLoad Load, String Name)
        {
            this.LoadID = LoadID;
            _crestronLoad = Load;
            _name = Name;
            currentState = loadState.Steady;

            if (_crestronLoad.Type == eLoadType.Dimmer)
                ((AdvDimmer)Load.DeviceLoadIsOn).LoadStateChange += new LoadEventHandler(load_LoadEventHandler);
            
            else
                Load.DeviceLoadIsOn.LoadStateChange += new LoadEventHandler(load_LoadEventHandler);

           // Load.DeviceLoadIsOn.
        }

        void load_LoadEventHandler(LightingBase currentDevice, LoadEventArgs args)
        {
            var hub = MessageHub.Instance;

           if (EventIds.ContainsKey(args.EventId))
               CrestronConsole.PrintLine("MessgeID: {0} Device {1}: {2}", sequence++, currentDevice.ID, EventIds[args.EventId]);

            // use this structure to react to the different events
            switch (args.EventId)
            {
                case LoadEventIds.IsOnEventId:
                    if (args.Load is SwitchedLoad)
                    {

                       // this._ison = ((SwitchedLoad)(args.Load)).IsOn;

                        LoadOnEvent message = new LoadOnEvent(this);
                        message.IsOn = this.IsOn();

                        hub.Publish<LoadOnEvent>(message);
                    
                    }
                    else if (args.Load is BasicDimmingLoad)
                    {
                        LoadOnEvent mess = new LoadOnEvent(this);
                        mess.IsOn = this.IsOn();

                        hub.Publish<LoadOnEvent>(mess);  
                        break;
                    }
                 
                    break;

                case LoadEventIds.LevelChangeEventId:

                    var dimmer = ((ClwDimmingLoad)(args.Load));
                    _levelFB = dimmer.LevelFeedback.UShortValue;

                    CrestronConsole.PrintLine("Processing Ramp Level Event: {0}", _levelFB);

                    if (currentState == loadState.Raise || currentState == loadState.Lower ||
                        currentState == loadState.StopLower || currentState == loadState.StopRaise)
                    {                              

                        if (expectedMessageCount == 3)
                        {
                            CrestronConsole.PrintLine("Start Ramp Level: {0}", _levelFB);
                            //Swallow the initial Level Message (for now);
                            expectedMessageCount--;
                            break;

                        }
                        else if (expectedMessageCount == 2)
                        {
                            expectedMessageCount--;
                            break;
                            //Next to last message (pulse)

                        }
                        else if (expectedMessageCount == 1)
                        {
                            //Final state (store in Level);
                            CrestronConsole.PrintLine("End Ramp Level: {0}", _levelFB);
                            expectedMessageCount--;
                            _level = dimmer.LevelFeedback.UShortValue;
                            SetLevelFB(_level);
                            currentState = loadState.Steady;
                            break;
                        }

                    }
                    else if (currentState == loadState.Toggle)
                    {
                        if (expectedMessageCount == 2)
                        {
                            //Swallow first level
                            expectedMessageCount--;
                            break;

                        } 
                        else if (expectedMessageCount == 1)
                        {
                            CrestronConsole.PrintLine("End State based on Toggle: {0}", _levelFB);
                            expectedMessageCount--;
                            _level = dimmer.LevelFeedback.UShortValue;
                            SetLevelFB(_level);
                            currentState = loadState.Steady;
                            break;
                        }
                    }
                    else
                    {
                        if (currentState == loadState.Steady)
                        {
                            CrestronConsole.PrintLine("Toggle state encountered: {0}", _levelFB);
                            currentState = loadState.Toggle;
                            expectedMessageCount = 1;
                            break;
                        }

                    }
                
                    break;
                case LoadEventIds.LevelInputChangedEventId:

                    break;
                case LoadEventIds.PresetLoadIsAtEventId: 

                    break;
                case LoadEventIds.LastPresetCalledEventId:

                    var dimmer2 = ((ClwDimmingLoad)(args.Load));
                    var lastPreset = dimmer2.LastPresetCalled();


                    if (lastPreset != null)
                    {
                        LoadPresetCalledEvent presetMessage = new LoadPresetCalledEvent(this);
                        presetMessage.PresetNumber = lastPreset.Number;

                        hub.Publish<LoadPresetCalledEvent>(presetMessage);
                    }
                    else
                    {


                    }

                    break;
                case LoadEventIds.LowerEventId:

                    if (currentState == loadState.Lower)
                    {
                        CrestronConsole.PrintLine("I was lowering, stopping lower");
                        currentState = loadState.StopLower;
                    }
                    else
                    {
                        CrestronConsole.PrintLine("In default Lower block");
                        currentState = loadState.Lower;
                        expectedMessageCount = 3;
                    }

                    LoadLowerEvent lowerMessage = new LoadLowerEvent(this);
                    hub.Publish<LoadLowerEvent>(lowerMessage);

                    break;
                case LoadEventIds.RaiseEventId:

                    if (currentState == loadState.Raise)
                    {
                        CrestronConsole.PrintLine("I was raising, stopping raise");
                        currentState = loadState.StopRaise;
                    }
                    else
                    {
                        CrestronConsole.PrintLine("In default Raise block");
                        currentState = loadState.Raise;
                        expectedMessageCount = 3;
                    }

                    LoadRaiseEvent raiseMessage = new LoadRaiseEvent(this);
                    hub.Publish<LoadRaiseEvent>(raiseMessage);

                    break;
                case LoadEventIds.OffReleaseEventId:

                    currentState = loadState.Toggle;
                    expectedMessageCount = 2;

                    LoadButtonOffReleaseEvent offReleaseMessage = new LoadButtonOffReleaseEvent(this);
                    hub.Publish<LoadButtonOffReleaseEvent>(offReleaseMessage);
                    break;
                default:
                    break;
            }

        }

        public ushort Level

        {

            get
            {

                if (_crestronLoad is SwitchedLoad)
                {
                    if (((SwitchedLoad)_crestronLoad).IsOn)
                        return 65535;
                    else
                        return 0;
                }
                else if (_crestronLoad is DimmingLoad)
                {
                //    var dimmer = ((BasicDimmingLoad)_crestronLoad);
                    
                    //Level Feedback is the appropriate value to return for the "Level"
                    //return dimmer.LevelFeedback.UShortValue;
                    return _level;
                }
                else
                    return 0;
            }

        }

        public void FullOn()
        {
          
            //Set Level 100
        //  level = 100;
          _crestronLoad.FullOn();
          _level = 65535;

          var hub = MessageHub.Instance;

          LoadToggleOnEvent message = new LoadToggleOnEvent(this);
          message.IsOn = true;

          hub.Publish<LoadToggleOnEvent>(message);

         

        }

        public void Off()
        {
           // level = 0;
            _crestronLoad.FullOff();
            _level = 0;

            var hub = MessageHub.Instance;

            LoadToggleOnEvent message = new LoadToggleOnEvent(this);
            message.IsOn = false;

            hub.Publish<LoadToggleOnEvent>(message);
        }

        public void Toggle()
        {

            CrestronConsole.PrintLine("Toggle is called");

            var _ison = this.IsOn();

            currentState = loadState.Toggle;
            expectedMessageCount = 2;

            if (!_ison)
            {
                FullOn();
            }
            else if (_ison)
            {
                Off();
            }

            var hub = MessageHub.Instance;
            LoadToggleOnEvent message = new LoadToggleOnEvent(this);
            message.IsOn = !_ison;

            hub.Publish<LoadToggleOnEvent>(message);

        }

        public void Raise()
        {
            
             if (_crestronLoad.Type == eLoadType.Switch)
                {
                    if (this.Level == 0)
                        FullOn();
                } else if (_crestronLoad.Type == eLoadType.Dimmer)
                {
                    currentState = loadState.Raise;
                    expectedMessageCount = 3;

                    var messageHub = MessageHub.Instance;
                    LoadRaiseEvent raiseMessage = new LoadRaiseEvent(this);
                    messageHub.Publish<LoadRaiseEvent>(raiseMessage);
                    
                    ((BasicDimmingLoad)_crestronLoad).Raise.BoolValue = true;
                }

        }

        public void StopRaise()
        {
            if (_crestronLoad.Type == eLoadType.Dimmer)
            {
                currentState = loadState.StopRaise;

                var messageHub = MessageHub.Instance;
                LoadRaiseEvent raiseMessage = new LoadRaiseEvent(this);
                messageHub.Publish<LoadRaiseEvent>(raiseMessage);

                ((BasicDimmingLoad)_crestronLoad).Raise.BoolValue = false;



            }

        }

        public void Lower()
        {
            if (_crestronLoad.Type == eLoadType.Switch)
            {
                if (this.Level > 0)
                    Off();
            }
            else if (_crestronLoad.Type == eLoadType.Dimmer)
            {
                currentState = loadState.Lower;
                expectedMessageCount = 3;

                var messageHub = MessageHub.Instance;
                LoadLowerEvent lowerMessage = new LoadLowerEvent(this);
                messageHub.Publish<LoadLowerEvent>(lowerMessage);

                ((BasicDimmingLoad)_crestronLoad).Lower.BoolValue = true;

                
            }
        }

        public void StopLower()
        {
            if (_crestronLoad.Type == eLoadType.Dimmer)
            {
                currentState = loadState.StopLower;

                var messageHub = MessageHub.Instance;
                LoadLowerEvent lowerMessage = new LoadLowerEvent(this);
                messageHub.Publish<LoadLowerEvent>(lowerMessage);

                ((BasicDimmingLoad)_crestronLoad).Lower.BoolValue = false;


            }
        }

        public bool IsOn()
        {
            if (_crestronLoad is SwitchedLoad)
            {
                var dswitch = ((SwitchedLoad)_crestronLoad);

                return dswitch.IsOn;

            }
            else if (_crestronLoad is DimmingLoad)
            {
                var dimmer = ((BasicDimmingLoad)_crestronLoad);
                
                return dimmer.IsOn;

            }
            else
                return false;
        }

        public void SetLevel(ushort level)
        {
         
            if (_crestronLoad.Type == eLoadType.Switch)
            {
                if (level > 0)
                    FullOn();
                else
                    Off();
            }
            else if (_crestronLoad.Type == eLoadType.Dimmer)
            {
                ((DimmingLoad)_crestronLoad).Level.UShortValue = level;
                _level = level;

                  var hub = MessageHub.Instance;

                  LoadToggleOnEvent message = new LoadToggleOnEvent(this);
                  if (level != 0)
                      message.IsOn = true;
                  else
                      message.IsOn = false;

                  message.stopwatch.Start();
                  hub.Publish<LoadToggleOnEvent>(message);

                //Testing with multiple loads going on

                 LoadLevelEvent lmessage = new LoadLevelEvent(this);
                 lmessage.Level = level;

                 hub.Publish<LoadLevelEvent>(lmessage);



            }

            return;

        }

        public void SetLevelFB(ushort level)
        {
            var hub = MessageHub.Instance;

            LoadOnEvent loadOnMessage = new LoadOnEvent(this);
            loadOnMessage.IsOn = this.IsOn();
          
            hub.Publish<LoadOnEvent>(loadOnMessage);
       
            LoadLevelEvent message = new LoadLevelEvent(this);
            message.Level = level;

            _levelFB = level;

            hub.Publish<LoadLevelEvent>(message);
            
        }

        public bool IsRamping
        {
            get
            {
                if (_crestronLoad.Type == eLoadType.Dimmer)
                {
                    if (currentState == loadState.Lower || currentState == loadState.Raise)
                    {                 
                        return true;
                    }
                    else if (currentState == loadState.StopLower || currentState == loadState.StopRaise)
                    {
                        return false;
                    }
                    else if (((BasicDimmingLoad)_crestronLoad).RaiseFeedback)
                    {
                        return true;
                    }
                    else if (((BasicDimmingLoad)_crestronLoad).LowerFeedback)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }

        }

        public String Name
        {
            get
            {
                return _name;
            }
        }
    }
}