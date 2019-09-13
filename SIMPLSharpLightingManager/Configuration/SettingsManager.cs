using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SIMPLSharpLightingManager.Configuration
{
    public class SettingsManager
    {
        public string ReadSettingsFile(string paramFilePath)
        {
            string str = "";
            try
            {
                if (File.Exists(paramFilePath))
                    str = File.ReadToEnd(paramFilePath, Encoding.UTF8);
                else
                    ErrorLog.Error("configuration file not found.");
            }
            catch (Exception ex)
            {
                ErrorLog.Error("Error while reading configuration file:");
                throw ex;
            }
            return str;
        }

        public SettingsManager()
        {
        }

        internal LightDeviceSettings GetDeviceConfiguration(
          string paramConfigurationString)
        {
            JObject jobject;
            try
            {
                jobject = JObject.Parse(paramConfigurationString);
            }
            catch (Exception ex)
            {
                ErrorLog.Notice("An exception occured while parsing the device configuration file. Please check if the file is formatted correctly.");
                throw ex;
            }
            try
            {
                if (jobject != null)
                {
                    string str = jobject.Value<string>((object)"configFileVersion") ?? "None";
                    LightDeviceSettings heSystemSettings = new LightDeviceSettings();
                    if (str == heSystemSettings.configFileVersion)
                        return JsonConvert.DeserializeObject<LightDeviceSettings>(paramConfigurationString);
                    ErrorLog.Notice("Settings Manager: System configuration file versions do not match. Module Version:{0}, ConfigFile Version:{1}", (object)heSystemSettings.configFileVersion, (object)str);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Notice("An exception occured while parsing the device configuration file. Please check if the file is formatted correctly.");
                throw ex;
            }
            return (LightDeviceSettings)null;
        }

        internal LightSystemSettings GetLightSystemConfiguration(
          string paramConfigurationString)
        {
            JObject jobject = (JObject)null;
            try
            {
                jobject = JObject.Parse(paramConfigurationString);
            }
            catch
            {
                ErrorLog.Notice("An exception occured while parsing the system configuration file. Please check if the file is formatted correctly.");
            }
            try
            {
                if (jobject != null)
                {
                    string str = jobject.Value<string>((object)"configFileVersion") ?? "None";
                    LightSystemSettings heMediaSettings = new LightSystemSettings();
                    if (str == heMediaSettings.configFileVersion)
                        return JsonConvert.DeserializeObject<LightSystemSettings>(paramConfigurationString);
                    ErrorLog.Notice("Settings Manager: Media configuration file versions do not match. Module Version:{0}, ConfigFile Version:{1}", (object)heMediaSettings.configFileVersion, (object)str);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Notice("An exception occured while parsing the system configuration file. Please check if the file is formatted correctly.");
            }
            return (LightSystemSettings)null;
        }


        internal static double ConvertStringNumberToDouble(string paramValue)
        {
            try
            {

                if (paramValue.Contains('%'))
                {
                    string[] strArray = paramValue.Split('%');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Percentage value is not formatted correctly");
                    return double.Parse(strArray[0]);
                }
                if (paramValue.Contains('s'))
                {
                    string[] strArray = paramValue.Split('s');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Seconds value is not formatted correctly");
                    return double.Parse(strArray[0]);
                }
                if (!paramValue.Contains('d'))
                    return 0.0;
                string[] strArray1 = paramValue.Split('d');
                if (strArray1.Length > 2 || !string.IsNullOrEmpty(strArray1[1]))
                    ErrorLog.Error("Decimal value is not formatted correctly");
                return double.Parse(strArray1[0]);
            }
            catch (Exception ex)
            {
                ErrorLog.Error("Exception occured while converting a string number to double.\n");
                throw ex;
            }
        }

        internal static int ConvertStringNumberToInt(string paramValue)
        {
            try
            {
                if (paramValue.Contains('%'))
                {
                    string[] strArray = paramValue.Split('%');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Percentage value is not formatted correctly");
                    return int.Parse(strArray[0]);
                }
                if (paramValue.Contains('s'))
                {
                    string[] strArray = paramValue.Split('s');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Seconds value is not formatted correctly");
                    return int.Parse(strArray[0]);
                }
                if (paramValue.Contains('m'))
                {
                    string[] strArray = paramValue.Split('m');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Minutes value is not formatted correctly");
                    return int.Parse(strArray[0]) * 60;
                }
                if (paramValue.Contains('h'))
                {
                    string[] strArray = paramValue.Split('h');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Hex value is not formatted correctly");
                    return Convert.ToInt32(strArray[0], 16);
                }
                if (!paramValue.Contains('d'))
                    return 0;
                string[] strArray1 = paramValue.Split('d');
                if (strArray1.Length > 2 || !string.IsNullOrEmpty(strArray1[1]))
                    ErrorLog.Error("Decimal value is not formatted correctly");
                return int.Parse(strArray1[0]);
            }
            catch (Exception ex)
            {
                ErrorLog.Error("Exception occured while converting a string number to int.\n");
                throw ex;
            }
        }

        internal static double ConvertUnsignedStringNumber(string paramValue)
        {
            try
            {
                if (paramValue.Contains('%'))
                {
                    string[] strArray = paramValue.Split('%');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Percentage value is not formatted correctly");
                    double num1 = double.Parse(strArray[0]);
                    if (num1 <= 0.0)
                        return 0.0;
                    if (num1 >= 100.0)
                        return (double)ushort.MaxValue;
                    double num2 = Math.Round((double)ushort.MaxValue * num1 / 100.0);
                    if (num2 < (double)ushort.MaxValue)
                        return num2 + 1.0;
                    return (double)ushort.MaxValue;
                }
                if (paramValue.Contains('s'))
                {
                    string[] strArray = paramValue.Split('s');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Seconds value is not formatted correctly");
                    return double.Parse(strArray[0]);
                }
                if (!paramValue.Contains('d'))
                    return 0.0;
                string[] strArray1 = paramValue.Split('d');
                if (strArray1.Length > 2 || !string.IsNullOrEmpty(strArray1[1]))
                    ErrorLog.Error("Decimal value is not formatted correctly");
                return double.Parse(strArray1[0]);
            }
            catch (Exception ex)
            {
                ErrorLog.Error("Exception occured while converting an unsigned string number to double.\n");
                throw ex;
            }
        }

        internal static double ConvertSignedStringNumber(string paramValue)
        {
            try
            {
                if (paramValue.Contains('%'))
                {
                    string[] strArray = paramValue.Split('%');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Percentage value is not formatted correctly");
                    double num = Math.Floor(double.Parse(strArray[0]));
                    if (num < 0.0)
                    {
                        if (num >= -50.0)
                            return (double)short.MaxValue * num / 50.0;
                        return -32767.0;
                    }
                    if (num > 0.0)
                    {
                        if (num <= 50.0)
                            return (double)short.MaxValue * num / 50.0;
                        return (double)short.MaxValue;
                    }
                }
                else
                {
                    if (!paramValue.Contains('d'))
                        return 0.0;
                    string[] strArray = paramValue.Split('d');
                    if (strArray.Length > 2 || !string.IsNullOrEmpty(strArray[1]))
                        ErrorLog.Error("Decimal value is not formatted correctly");
                    return double.Parse(strArray[0]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Error("Exception occured while converting a signed string number to double.\n");
                throw ex;
            }
            return 0.0;
        }
    }
}
