ForcedWirelessOnW32

This app will forcily power on BT and WWAN in a specified interval. It will run in background process (no GUI).

The cmd line application will prevent multiple instances running using a MUTEX.

The configuration is done via the registry:

	REGEDIT4
	[HKEY_LOCAL_MACHINE\Software\wireless forced on]
	"SleepTimer"=dword:00000005
	"Interval"=dword:0000000A

SleepTimer (interger, default = 5, is 5 seconds) specifies the amount of time the background thread will sleep
Interval (integer, default = 10) is the number of sleep times between checking the power of BT and WWAN
The above will check the power every 5*10=50 seconds
The registry is checked only at startup.

Having a low SleepTimer enlarges the cpu usage of the application!

You can watch the app working using an UDP listener. All actions of the app are broadcasted as UDP packets (strings).

To stop ForcedWirelessOnW32, lauch it with the argument 'stop'. There should be a link installed in Programs the will just do this.
 