using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp;

namespace SIMPLSharpLightingManager.Modules
{
    public class XSIG
    {
        Dictionary<int, UserSig> _values;

        public Dictionary<int, UserSig> Values
        {
            get { return _values; }
            set { _values = value; }
        }

        //DIGITAL ENCODING (16-Bit)
        //1 0 C # # # # #    0 # # # # # # #
        //    C     =   COMPLEMENT
        //    C     =   HIGH = 0, LOW = 1
        //    #     =   12-Bit Signal


        // ANALOG ENCODING (32-Bit)
        //1 1 A A 0 # # #    0 # # # # # # #
        //0 A A A A A A A    0 A A A A A A A
        //    A     =    16-Bit Analog
        //    #     =    10-Bit Signal

        //SERIAL ENCODING (Variable)
        //1 1 0 0 1 # # #    0 # # # # # # #
        //d d d d d d d d    . . . . . . . .
        //1 1 1 1 1 1 1 1
        //    #     =    10-Bit Signal
        //    Maximum 252 bytes of serial data

        public XSIG(Dictionary<int, UserSig> values)
        {
            this._values = values;
        }

        public void ProcessString(String rawString)
        {

            byte[] newbytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(rawString);
            int index = 0;


            while (index <= (newbytes.Length - 1))
            {

                BitArray ba = new BitArray(new byte[] { newbytes[index] });

                if (ba[7] == true && ba[6] == false)
                {
                    // CrestronConsole.PrintLine("Found Digital");
                    byte[] array = new byte[] { newbytes[index], newbytes[index + 1] };
                    ProcessDigital(new BitArray(array));
                    index = index + 1;
                }
                else if (ba[7] == true && ba[6] == true && ba[3] == false)
                {
                    // CrestronConsole.PrintLine("Found Analog");
                    byte[] array = new byte[] { newbytes[index], newbytes[index + 1], newbytes[index + 2], newbytes[index + 3] };
                    ProcessAnalog(new BitArray(array));
                    index = index + 3;
                }
                else if (ba[7] == true && ba[6] == true && ba[3] == true)
                {

                    //  CrestronConsole.PrintLine("Found Serial");
                    //Serial
                    bool found = false;
                    int startindex = index;
                    byte footer = 0xFF;

                    while (!found)
                    {

                        index++;

                        if (index > (newbytes.Length - 1))
                        {

                            CrestronConsole.PrintLine("\r\nDidn't find footer, giving up!");
                            break;
                        }

                        if (newbytes[index] == footer)
                        {
                            //  CrestronConsole.PrintLine("Found footer!");
                            found = true;
                            byte[] serials = new byte[index - startindex];
                            Array.Copy(newbytes, startindex, serials, 0, index - startindex);
                            ProcessSerial(new BitArray(serials));
                        }

                    }
                }
                else
                {
                    //Either invalid data or I'm processing in the wrong order
                    // throw new Exception("Danger Danger!");
                }

                index++;
            }



        }

        public void ProcessDigital(BitArray ba)
        {
            int id;
            bool value;

            BitArray bid = new BitArray(12);
            bid[11] = ba[4];
            bid[10] = ba[3];
            bid[9] = ba[2];
            bid[8] = ba[1];
            bid[7] = ba[0];
            bid[6] = ba[14];
            bid[5] = ba[13];
            bid[4] = ba[12];
            bid[3] = ba[11];
            bid[2] = ba[10];
            bid[1] = ba[9];
            bid[0] = ba[8];

            value = !ba[5];

            id = getIntFromBitArray(bid);

            //CrestronConsole.PrintLine("Received Digital - ID: " + id + " Value: " + value);
            if (_values.ContainsKey(id))
            {
                if (_values[id].Type == eUserSigType.Digital)
                {
                    _values[id].BoolValue = value;
                }
                else
                {
                    CrestronConsole.PrintLine("Type mismatch!");
                }
            }
            else
            {
                CrestronConsole.PrintLine("Unable to find ID:" + id + " when processing digital");
            }



        }

        public void ProcessAnalog(BitArray ba)
        {
            //  CrestronConsole.Print("Processing Analog");
            byte[] myarray = new byte[(ba.Length / 8)];
            ba.CopyTo(myarray, 0);
            //   CrestronConsole.PrintLine("Processing Message: " + BitConverter.ToString(myarray));
            //  CrestronConsole.PrintLine(ba.ToString());


            int id;
            ushort value;

            BitArray bid = new BitArray(10);
            bid[9] = ba[2];
            bid[8] = ba[1];
            bid[7] = ba[0];
            bid[6] = ba[14];
            bid[5] = ba[13];
            bid[4] = ba[12];
            bid[3] = ba[11];
            bid[2] = ba[10];
            bid[1] = ba[9];
            bid[0] = ba[8];

            BitArray bvalue = new BitArray(16);

            bvalue[15] = ba[5];
            bvalue[14] = ba[4];
            bvalue[13] = ba[22];
            bvalue[12] = ba[21];
            bvalue[11] = ba[20];
            bvalue[10] = ba[19];
            bvalue[9] = ba[18];
            bvalue[8] = ba[17];
            bvalue[7] = ba[16];
            bvalue[6] = ba[30];
            bvalue[5] = ba[29];
            bvalue[4] = ba[28];
            bvalue[3] = ba[27];
            bvalue[2] = ba[26];
            bvalue[1] = ba[25];
            bvalue[0] = ba[24];

            id = getIntFromBitArray(bid);


            byte[] array = new byte[2];
            bvalue.CopyTo(array, 0);
            value = BitConverter.ToUInt16(array, 0);

            // CrestronConsole.PrintLine("Received Analog - ID: " + id + " Value: " + value);

            if (_values.ContainsKey(id))
            {
                if (_values[id].Type == eUserSigType.Analog)
                {
                    _values[id].UShortValue = value;
                }
                else
                {
                    CrestronConsole.PrintLine("Type mismatch!");
                }
            }
            else
            {
                CrestronConsole.PrintLine("Unable to find ID:" + id + " when processing Analog");
            }
        }

        public void ProcessSerial(BitArray ba)
        {
            //CrestronConsole.Print("Processing String");
            byte[] myarray = new byte[(ba.Length / 8)];
            ba.CopyTo(myarray, 0);
            // CrestronConsole.PrintLine("Processing Message: " + BitConverter.ToString(myarray));
            //CrestronConsole.PrintLine(Encoding.Default.);

            int id;
            string value;

            BitArray bid = new BitArray(10);

            bid[9] = ba[2];
            bid[8] = ba[1];
            bid[7] = ba[0];
            bid[6] = ba[14];
            bid[5] = ba[13];
            bid[4] = ba[12];
            bid[3] = ba[11];
            bid[2] = ba[10];
            bid[1] = ba[9];
            bid[0] = ba[8];

            id = getIntFromBitArray(bid);

            byte[] bvalue = new byte[(ba.Length / 8)];
            ba.CopyTo(bvalue, 0);
            value = Encoding.GetEncoding("ISO-8859-1").GetString(bvalue, 2, bvalue.Length - 2);

            // value = BitConverter.ToString(bvalue, 0, bvalue.Length - 1);

            // CrestronConsole.PrintLine("Received Serial - ID: " + id + " Value is: " + value);

            if (_values.ContainsKey(id))
            {
                if (_values[id].Type == eUserSigType.Serial)
                {
                    _values[id].StringValue = value;
                }
                else
                {
                    CrestronConsole.PrintLine("Type mismatch!");
                }
            }
            else
            {
                CrestronConsole.PrintLine("Unable to find ID:" + id + " when processing serial");
            }

        }

        public string XSig_Digital(int id, bool state)
        {
            BitArray ba = new BitArray(16, false);
            BitArray bid = new BitArray(new int[] { id - 1 });

            ba[7] = true;
            ba[6] = false;
            ba[5] = !state;
            ba[4] = bid[11];
            ba[3] = bid[10];
            ba[2] = bid[9];
            ba[1] = bid[8];
            ba[0] = bid[7];


            ba[15] = false;
            ba[14] = bid[6];
            ba[13] = bid[5];
            ba[12] = bid[4];
            ba[11] = bid[3];
            ba[10] = bid[2];
            ba[9] = bid[1];
            ba[8] = bid[0];


            byte[] newbytes = new byte[2];
            ba.CopyTo(newbytes, 0);

            return Encoding.GetEncoding("ISO-8859-1").GetString(newbytes, 0, newbytes.Length);
        }

        public string XSig_Analog(int id, UInt32 value)
        {
            BitArray ba = new BitArray(32, false);
            BitArray bid = new BitArray(new int[] { id - 1 });
            BitArray bvalue = new BitArray(BitConverter.GetBytes(value));

            ba[7] = true;
            ba[6] = true;
            ba[5] = bvalue[15];
            ba[4] = bvalue[14];
            ba[3] = false;
            ba[2] = bid[9];
            ba[1] = bid[8];
            ba[0] = bid[7];

            ba[15] = false;
            ba[14] = bid[6];
            ba[13] = bid[5];
            ba[12] = bid[4];
            ba[11] = bid[3];
            ba[10] = bid[2];
            ba[9] = bid[1];
            ba[8] = bid[0];

            ba[23] = false;
            ba[22] = bvalue[13];
            ba[21] = bvalue[12];
            ba[20] = bvalue[11];
            ba[19] = bvalue[10];
            ba[18] = bvalue[9];
            ba[17] = bvalue[8];
            ba[16] = bvalue[7];

            ba[31] = false;
            ba[30] = bvalue[6];
            ba[29] = bvalue[5];
            ba[28] = bvalue[4];
            ba[27] = bvalue[3];
            ba[26] = bvalue[2];
            ba[25] = bvalue[1];
            ba[24] = bvalue[0];


            byte[] newbytes = new byte[4];
            ba.CopyTo(newbytes, 0);


            return Encoding.GetEncoding("ISO-8859-1").GetString(newbytes, 0, newbytes.Length);
        }

        public string XSig_Serial(int id, String value)
        {
            BitArray bheader = new BitArray(16, false);
            BitArray bid = new BitArray(new int[] { id - 1 });
            BitArray bdata = new BitArray(Encoding.GetEncoding("ISO-8859-1").GetBytes(value));
            BitArray bfooter = new BitArray(8, true);

            bheader[15] = true;
            bheader[14] = true;
            bheader[13] = false;
            bheader[12] = false;
            bheader[11] = true;
            bheader[10] = bid[9];
            bheader[9] = bid[8];
            bheader[8] = bid[7];
            bheader[7] = false;
            bheader[6] = bid[6];
            bheader[5] = bid[5];
            bheader[4] = bid[4];
            bheader[3] = bid[3];
            bheader[2] = bid[2];
            bheader[1] = bid[1];
            bheader[0] = bid[0];


            byte[] newbytes = new byte[2 + (bdata.Length / 8) + 1];
            bheader.CopyTo(newbytes, 0);
            bdata.CopyTo(newbytes, 2);
            bfooter.CopyTo(newbytes, newbytes.Length - 1);

            return Encoding.GetEncoding("ISO-8859-1").GetString(newbytes, 0, newbytes.Length);
        }

        public string ToBitString(BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bits.Count; i++)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }

        private int getIntFromBitArray(BitArray bitArray)
        {
            int value = 0;

            for (int i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i])
                    value += Convert.ToInt16(Math.Pow(2, i));
            }

            return value;
        }


    }


}