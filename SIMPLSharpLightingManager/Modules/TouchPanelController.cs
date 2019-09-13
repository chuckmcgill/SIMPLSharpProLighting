using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace SIMPLSharpLightingManager.Modules
{
    public class TouchPanelController
    {
        private EthernetIntersystemCommunications _EIScom;

        public TouchPanelController(CrestronControlSystem controlSystem)
        {
            _EIScom = new EthernetIntersystemCommunications(0x09, "127.0.0.2", controlSystem);
            _EIScom.SigChange += EISComm_SigChange;


        }
        /*
        private void EISComm_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
           //args.

        }*/

        public void InitializeStuff()
        {
            _EIScom.BooleanInput[5].UserObject = new System.Action<bool>(b => this.Test(b));

        }

        public void Test(bool value)
        {

        }

        void EISComm_SigChange(GenericBase currentDevice, SigEventArgs args)
        {
            var sig = args.Sig;
            var uo = sig.UserObject;

            CrestronConsole.PrintLine("Event sig: {0}, Type: {1}", sig.Number, sig.GetType());

            if (uo is Action<bool>)                             // If the userobject for this signal with boolean
            {
                (uo as System.Action<bool>)(sig.BoolValue);     // cast this signal's userobject as delegate Action<bool>
                // passing one parm - the value of the bool
            }
            else if (uo is Action<ushort>)
            {
                (uo as Action<ushort>)(sig.UShortValue);
            }
            else if (uo is Action<string>)
            {
                (uo as Action<string>)(sig.StringValue);
            }
        }//UI event handler

    }
}