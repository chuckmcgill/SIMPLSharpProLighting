using System;
using Crestron.SimplSharpPro.DeviceSupport;

namespace SIMPLSharpLightingManager.UserInterfaces
{
    public interface ITouchPanelView
    {
        event Action<uint> RoomSelected;
        event Action<uint> SceneSelected;
        event Action PreviousPageSelected;
        event Action NextPageSelected;
        event Action<uint> LoadToggleSelected;
        event Action<uint> LoadRaiseSelected;
        event Action<uint> LoadRaiseDeselected;
        event Action<uint> LoadLowerSelected;
        event Action<uint> LoadLowerDeselected;

        void SetLoadType(uint id, eLoadType type);
        void HideLoad(uint id);
        void SetLoadOn(uint id);
        void SetLoadOff(uint id);
        void SetLoadName(uint id, string name);
        void SetLoadLevel(uint id, ushort level);
        void RampLoadToLevel(uint id, ushort level, uint time);
        void RaiseLoad(uint id, uint time);
        void LowerLoad(uint id, uint time);
        void StopRampLoad(uint id);

        void SetSelectedRoom(uint id);
        void SetSelectedRoomName(uint id, string name);
        void SetRoomName(uint id, string name);
        void SetRoomStatus(uint id, string status);
        void SetRoomCount(ushort count);

        void SetSceneOn(uint id);
        void SetSceneOff(uint id);
        void SetSceneName(uint id, string name);
        void SetSceneVisible(uint id, bool visible);
    }
}