using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;

namespace ForceWirelessOn
{
    public partial class Form1 : Form
    {
        wireless myWireless;
        int iActivity = 0;
        string[] sAnimation={ ".", "-", "/" , "-", "\\" };
        public Form1()
        {
            InitializeComponent();
            System.Threading.TimerCallback timerDelegate = new System.Threading.TimerCallback(_TimerCallback);
            System.Threading.Timer timer = new System.Threading.Timer(timerDelegate, null, 5000, System.Threading.Timeout.Infinite);
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Really close?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.No)
                e.Cancel = true;
            else
            {
                if (myWireless != null)
                    myWireless.Dispose();
            }
        }

        delegate void hideDelegate();
        public void HideMe()
        {
            if (this.InvokeRequired)
            {
                hideDelegate d = new hideDelegate(HideMe);
                this.Invoke(d, new object[] { });
            }
            else
            {
                ShowWindow(this.Handle, 6); //Minimize
            }
        }

        delegate void toggleActivityDelegate();
        public void toggleActivity()
        {
            if (this.InvokeRequired)
            {
                toggleActivityDelegate d = new toggleActivityDelegate(toggleActivity);
                this.Invoke(d, new object[] { });
            }
            else
            {
                label2.Text = sAnimation[iActivity];
                iActivity++;
                if (iActivity > sAnimation.Length-1)
                    iActivity = 0;
            }

        }

        void _TimerCallback(Object stateInfo)
        {
            myWireless = new wireless();
            myRegistry reg = new myRegistry();
            myWireless.iInterval = reg.iInterval;
            myWireless.iSleepTime = reg.iSleepTime;

            myWireless.updateEvent += new wireless.updateEventHandler(myWireless_updateEvent);
            myWireless.watchdogEvent += new wireless.watchdogEventHandler(myWireless_watchdogEvent);
            HideMe();
        }

        void myWireless_watchdogEvent(object sender, EventArgs eventArgs)
        {
            toggleActivity();
        }

        void myWireless_updateEvent(object sender, wireless.MyEventArgs eventArgs)
        {
            addLog(eventArgs.msg);
        }

        delegate void SetTextCallback(string text);
        public void addLog(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.txtLog.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(addLog);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (txtLog.Text.Length > 2000)
                    txtLog.Text = "";
                txtLog.Text += text + "\r\n";
                txtLog.SelectionLength = 0;
                txtLog.SelectionStart = txtLog.Text.Length - 1;
                txtLog.ScrollToCaret();
            }
        }

        [DllImport("coredll.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}