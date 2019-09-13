using System;

namespace SIMPLSharpLightingManager.Modules
{
    public class UserSig
    {
        eUserSigType _type;
        string _name;
        uint _number;

        Nullable<bool> _booleanvalue;

        Nullable<ushort> _ushortvalue;

        string _stringvalue;

        Object _userObject;




        public UserSig(uint paramNumber, string paramName, bool? paramBooleanValue)
        {
            this._number = paramNumber;
            this._name = paramName;
            this._booleanvalue = paramBooleanValue;
            this._type = eUserSigType.Digital;
        }

        public UserSig(uint paramNumber, string paramName, ushort? paramUintValue)
        {
            this._number = paramNumber;
            this._name = paramName;
            this._ushortvalue = paramUintValue;
            this._type = eUserSigType.Analog;
        }

        public UserSig(uint paramNumber, string paramName, string paramStringValue)
        {
            this._number = paramNumber;
            this._name = paramName;
            this._stringvalue = paramStringValue;
            this._type = eUserSigType.Serial;
        }

        public eUserSigType Type
        {
            get { return _type; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public uint Number
        {
            get { return _number; }
        }

        public Nullable<bool> BoolValue
        {
            get { return _booleanvalue; }
            set { _booleanvalue = value; }
        }


        public Nullable<ushort> UShortValue
        {
            get { return _ushortvalue; }
            set { _ushortvalue = value; }
        }


        public string StringValue
        {
            get { return _stringvalue; }
            set { _stringvalue = value; }
        }

        public Object UserObject
        {
            get { return _userObject; }
            set { _userObject = value; }
        }

    }
}