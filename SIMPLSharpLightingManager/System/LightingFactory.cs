using SIMPLSharpLightingManager.Configuration;

namespace SIMPLSharpLightingManager.System
{
    public class LightingFactory
    {

        public ILightingSystem createLightingSystem(ControlSystem system)
        {
            return new LightingSystem(system, new SettingsManager(), new RoomManager(), new SceneManager(), new LoadManager(), new TouchPanelManager());
        }


    }
}