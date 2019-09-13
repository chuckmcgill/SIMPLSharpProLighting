
namespace SIMPLSharpLightingManager.Entities
{
    public class LoadState
    {
        private ushort _savedLevel;
        private short _currentLevel;
        public LightingLoad _load;
        private static uint _rampTime = 10; //1 Second Ramp

        public LoadState(ushort level, LightingLoad load)
        {
            this._savedLevel = level;
            this._load = load;

        }

        /*
        void lightLoad_LoadStateChange(LightingBase lightingObject, LoadEventArgs args)
        {
            LightLoad eventLoad = args.Load;
 
            switch (args.EventId)
            {
                case LoadEventIds.IsOnEventId:
                    if (eventLoad.Type == eLoadType.Switch)
                    {
                        if (((SwitchedLoad)eventLoad).IsOn)
                            _currentLevel = 100;
                        else if (!((SwitchedLoad)eventLoad).IsOn)
                        {
                            _currentLevel = 0;
                        }
                    }
                    break;
    
                case LoadEventIds.LevelChangeEventId:
                    if(eventLoad.Type == eLoadType.Dimmer)
                        _currentLevel = ((DimmingLoad)eventLoad).LevelFeedback.ShortValue;
                    break;

                default:
                    break;
            }

        }*/


        public void Recall()
        {
            _load.SetLevel(_savedLevel);
        }

        public bool AtLevel()
        {
            return _savedLevel == _load.Level;
        }

    }
}