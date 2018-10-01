// ForceWirelessOnW32.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "registry.h"
#include "nclog.h"

#include "itcSSApi.h"
#pragma comment(lib, "ITCSSApi.lib")

#include "smartsysErrors.h"


HANDLE hMutex=NULL;
#define MY_MUTEX L"ForecWirelessON"

HANDLE hThread=NULL;
DWORD threadID=0;
HANDLE hStop=NULL;
#define MYSTOPEVENT L"STOPME"

TCHAR* sSubkey = L"Software\\wireless forced on";
TCHAR* sInterval = L"Interval";
TCHAR* sKeySleepTimer = L"SleepTimer";
DWORD iInterval=10;
DWORD iSleepTimer=5;

static const TCHAR xmlWWANpowerGET[] =
_T("<Subsystem Name=\"WWAN Radio\">\r\n \
		<Field Name=\"Radio Power State\"></Field>\r\n \
   </Subsystem>\r\n");
static const TCHAR xmlWWANpowerON[] =
_T("<Subsystem Name=\"WWAN Radio\">\r\n \
		<Field Name=\"Radio Power State\">1</Field>\r\n \
   </Subsystem>\r\n");
static const TCHAR xmlBTpowerGET[] =
_T("<Subsystem Name=\"Bluetooth\">\r\n \
		<Field Name=\"Power\"></Field>\r\n \
   </Subsystem>\r\n");
static const TCHAR xmlBTpowerON[] =
_T("<Subsystem Name=\"Bluetooth\">\r\n \
		<Field Name=\"Power\">On</Field>\r\n \
   </Subsystem>\r\n");

TCHAR* dwModesStr[]={L"BTH_POWER_OFF", L"BTH_CONNECTABLE", L"BTH_DISCOVERABLE"};

DWORD getBTMode(){
	nclog(L"getBTMode...\n");
	DWORD dwMode=BTH_POWER_OFF;
	int iRes=BthGetMode(&dwMode);
	if(iRes==ERROR_SUCCESS){
		nclog(L"BthGetMode=OK : %s\n", dwModesStr[dwMode]);
		return dwMode;
	}
	else{
		nclog(L"BthGetMode iRes=%i : LastError=%i\n", iRes, GetLastError());
		return -1;
	}
}

int setBTMode(DWORD dwMode){
	nclog(L"setBTMode %s...\n", dwModesStr[dwMode]);
	int iRes=0;
	iRes=BthSetMode(dwMode);
	if(iRes==ERROR_SUCCESS){
		nclog(L"BthSetMode=OK for %s\n", dwModesStr[dwMode]);
	}
	else{
		nclog(L"BthSetMode=failed for %s, iRes=%i, LastError=%i\n", dwModesStr[dwMode], iRes, GetLastError());
		iRes=-1;
	}
	return iRes;
}

void regRead(){
	OpenCreateKey(sSubkey);
	DWORD dwVal=0;
	if(RegReadDword(sInterval, &dwVal)==ERROR_SUCCESS){
		iInterval=dwVal;
	}else{
		RegWriteDword(sInterval, &iInterval);
	}
	if(RegReadDword(sKeySleepTimer, &dwVal)==ERROR_SUCCESS){
		iSleepTimer=dwVal;
	}else{
		RegWriteDword(sKeySleepTimer, &iSleepTimer);
	}
	DEBUGMSG(1, (L"ForceWirelessOn\n\tinterval=%i\n\tsleeptimer=%i\n", iInterval, iSleepTimer));
	nclog(L"ForceWirelessOn\n\tinterval=%i\n\tsleeptimer=%i\n", iInterval, iSleepTimer);
}

/**
* setConfigurationData ()
* This function will set the configuration on the device contained in the
* pszConfigData paramater via the SS api.
*
* @param pszConfigData The configuration data to set.
*
*/
int setConfigurationData (TCHAR *pszConfigData)
{
	int		iRet = 0;
	TCHAR	*pRetData;
	size_t  iRetDataSize = 0;
	int		iLen = 0;
	
	ITCSSAPI_RETURN_TYPE sRet;

	iLen = _tcslen (pszConfigData) + 1024;  //ensure enough space for returned data
	pRetData = (TCHAR *)malloc (iLen * sizeof (TCHAR)); 
	memset (pRetData, 0, iLen * sizeof (TCHAR));
	iRetDataSize = iLen;
	sRet = ITCSSSet (pszConfigData, pRetData, &iRetDataSize, 0);
	if (sRet != E_SS_SUCCESS)
	{
		iRet = -1;
	}
	else
	{
		iRet = 0;
	}

	if (pRetData != 0)
	{
		free (pRetData);
	}
	return iRet;
}
/**
* getConfigurationData ()
* This function will set the configuration on the device contained in the
* pszConfigData paramater via the SS api.
*
* @param pszConfigData The configuration data to get.
*
*/
int getConfigurationData (TCHAR *pszConfigData, TCHAR *pRetData, size_t *piLen)
{
	int  iRet = 0;
	int  iLen = 0;
	ITCSSAPI_RETURN_TYPE sRet;

	sRet = ITCSSGet (pszConfigData, pRetData, piLen, 0);
	if (sRet != E_SS_SUCCESS)
	{
		iRet = -1;
	}
	else
	{
		iRet = 0;
	}

	return iRet;
}

DWORD getBTpower(){
	int iRet;
	TCHAR szConfigData[512] = {0};
	TCHAR pRetData[512] = {0};
	size_t len = 512;
	TCHAR power[128] = {0};

	_stprintf (szConfigData, xmlBTpowerGET);
	iRet = getConfigurationData (szConfigData, pRetData, &len);

	if (iRet == 0)
	{
		TCHAR* lookFor=_T("<Field Name=\"Power\">");
			TCHAR *pData = _tcsstr (pRetData, lookFor);
			if (pData != NULL)
			{
				pData = pData + wcslen(lookFor);
				for (int ii=0; *pData != '<'; ii++, pData++)
				{
					power[ii] = *pData;
				}
			}
	}
	else
	{
		DEBUGMSG(1, (L"Error retrieving BT Power\n"));
		nclog(L"Error retrieving BT Power\n");
	}
	if(wcsicmp(power, L"on")==0)
		iRet=1;
	else
		iRet=0;

	return iRet;
}

DWORD getWWANpower(){
	int iRet;
	TCHAR szConfigData[512] = {0};
	TCHAR pRetData[512] = {0};
	size_t len = 512;
	TCHAR power[128] = {0};

	_stprintf (szConfigData, xmlWWANpowerGET);
	iRet = getConfigurationData (szConfigData, pRetData, &len);

	if (iRet == 0)
	{
		TCHAR* lookFor=_T("<Field Name=\"Radio Power State\">");
			TCHAR *pData = _tcsstr (pRetData, lookFor);
			if (pData != NULL)
			{
				pData = pData + wcslen(lookFor);
				for (int ii=0; *pData != '<'; ii++, pData++)
				{
					power[ii] = *pData;
				}
			}
	}
	else
	{
		DEBUGMSG(1, (L"Error retrieving BT Power\n"));
		nclog(L"Error retrieving BT Power\n");
	}
	if(wcsicmp(power, L"1")==0)
		iRet=1;
	else
		iRet=0;

	return iRet;
}

DWORD myThread(LPVOID lpParm){
	DEBUGMSG(1, (L"ForceWirelessOn: thread starting...\n"));
	nclog(L"ForceWirelessOn: thread starting...\n");
	BOOL bContinue=TRUE;
	DWORD dwWait=0;
	DWORD dwCounter=0;
	TCHAR szSetXML[1024] = {0};
	do{
		dwWait = WaitForSingleObject(hStop, iSleepTimer*1000);
		switch(dwWait){
			case WAIT_OBJECT_0:
				DEBUGMSG(1, (L"ForceWirelessOn: stop event set...\n"));
				nclog(L"ForceWirelessOn: stop event set...\n");
				bContinue=FALSE;
				break;
			case WAIT_TIMEOUT:
				dwCounter++;
				if(dwCounter>iInterval){
					//check connections and re-power BT/WWAN if OFF
					DEBUGMSG(1, (L"ForceWirelessOn: WAIT_TIMEOUT...\n"));
					nclog(L"ForceWirelessOn: WAIT_TIMEOUT...\n");
					if(getBTpower()==1){
						DEBUGMSG(1,(L"ForceWirelessOn: BT is on\n"));
						nclog(L"ForceWirelessOn: BT is on\n");
					}
					else{
						DEBUGMSG(1,(L"ForceWirelessOn: BT is OFF\n"));
						nclog(L"ForceWirelessOn: BT is OFF\n");
						_stprintf(szSetXML, xmlBTpowerON);
						if(setConfigurationData(szSetXML)==0){
							DEBUGMSG(1,(L"ForceWirelessOn: setConfigurationData BT OK\n"));
							nclog(L"ForceWirelessOn: setConfigurationData BT OK\n");
						}
						else{
							DEBUGMSG(1,(L"ForceWirelessOn: setConfigurationData BT failed\n"));
							nclog(L"ForceWirelessOn: setConfigurationData BT failed\n");
						}
					}
					//do the same with MS BT API
					if(getBTMode()==BTH_DISCOVERABLE || getBTMode()==BTH_CONNECTABLE){
						nclog(L"ForceWirelessOn: MS BT is on\n");
					}else{
						nclog(L"ForceWirelessOn: MS BT is OFF\n");
						setBTMode(BTH_CONNECTABLE);
					}

					//### WWAN...
					if(getWWANpower()==1){
						DEBUGMSG(1,(L"ForceWirelessOn: WWAN is on\n"));
						nclog(L"ForceWirelessOn: WWAN is on\n");
					}
					else{
						DEBUGMSG(1,(L"ForceWirelessOn: WWAN is OFF\n"));
						nclog(L"ForceWirelessOn: WWAN is OFF\n");
						_stprintf(szSetXML, xmlWWANpowerON);
						if(setConfigurationData(szSetXML)==0){
							DEBUGMSG(1,(L"ForceWirelessOn: setConfigurationData WWAN OK\n"));
							nclog(L"ForceWirelessOn: setConfigurationData WWAN OK\n");
						}
						else{
							DEBUGMSG(1,(L"ForceWirelessOn: setConfigurationData WWAN failed\n"));
							nclog(L"ForceWirelessOn: setConfigurationData WWAN failed\n");
						}
					}

					dwCounter=0;
				}
				break;
			default:
				break;
		}
	}while(bContinue);
	DEBUGMSG(1, (L"ForceWirelessOn: thread STOPPED\n"));
	nclog(L"ForceWirelessOn: thread STOPPED\n");
	free (szSetXML);
	return 0;
}

void waitForAPIs(){
    int waitResult = 0;
    HANDLE hWMGREvent = OpenEvent(EVENT_ALL_ACCESS, false, L"SYSTEM/GweApiSetReady");
    if(hWMGREvent!=NULL)
        waitResult = WaitForSingleObject(hWMGREvent, INFINITE);

    hWMGREvent = OpenEvent(EVENT_ALL_ACCESS, false, L"system/events/notify/APIReady");
    if(hWMGREvent!=NULL)
        waitResult = WaitForSingleObject(hWMGREvent, INFINITE);
    
    hWMGREvent = OpenEvent(EVENT_ALL_ACCESS, false, L"SYSTEM/PowerManagerReady");
    if(hWMGREvent!=NULL)
        waitResult = WaitForSingleObject(hWMGREvent, INFINITE);
    Sleep(3000);
	DEBUGMSG(1, (L"ForceWirelessOn waitForAPIs DONE\n"));
	nclog(L"ForceWirelessOn waitForAPIs DONE\n");
}

int _tmain(int argc, _TCHAR* argv[])
{
	if(argc==1){ //no arg
		DEBUGMSG(1, (L"ForceWirelessOn starting: no args\n"));
		nclog(L"ForceWirelessOn starting: no args\n");
	}
	else if(argc==2){
		DEBUGMSG(1, (L"ForceWirelessOn starting: one arg: '%s'\n", argv[1]));
		nclog(L"ForceWirelessOn starting: one arg: '%s'\n", argv[1]);
		if(wcsicmp(argv[1], L"stop")==0){
			hStop=CreateEvent(NULL, FALSE, FALSE, MYSTOPEVENT);
			SetEvent(hStop);
			return 1;
		}
	}

	//##################### dont run if already running #############################
	DEBUGMSG(1, (L"Checking for Mutex (single instance allowed only)...\n"));
	nclog(L"Checking for Mutex (single instance allowed only)...\n");

	hMutex=CreateMutex(NULL, TRUE, MY_MUTEX);
	if(hMutex==NULL){
		//this should never happen
		DWORD dwErr=GetLastError();
		DEBUGMSG(1, (L"Error in CreateMutex! GetLastError()=%i\n", dwErr));
		nclog(L"Error in CreateMutex! GetLastError()=%i\n", dwErr);
		DEBUGMSG(1, (L"-------- END -------\n"));
		nclog(L"-------- END -------\n");
		return -99;
	}
	DWORD dwLast = GetLastError();
	if(dwLast== ERROR_ALREADY_EXISTS){//mutex already exists, wait for mutex release
		DEBUGMSG(1, (L"\tAttached to existing mutex\n"));
		nclog(L"\tAttached to existing mutex\n");
		//DEBUGMSG(1, (L"................ Waiting for mutex release......\n"));
		//WaitForSingleObject( hMutex, INFINITE );
		//DEBUGMSG(1, (L"++++++++++++++++ Mutex released. +++++++++++++++\n"));
		return 88;
	}
	else{
		DEBUGMSG(1, (L"\tCreated new mutex\n"));
		nclog(L"\tCreated new mutex\n");
	}
	//##################### dont run if already running #############################

	waitForAPIs();

	regRead();

	hStop=CreateEvent(NULL, TRUE, FALSE, MYSTOPEVENT);
	if(hStop==NULL){
		DEBUGMSG(1,(L"CreateEvent for StopThread failed: 0x%08x\n", GetLastError()));
		nclog(L"CreateEvent for StopThread failed: 0x%08x\n", GetLastError());
	}
	hThread=CreateThread(NULL, 0, myThread, NULL, 0, &threadID);

	BOOL bStop=FALSE;
	do{
		DWORD dwWait=0;
		dwWait=WaitForSingleObject(hStop, 10*1000);
		switch (dwWait){
			case WAIT_OBJECT_0:
				bStop=TRUE;
				break;
			case WAIT_TIMEOUT:
				break;
			default:
				break;
		}
	}while(!bStop);
	ResetEvent(hStop);
	nclog(L"ForceWirelessOn ENDED");
	return 0;
}

