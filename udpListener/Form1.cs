using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace udpListener
{
    public partial class Form1 : Form
    {
        udpServer server = null;
        public Form1()
        {
            InitializeComponent();
            server = new udpServer();
            server.updateEvent += new udpServer.updateEventHandler(server_updateEvent);
        }

        void server_updateEvent(object sender, udpServer.MyEventArgs eventArgs)
        {
            string s=eventArgs.msg.Replace("\n",System.Environment.NewLine);
            addLog(s);
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

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            if (server != null)
            {
                server.Dispose();
                server = null;
            }
        }
    }
}