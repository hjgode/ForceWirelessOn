ForcedWirelessOn

This app will forcily power on BT and WWAN in a specified interval. It will run in background (minimized).

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

You can see the application GUI by switching to the application using the Mobile TaskManager