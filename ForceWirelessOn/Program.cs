using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ForceWirelessOn
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            //blocks until API is ready
            winapi.WaitForApiReady();
            Application.Run(new Form1());
        }
    }
}