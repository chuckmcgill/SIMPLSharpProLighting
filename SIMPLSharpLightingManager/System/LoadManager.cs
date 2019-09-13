using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Lighting;
using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.System
{
    public class LoadManager
    {
        private IList<LightingLoad> _loads;



        public LoadManager()
        {
            this._loads = new List<LightingLoad>(100);
        }

        public void AddLoadsFromCrestronDevice(int LoadID, Dimmer _dimmer)
        {

            foreach (LightLoad load in _dimmer.DimmingLoads)
            {
                LightingLoad newLoad = new LightingLoad(LoadID, load, _dimmer.Description);
                this._loads.Add(newLoad);
            }

        }

        public void AddLoadsFromCrestronLoad(int LoadID, string name, LightLoad load)
        {
            LightingLoad newLoad = new LightingLoad(LoadID, load, name);
            this._loads.Add(newLoad);
        }

        public void AddLoadsFromCrestronDevice(int LoadID, Switch _switch)
        {

            foreach (LightLoad load in _switch.SwitchedLoads)
            {
                LightingLoad newLoad = new LightingLoad(LoadID, load, _switch.Description);
                this._loads.Add(newLoad);
            }

        }



        public void AddLoad(LightingLoad _load)
        {

            this._loads.Add(_load);

        }

        public void RemoveLoad(int loadID)
        {
            return;
        }

        public LightingLoad GetLoad(int loadID)
        {
            return this._loads.First<LightingLoad>(x => x.LoadID == loadID);
        }

        public IList<LightingLoad> GetLoads()
        {
            return this._loads;
        }

    }
}