using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.Events
{
    public class SceneAtLevelEvent
    {
        private Scene _scene;
        public bool _atLevel;

        public SceneAtLevelEvent(Scene scene, bool AtLevel)
        {
            this._scene = scene;
            this._atLevel = AtLevel;
        }

        public bool AtLevel
        {
            get
            {
                return _atLevel;
            }

        }

        public Scene Scene
        {
            get
            {
                return _scene;
            }
        }
    }
}