using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace SIMPLSharpLightingManager.UserInterfaces
{
    public class TouchPanelView : ITouchPanelView
    {

        uint startindex_load_onoff = (uint)31;
        uint startindex_load_raise = (uint)41;
        uint startindex_load_lower = (uint)51;
        uint startindex_load_show_switch = (uint)31;
        uint startindex_load_show_dimmer = (uint)41;
        uint startindex_load_level = (uint)31;
        uint startindex_load_ison = (uint)51;
        uint startindex_load_name = (uint)51;
        uint startindex_room_select = (uint)1;
        uint startindex_room_name = (uint)1;
        uint startindex_scene_recall = (uint)61;
        uint startindex_scene_name = (uint)71;
        uint startindex_scene_show = (uint)71;
        uint startindex_scene_ison = (uint)61;
        uint load_prev_page = (uint)23;
        uint load_next_page = (uint)24;
        uint startindex_room_selected = (uint)1;
        uint startindex_room_selected_name = (uint)41;

        BasicTriList _triList;

        public event Action<uint> RoomSelected;
        public event Action<uint> SceneSelected;
        public event Action PreviousPageSelected;
        public event Action NextPageSelected;
        public event Action<uint> LoadToggleSelected;
        public event Action<uint> LoadRaiseSelected;
        public event Action<uint> LoadRaiseDeselected;
        public event Action<uint> LoadLowerSelected;
        public event Action<uint> LoadLowerDeselected;

        public TouchPanelView(BasicTriList triList)
        {

            _triList = triList;
            _triList.SigChange += EISComm_SigChange;
            _triList.Register();

            BindCallbacks();


        }


        private void BindCallbacks()
        {

            _triList.BooleanOutput[load_prev_page].UserObject = new Action<bool>(x => { if (x) this.OnPreviousPageSelected(); });
            _triList.BooleanOutput[load_next_page].UserObject = new Action<bool>(x => { if (x) this.OnNextPageSelected(); });

            for (uint i = 0; i < 20; i++)
            {
                var index = i;
                _triList.BooleanOutput[startindex_room_select + (uint)index].UserObject = new Action<bool>(x => { if (x) this.OnRoomSelectedIndexChanged(index); });
            }

            for (uint i = 0; i < 10; i++)
            {
                var index = i;
                _triList.BooleanOutput[startindex_scene_recall + (uint)index].UserObject = new Action<bool>(x => { if (x) this.OnSceneSelectedIndexChanged(index); });

            }

            for (uint i = 0; i < 4; i++)
            {
                var index = i;
                //Toggle
                _triList.BooleanOutput[startindex_load_onoff + (uint)index].UserObject = new Action<bool>(x => { if (x) this.OnLoadToggleSelected(index); });

                //Raise
                _triList.BooleanOutput[startindex_load_raise + (uint)index].UserObject = new Action<bool>(x => { if (x) this.OnLoadRaiseSelected(index); else this.OnLoadRaiseDeselected(index); });

                //Lower
                _triList.BooleanOutput[startindex_load_lower + (uint)index].UserObject = new Action<bool>(x => { if (x) this.OnLoadLowerSelected(index); else this.OnLoadLowerDeselected(index); });


            }

        }

        private void OnPreviousPageSelected()
        {

            if (this.PreviousPageSelected != null)
            {
                this.PreviousPageSelected();
            }

        }

        private void OnNextPageSelected()
        {
            if (this.NextPageSelected != null)
            {
                this.NextPageSelected();
            }

        }

        private void OnRoomSelectedIndexChanged(uint index)
        {
            if (this.RoomSelected != null)
            {
                this.RoomSelected(index);
            }

        }

        private void OnSceneSelectedIndexChanged(uint index)
        {
            if (this.SceneSelected != null)
            {
                this.SceneSelected(index);
            }


        }

        private void OnLoadToggleSelected(uint index)
        {
            if (this.LoadToggleSelected != null)
            {
                this.LoadToggleSelected(index);
            }


        }

        private void OnLoadRaiseSelected(uint index)
        {
            if (this.LoadRaiseSelected != null)
            {
                this.LoadRaiseSelected(index);
            }


        }

        private void OnLoadRaiseDeselected(uint index)
        {
            if (this.LoadRaiseDeselected != null)
            {
                this.LoadRaiseDeselected(index);
            }


        }

        private void OnLoadLowerSelected(uint index)
        {
            if (this.LoadLowerSelected != null)
            {
                this.LoadLowerSelected(index);
            }



        }

        private void OnLoadLowerDeselected(uint index)
        {
            if (this.LoadLowerDeselected != null)
            {
                this.LoadLowerDeselected(index);
            }

        }


        public void SetLoadOn(uint loadID)
        {
            _triList.BooleanInput[startindex_load_ison + loadID].BoolValue = true;
        }

        public void SetLoadOff(uint loadID)
        {
            _triList.BooleanInput[startindex_load_ison + loadID].BoolValue = false;
        }

        public void SetLoadName(uint loadID, string name)
        {
            _triList.StringInput[startindex_load_name + loadID].StringValue = name;
        }

        public void SetLoadLevel(uint loadID, ushort level)
        {
            _triList.UShortInput[startindex_load_level + loadID].UShortValue = level;
        }

        public void HideLoad(uint loadID)
        {
            _triList.BooleanInput[startindex_load_show_dimmer + (uint)loadID].BoolValue = false;
            _triList.BooleanInput[startindex_load_show_switch + (uint)loadID].BoolValue = false;
        }

        public void SetLoadType(uint loadID, eLoadType type)
        {
            if (type == eLoadType.Dimmer)
            {
                _triList.BooleanInput[startindex_load_show_dimmer + (uint)loadID].BoolValue = true;
                _triList.BooleanInput[startindex_load_show_switch + (uint)loadID].BoolValue = false;

            }
            else if (type == eLoadType.Switch)
            {

                _triList.BooleanInput[startindex_load_show_switch + (uint)loadID].BoolValue = true;
                _triList.BooleanInput[startindex_load_show_dimmer + (uint)loadID].BoolValue = false;

            }

        }

        public void RaiseLoad(uint loadID, uint time)
        {
            this.RampLoadToLevel(loadID, 65535, time);
        }

        public void LowerLoad(uint loadID, uint time)
        {
            this.RampLoadToLevel(loadID, 0, time);
        }

        public void RampLoadToLevel(uint loadID, ushort level, uint time)
        {
            ushort startLevel = _triList.UShortInput[startindex_load_level + loadID].UShortValue;
            ushort endLevel = level;

            _triList.UShortInput[startindex_load_level + loadID].CreateRamp(endLevel, CalculateRampTime(startLevel, endLevel, time / 10));

        }

        public void StopRampLoad(uint loadID)
        {
            _triList.UShortInput[startindex_load_level + loadID].StopRamp();
        }

        public void SetRoomName(uint roomID, string name)
        {

            _triList.StringInput[startindex_room_name + roomID].StringValue = name;

        }

        public void SetRoomStatus(uint roomID, string status)
        {

            //_EIScom.StringInput[startindex_room_name + (uint)index].StringValue = _roomManager.GetRooms()[index].Name;

        }

        public void SetRoomCount(ushort roomCount)
        {
            _triList.UShortInput[(uint)5].UShortValue = roomCount;
        }

        public void SetSceneName(uint sceneID, string name)
        {

            _triList.StringInput[startindex_scene_name + sceneID].StringValue = name;

        }

        public void SetSceneVisible(uint sceneID, bool visible)
        {

            _triList.BooleanInput[startindex_scene_show + sceneID].BoolValue = visible;

        }

        public void SetSceneOn(uint sceneID)
        {

            _triList.BooleanInput[startindex_scene_ison + (uint)sceneID].BoolValue = true;

        }


        public void SetSceneOff(uint sceneID)
        {

            _triList.BooleanInput[startindex_scene_ison + (uint)sceneID].BoolValue = false;

        }

        public void SetSelectedRoom(uint roomID)
        {
            for (int i = 0; i <= 20; i++)
            {
                if (i == (roomID - 1))
                {
                    _triList.BooleanInput[startindex_room_selected + (uint)roomID].BoolValue = true;
                }
                else
                {
                    _triList.BooleanInput[startindex_room_selected + (uint)roomID].BoolValue = false;
                }
            }
        }

        public void SetSelectedRoomName(uint roomID, string name)
        {
            _triList.StringInput[startindex_room_selected_name].StringValue = name;

        }

        void EISComm_SigChange(GenericBase currentDevice, SigEventArgs args)
        {
            var sig = args.Sig;
            var uo = sig.UserObject;

            if (uo is Action<bool>)                             // If the userobject for this signal with boolean
            {
                (uo as Action<bool>)(sig.BoolValue);     // cast this signal's userobject as delegate Action<bool>
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
        }

        public uint CalculateRampTime(ushort startLevel, ushort endLevel, uint rampTime)
        {
            var tick = 65535 / rampTime;


            if (endLevel > startLevel)
            {
                return ((ushort)(endLevel - startLevel)) / tick;

            }
            else
            {
                return (startLevel) / tick;

            }

        }

    }
}