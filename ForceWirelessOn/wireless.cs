using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ForceWirelessOn
{
    class wireless:IDisposable
    {
        Intermec.DeviceManagement.SmartSystem.ITCSSApi ITCSSApi;
        static bool bStopThread = false;
        Thread thread = null;

        static int _interval = 10;
        public int iInterval
        {
            get { return _interval; }
            set { _interval = value; }
        }
        /// <summary>
        /// SleepTime in seconds
        /// </summary>
        static int _sleepTime = 5;
        public int iSleepTime
        {
            get { return _sleepTime; }
            set { _sleepTime = value; }
        }

        public wireless()
        {
            doLog("wireless class started");
            ITCSSApi = new Intermec.DeviceManagement.SmartSystem.ITCSSApi();
            if(!getBtPower())
                powerBT();
            if(!getCellPower())
                powerCell();

            thread = new Thread(new ThreadStart(checkThread));
            thread.Name = "wireless checkthread";
            thread.Start();
        }
        
        public void Dispose()
        {
            bStopThread = true;
            Thread.Sleep(_sleepTime);
            if (thread!=null)
            {
                if (!thread.Join(500))
                {
                    thread.Abort();
                }
            }
        }

        void checkThread()
        {
            doLog("Thread started");
            int count = 0;
            try
            {
                while (!bStopThread)
                {
                    count++;
                    if (count > _interval)
                    {
                        count = 0;
                        if (!getBtPower())
                            powerBT();
                        if (!getCellPower())
                            powerCell();
                    }
                    Thread.Sleep(_sleepTime*1000); //seconds
                    onWatchdogHandler(new EventArgs());
                }
            }
            catch (Exception ex)
            {
                string s = "Error in background thread: " + ex.Message;
                onUpdateHandler(new MyEventArgs(s));
            }
            doLog("Thread stopped");
            return;
        }

        void powerCell()
        {
            doLog("Trying powerCell");
            String cellOn = "<Subsystem Name=\"WWAN Radio\">" +
                                "<Field Name=\"Radio Power State\">1</Field> " +
                            "</Subsystem>";

            StringBuilder sb = new StringBuilder(255);
            int iLen = 255;
            uint uRes = ITCSSApi.Set(cellOn, sb, ref iLen, 2000);
            if (uRes != Intermec.DeviceManagement.SmartSystem.ITCSSErrors.E_SS_SUCCESS)
            {
                doLog("cellOn error: " + uRes.ToString("X08"));
            }
            else
                doLog("cellOn OK");
            
        }

        void powerBT()
        {
            doLog("Trying powerBT");
            String btON = "  <Subsystem Name=\"Bluetooth\">" +
                            "   <Field Name=\"Power\">On</Field> " +
                            "</Subsystem>";
            StringBuilder sb = new StringBuilder(255);
            int iLen = 255;
            uint uRes = ITCSSApi.Set(btON, sb, ref iLen, 2000);
            if (uRes != Intermec.DeviceManagement.SmartSystem.ITCSSErrors.E_SS_SUCCESS)
            {
                doLog("powerBT error: " + uRes.ToString("X08"));
            }
            else
                doLog("powerBT OK");
        }

        bool getCellPower()
        {
            bool bRet = false;
            String cellON = "  <Subsystem Name=\"WWAN Radio\">" +
                "   <Field Name=\"Radio Power State\"></Field> " +
                "</Subsystem>";

            StringBuilder sb = new StringBuilder(255);
            int iLen = 255;
            uint uRes = ITCSSApi.Get(cellON, sb, ref iLen, 2000);
            if (uRes != Intermec.DeviceManagement.SmartSystem.ITCSSErrors.E_SS_SUCCESS)
            {
                doLog("getCellPower error: " + uRes.ToString("X08"));
            }
            else
            {
                doLog("getCellPower OK");
                // "<Subsystem Name=\"WWAN Radio\"><Field Name=\"Radio Power State\">1</Field></Subsystem>"
                if (sb.ToString().ToLower().Contains("\"radio power state\">1<"))
                    bRet=true;
            }
            doLog("Cell power is " + bRet.ToString());
            return bRet;
        }

        bool getBtPower()
        {
            bool bRet = false;
            String btON = "  <Subsystem Name=\"Bluetooth\">" +
                "   <Field Name=\"Power\"></Field> " +
                "</Subsystem>";

            StringBuilder sb = new StringBuilder(255);
            int iLen = 255;
            uint uRes = ITCSSApi.Get(btON, sb, ref iLen, 2000);
            if (uRes != Intermec.DeviceManagement.SmartSystem.ITCSSErrors.E_SS_SUCCESS)
            {
                doLog("get powerBT error: " + uRes.ToString("X08"));
            }
            else
            {
                doLog("get powerBT OK");
                if (sb.ToString().ToLower().Contains("\"power\">on<"))
                    bRet = true;
            }
            doLog("BT power is " + bRet.ToString());
            return bRet;
        }

        void doLog(String s)
        {
            System.Diagnostics.Debug.WriteLine(s);
            onUpdateHandler(new MyEventArgs(s));
        }

        public class MyEventArgs : EventArgs
        {
            //fields
            public string msg { get; set; }
            public MyEventArgs(string s)
            {
                msg = s;
            }
        }
        public delegate void updateEventHandler(object sender, MyEventArgs eventArgs);
        public event updateEventHandler updateEvent;
        void onUpdateHandler(MyEventArgs args)
        {
            //anyone listening?
            if (this.updateEvent == null)
                return;
            MyEventArgs a = args;
            this.updateEvent(this, a);
        }
        public delegate void watchdogEventHandler(object sender, EventArgs eventArgs);
        public event watchdogEventHandler watchdogEvent;
        void onWatchdogHandler(EventArgs args)
        {
            //anyone listening?
            if (this.watchdogEvent == null)
                return;
            EventArgs a = args;
            this.watchdogEvent(this, a);
        }

    }
    public class MyEventArgs : EventArgs
    {
        //fields
        public string msg { get; set; }
        public MyEventArgs(string s)
        {
            msg = s;
        }
    }
}
