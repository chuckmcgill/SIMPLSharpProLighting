using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using SIMPLSharpLightingManager.Entities;

namespace SIMPLSharpLightingManager.System
{
    public class SceneManager
    {
        private IList<Scene> _scenes;

        public SceneManager()
        {
            this._scenes = new List<Scene>(100);
        }

        public void AddScene(Scene scene)
        {
            this._scenes.Add(scene);
        }

        public void DeleteScene(int sceneID)
        {
            return;
        }

        public Scene GetScene(int sceneID)
        {
            return this._scenes.First<Scene>(x => x.SceneID == sceneID);
        }

        public IList<Scene> GetScenes()
        {
            return this._scenes;
        }

        public IList<Scene> GetScenesByRoomID(int roomID)
        {
            return this._scenes.Where(x => x.WholeHome == true ||
                                           x.Rooms.Any(y => y.RoomID == roomID)).ToList();
        }

        public IList<Scene> GetWholeHomeScenes()
        {
            return this._scenes.Where(x => x.WholeHome == true).ToList();
        }
    }
}