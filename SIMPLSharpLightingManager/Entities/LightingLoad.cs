using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Lighting;
using Easy.MessageHub;
using SIMPLSharpLightingManager.Events;

namespace SIMPLSharpLightingManager.Entities
{
    public class LightingLoad
    {
        public int LoadID;

        private LightLoad _crestronLoad;

        //    public eLoadType Type;

        public String _name;

        //private bool _ison = false;

        private LoadState currentState;

        private ushort _level;
        // private ushort _targetLevel;
        private ushort _levelFB;

        double sequence = 0;

        private ushort _startLevel;
        private ushort _endLevel;

        //Raise Lower Rate 3s
        private ushort _raiseLowerRate = 3000;
        //Preset Fade Time 1s
        private ushort _presetFadeTime = 1000;
        //Off Fade Time 1s
        private ushort _offFadeTime = 1000;
        //Dimmer Delayed Off 0s
        private ushort _dimmerDelayedOff = 1000;
        private ushort _fastOnOffTime = 250;









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

        enum LoadState
        {
            Steady,
            Raise,
            StopRaise,
            Lower,
            StopLower,
            Toggle,
            Recall
        }

        private int expectedMessageCount = 0;

        public LightingLoad(int loadID, LightLoad load, String name)
        {

            this.LoadID = loadID;

            _crestronLoad = load;
            _name = name;
            currentState = LoadState.Steady;

            _crestronLoad.DeviceLoadIsOn.LoadStateChange += new LoadEventHandler(load_LoadEventHandler);

            // if (_crestronLoad.Type == eLoadType.Dimmer)
            //     ((AdvDimmer)_crestronLoad.DeviceLoadIsOn).ButtonStateChange += new ButtonEventHandler(load_ButtonEventHandler);


        }

        void load_ButtonEventHandler(GenericBase currentDevice, ButtonEventArgs args)
        {
            CrestronConsole.PrintLine("MessgeID: {0} Device {1}: Button {2}, State {3}", sequence++, currentDevice.ID, args.Button, args.NewButtonState);
        }

        void load_LoadEventHandler(LightingBase currentDevice, LoadEventArgs args)
        {

            //Need to review this code/handler to better support multiple loads on device
            var hub = MessageHub.Instance;

            //if (EventIds.ContainsKey(args.EventId))
            //    CrestronConsole.PrintLine("MessgeID: {0} Device {1}: {2}", sequence++, currentDevice.ID, EventIds[args.EventId]);

            // use this structure to react to the different events
            switch (args.EventId)
            {

                case LoadEventIds.IsOnEventId:
                    {
                        //Shouldn't matter if Switch vs Dimmer
                        if (this.IsOn())
                        {
                            LoadIsOnEvent message = new LoadIsOnEvent(this);
                            hub.Publish<LoadIsOnEvent>(message);
                        }
                        else
                        {
                            LoadIsOffEvent message = new LoadIsOffEvent(this);
                            hub.Publish<LoadIsOffEvent>(message);
                        }
                    }
                    break;
                case LoadEventIds.LevelChangeEventId:
                    {
                        //If Level is/was 65535
                        //You can expect two level events with no prior warning (This means button was pressed);

                        var dimmer = ((ClwDimmingLoad)(args.Load));
                        _levelFB = dimmer.LevelFeedback.UShortValue;

                        if (currentState == LoadState.Raise || currentState == LoadState.Lower ||
                            currentState == LoadState.StopLower || currentState == LoadState.StopRaise)
                        {

                            if (expectedMessageCount == 3)
                            {
                                //Swallow the initial Level Message (for now);
                                _startLevel = _levelFB;
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
                                _endLevel = _levelFB;
                                expectedMessageCount--;
                                _level = dimmer.LevelFeedback.UShortValue;
                                SetLevelFB(_level);
                                currentState = LoadState.Steady;
                                break;
                            }

                        }
                        else if (currentState == LoadState.Toggle)
                        {
                            if (expectedMessageCount == 2)
                            {
                                //Swallow first level
                                _startLevel = _levelFB;
                                expectedMessageCount--;
                                break;
                            }
                            else if (expectedMessageCount == 1)
                            {
                                expectedMessageCount--;
                                _endLevel = _levelFB;
                                _level = _levelFB;
                                SetLevelFB(_level);
                                currentState = LoadState.Steady;
                                break;
                            }
                        }
                        else
                        {
                            if (currentState == LoadState.Steady)
                            {
                                //This only gets called when load is at MAX 65535 and we keep pushing to go max again
                                //This really shouldn't trigger an event (as we already should have this level)
                                //Let's process it anyway
                                _startLevel = _levelFB;
                                currentState = LoadState.Toggle;
                                expectedMessageCount = 1;
                                break;
                            }
                        }
                    }
                    break;
                case LoadEventIds.LevelInputChangedEventId:
                    {

                    }
                    break;
                case LoadEventIds.PresetLoadIsAtEventId:
                    {

                    }
                    break;
                case LoadEventIds.FastOffEventId:
                    { //We must handle this
                        currentState = LoadState.Toggle;
                        expectedMessageCount = 2;

                        LoadOffEvent message = new LoadOffEvent(this);
                        message.Time = _fastOnOffTime;

                        hub.Publish<LoadOffEvent>(message);
                    }
                    break;

                case LoadEventIds.FastFullOnEventId:
                    {
                        //Need to handle this
                        var dimmer = ((ClwDimmingLoad)(args.Load));
                        // var last1Preset = dimmer12.LastPresetCalled();

                        currentState = LoadState.Toggle;
                        expectedMessageCount = 2;

                        LoadOnEvent message = new LoadOnEvent(this);
                        message.Time = _fastOnOffTime;
                        //lpresetMessage.PresetNumber = last1Preset.Number;

                        hub.Publish<LoadOnEvent>(message);
                    }

                    break;
                case LoadEventIds.LastPresetCalledEventId:
                    {
                        //This is preset to go full on
                        //I'm not handling other presets right now
                        //Switch somehow has "PressOn" triggered to preset

                        var dimmer = ((ClwDimmingLoad)(args.Load));
                        var lastPreset = dimmer.LastPresetCalled();

                        currentState = LoadState.Toggle;
                        expectedMessageCount = 2;


                        if (lastPreset != null)
                        {
                            LoadOnEvent message = new LoadOnEvent(this);
                            message.Time = _presetFadeTime;
                            // presetMessage.PresetNumber = lastPreset.Number;

                            hub.Publish<LoadOnEvent>(message);
                        }
                        else
                        {


                        }
                    }
                    break;
                case LoadEventIds.LowerEventId:
                    {
                        //Basic lower

                        if (currentState == LoadState.Lower)
                        {
                            currentState = LoadState.StopLower;
                        }
                        else
                        {
                            currentState = LoadState.Lower;
                            expectedMessageCount = 3;
                        }

                        LoadLowerEvent message = new LoadLowerEvent(this);
                        hub.Publish<LoadLowerEvent>(message);
                    }
                    break;
                case LoadEventIds.RaiseEventId:
                    { //Basic Raise

                        if (currentState == LoadState.Raise)
                        {
                            currentState = LoadState.StopRaise;
                        }
                        else
                        {
                            currentState = LoadState.Raise;
                            expectedMessageCount = 3;
                        }

                        LoadRaiseEvent message = new LoadRaiseEvent(this);
                        hub.Publish<LoadRaiseEvent>(message);
                    }
                    break;
                case LoadEventIds.OffReleaseEventId:
                    {
                        //Off Release
                        //We don't do anything with Off Press?
                        //Holding Off just triggers a lower

                        currentState = LoadState.Toggle;
                        expectedMessageCount = 2;

                        LoadOffEvent message = new LoadOffEvent(this);
                        message.Time = _offFadeTime;

                        hub.Publish<LoadOffEvent>(message);
                    }
                    break;
                default:
                    {

                    }
                    break;
            }

        }

        public ushort Level
        {

            get
            {

                //TODO:  Evaluate if I'm managing internal Level state enough to avoid checking the load
                if (_crestronLoad is SwitchedLoad)
                {
                    if (((SwitchedLoad)_crestronLoad).IsOn)
                        return 65535;
                    else
                        return 0;
                }
                else if (_crestronLoad is DimmingLoad)
                {
                    return _level;
                }
                else
                    return 0;
            }

        }

        public void FullOn()
        {
            //Level Feedback should occur to process even to set _level
            //No need to set it here

            _crestronLoad.FullOn();

            var hub = MessageHub.Instance;

            LoadOnEvent onMessage = new LoadOnEvent(this);
            onMessage.Time = _presetFadeTime;

            hub.Publish<LoadOnEvent>(onMessage);

            //Might be better to call this with a CTimer
            LoadIsOnEvent isOnmessage = new LoadIsOnEvent(this);

            hub.Publish<LoadIsOnEvent>(isOnmessage);



        }

        public void Off()
        {
            //Level Feedback should occur to process even to set _level
            //No need to set it here

            _crestronLoad.FullOff();

            var hub = MessageHub.Instance;

            LoadOffEvent offMessage = new LoadOffEvent(this);
            offMessage.Time = _offFadeTime;


            //Might be better to send this with a CTimer event
            hub.Publish<LoadOffEvent>(offMessage);

            LoadIsOffEvent isOffMessage = new LoadIsOffEvent(this);

            hub.Publish<LoadIsOffEvent>(isOffMessage);


        }

        public void Toggle()
        {

            var _ison = this.IsOn();

            currentState = LoadState.Toggle;
            expectedMessageCount = 2;

            if (!_ison)
            {
                FullOn();
            }
            else if (_ison)
            {
                Off();
            }

        }

        public void Raise()
        {

            if (_crestronLoad.Type == eLoadType.Switch)
            {
                if (this.Level == 0)
                    FullOn();
            }
            else if (_crestronLoad.Type == eLoadType.Dimmer)
            {
                currentState = LoadState.Raise;
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
                currentState = LoadState.StopRaise;

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
                currentState = LoadState.Lower;
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
                currentState = LoadState.StopLower;

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

                currentState = LoadState.Recall;
                ((DimmingLoad)_crestronLoad).Level.UShortValue = level;

                _endLevel = level;


                //Ramp Timer
                //Change state back

                CTimer timer = new CTimer(atLevel, _presetFadeTime);

                var hub = MessageHub.Instance;


                //This should RampToLevel
                LoadRampToLevelEvent message = new LoadRampToLevelEvent(this);
                message.Level = level;
                message.Time = _presetFadeTime;

                hub.Publish<LoadRampToLevelEvent>(message);

                //Testing with multiple loads going on

            }

            return;

        }

        public void atLevel(object callback)
        {
            if (currentState == LoadState.Recall)
            {
                this.currentState = LoadState.Steady;
                _level = _endLevel;

                var hub = MessageHub.Instance;

                LoadLevelEvent lmessage = new LoadLevelEvent(this);
                lmessage.Level = _level;

                hub.Publish<LoadLevelEvent>(lmessage);
            }

        }

        public void SetLevelFB(ushort level)
        {
            var hub = MessageHub.Instance;

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
                    if (currentState == LoadState.Lower || currentState == LoadState.Raise)
                    {
                        return true;
                    }
                    else if (currentState == LoadState.StopLower || currentState == LoadState.StopRaise)
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

        public uint RaiseLowerRate
        {
            get
            {
                return _raiseLowerRate;
            }

        }

        public uint PresetFadeTime
        {

            get
            {
                return _presetFadeTime;
            }

        }

        public uint OffFadeTime
        {
            get
            {

                return _offFadeTime;
            }

        }

        public eLoadType Type
        {

            get
            {

                return _crestronLoad.Type;
            }

        }
    }
}