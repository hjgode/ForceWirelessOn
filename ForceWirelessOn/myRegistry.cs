using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Microsoft.Win32;

namespace ForceWirelessOn
{
    class myRegistry
    {
        const string sSubkey = "Software\\wireless forced on";
        const string sInterval = "Interval";
        const string sKeySleepTimer = "SleepTimer";
        RegistryKey key=null;

        /// <summary>
        /// 
        /// </summary>
        int _interval = 10;
        public int iInterval
        {
            get { return _interval; }
            set { _interval = value; }
        }
        /// <summary>
        /// SleepTime in seconds
        /// </summary>
        int _sleepTime = 5;
        public int iSleepTime
        {
            get { return _sleepTime; }
            set { _sleepTime = value; }
        }

        public myRegistry()
        {
            try
            {
                key = Registry.LocalMachine.CreateSubKey(sSubkey);
                if (existValue(key, sInterval))
                    _interval = getIntValue(key, sInterval);
                else
                    setIntValue(key, sInterval, _interval);
                if (existValue(key, sKeySleepTimer))
                    _sleepTime = getIntValue(key, sKeySleepTimer);
                else
                    setIntValue(key, sKeySleepTimer, _sleepTime);
                key.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in CreateSubKey: " + ex.Message);
            }
        }

        bool existValue(RegistryKey rKey, string sValue)
        {
            bool bRet = false;
            try
            {
                object o = rKey.GetValue(sValue);
                if (o != null)
                    bRet = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in existValue for " + sValue + " :" + ex.Message);
                               
            }
            return bRet;
        }

        int getIntValue(RegistryKey rKey, string sValue)
        {
            int iRet = -1;
            try
            {
                int i = (int)rKey.GetValue(sValue, -1);
                if (i != -1)
                    iRet=i;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in getIntValue for "+sValue+" :" + ex.Message);                               
            }

            return iRet;
        }

        bool setIntValue(RegistryKey rKey, string sValue, int iValue)
        {
            bool bRet = false;
            try
            {
                rKey.SetValue(sValue, iValue, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in setIntValue for " + sValue + " :" + ex.Message);                               
            }
            return bRet;
        }
    }
}
