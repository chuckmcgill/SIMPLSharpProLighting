using System.Collections.Generic;

namespace SIMPLSharpLightingManager.Configuration
{
    public class LightDeviceSettings
    {
        public string configFileVersion = "3.0";
        public List<GatewayDeviceInformation> gateways = new List<GatewayDeviceInformation>();
        public List<LightingDeviceInformation> devices = new List<LightingDeviceInformation>();
        public List<TouchPanelDeviceInformation> touchpanels = new List<TouchPanelDeviceInformation>();
        public string systemName;
    }
}
