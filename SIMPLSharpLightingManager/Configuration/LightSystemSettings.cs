using System.Collections.Generic;

namespace SIMPLSharpLightingManager.Configuration
{
    public class LightSystemSettings
    {
        public string configFileVersion = "3.0";
        public List<SerializationRoom> rooms = new List<SerializationRoom>();
        public List<SerializationLightLoad> loads = new List<SerializationLightLoad>();
        public List<SerializationScene> scenes = new List<SerializationScene>();
        public string systemName;
    }
}
