using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ForceWirelessOn
{
    class winapi
    {
        [DllImport("coredll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenEvent(int desiredAccess, bool inheritHandle, string name);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern Int32 WaitForSingleObject(IntPtr Handle, Int32 Wait);

        public const Int32 INFINITE = -1;
        public const Int32 WAIT_ABANDONED = 0x80;
        public const Int32 WAIT_OBJECT_0 = 0x00;
        public const Int32 WAIT_TIMEOUT = 0x102;
        public const Int32 WAIT_FAILED = -1;

        const int EVENT_ALL_ACCESS = 0x001F0003;

        public static void WaitForApiReady(){
            int waitResult = 0;
            IntPtr hWMGREvent = OpenEvent(EVENT_ALL_ACCESS, false, "SYSTEM/GweApiSetReady");
            if(hWMGREvent!=IntPtr.Zero)
                waitResult = WaitForSingleObject(hWMGREvent, INFINITE);

            hWMGREvent = OpenEvent(EVENT_ALL_ACCESS, false, "system/events/notify/APIReady");
            if (hWMGREvent != IntPtr.Zero) 
                waitResult = WaitForSingleObject(hWMGREvent, INFINITE);
            
            hWMGREvent = OpenEvent(EVENT_ALL_ACCESS, false, "SYSTEM/PowerManagerReady");
            if (hWMGREvent != IntPtr.Zero) 
                waitResult = WaitForSingleObject(hWMGREvent, INFINITE);
            System.Threading.Thread.Sleep(10000);
        }
    }
}
