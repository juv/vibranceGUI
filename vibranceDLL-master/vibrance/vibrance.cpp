#include "stdafx.h"
#include <windows.h>
#include <iostream>
#include <thread>
#include "vibrance.h"
using namespace std;

namespace vibranceDLL
{
	vibrance::NvAPI_QueryInterface_t					NvAPI_QueryInterface     = NULL;
	vibrance::NvAPI_Initialize_t						NvAPI_Initialize         = NULL;
	vibrance::NvAPI_Unload_t							NvAPI_Unload			 = NULL;
	vibrance::NvAPI_EnumPhysicalGPUs_t					NvAPI_EnumPhysicalGPUs   = NULL;
	vibrance::NvAPI_GPU_GetUsages_t						NvAPI_GPU_GetUsages      = NULL;
	vibrance::NvAPI_GPU_GetFullName_t					NvAPI_GPU_GetFullName	 = NULL;
	vibrance::NvAPI_GPU_GetActiveOutputs_t				NvAPI_GPU_GetActiveOutputs = NULL;
	vibrance::NvAPI_GetDVCInfo_t						NvAPI_GetDVCInfo		 = NULL;
	vibrance::NvAPI_GetDVCInfoEx_t						NvAPI_GetDVCInfoEx		 = NULL;
	vibrance::NvAPI_SetDVCLevel_t						NvAPI_SetDVCLevel		 = NULL;
	vibrance::NvAPI_EnumNvidiaDisplayHandle_t			NvAPI_EnumNvidiaDisplayHandle = NULL;
	vibrance::NvAPI_GetInterfaceVersionString_t			NvAPI_GetInterfaceVersionString = NULL;
	vibrance::NvAPI_GetErrorMessage_t					NvAPI_GetErrorMessage = NULL;
	vibrance::NvAPI_GetAssociatedNvidiaDisplayHandle_t	NvAPI_GetAssociatedNvidiaDisplayHandle = NULL;

	bool shouldRun;
	int defaultHandle;

	void vibrance::printError(_NvAPI_Status status)
	{
		char szDesc[64] = {0};
		NvAPI_GetErrorMessage(status, szDesc);
	}


	void vibrance::handleDVC()
	{
		bool isChanged = false;
		while(shouldRun)
		{
			HWND hwnd = NULL;
			if(isCsgoStarted(&hwnd) && hwnd != NULL)
			{
				if(isCsgoActive(&hwnd))
				{
					int nLen = GetWindowTextLength(hwnd);
					if(nLen > 0)
					{
						char *szTitle = (char*)malloc(nLen + 1);
						GetWindowTextA(hwnd, szTitle, nLen+1);
						if(szTitle != NULL)
						{
							if(!equalsDVCLevel(defaultHandle, NVAPI_MAX_LEVEL))
							{
								setDVCLevel(defaultHandle, NVAPI_MAX_LEVEL);
								isChanged = true;
							}
						}
					}
				}
				else
				{
					if(isChanged)
					{
						setDVCLevel(defaultHandle, NVAPI_DEFAULT_LEVEL);
						isChanged = false;
					}
				}
			}
			else
			{
				if(isChanged || !equalsDVCLevel(defaultHandle, NVAPI_DEFAULT_LEVEL))
				{
					setDVCLevel(defaultHandle, NVAPI_DEFAULT_LEVEL);
					isChanged = false;
				}

			}
			Sleep(5000);
		}
		if(!equalsDVCLevel(defaultHandle, NVAPI_DEFAULT_LEVEL))
			setDVCLevel(defaultHandle, NVAPI_DEFAULT_LEVEL);

		std::cout << "DVC Level Thread exited!" << std::endl;
	}


	bool vibrance::isCsgoStarted(HWND *hwnd)
	{
		HWND test = FindWindowW(0, L"Counter-Strike: Global Offensive");
		*hwnd = test;
		if (!hwnd)   // Process is not running
		{
			return false;
		}
		return true;
	}

	bool vibrance::isCsgoActive(HWND *hwnd)
	{
		HWND activeWindow = GetForegroundWindow();
		if(activeWindow == NULL)
			return false;
		return *hwnd == activeWindow;
	}

	bool vibrance::equalsDVCLevel(int defaultHandle, int level)
	{	
		NV_DISPLAY_DVC_INFO info = {};
		if(getDVCInfo(&info, defaultHandle))
		{
			return info.currentLevel == level;
		}
		return false;
	}

	bool vibrance::getDVCInfo(NV_DISPLAY_DVC_INFO *info, int defaultHandle)
	{
		//NV_DISPLAY_DVC_INFO info2 = {};

		//info2.version = sizeof(NV_DISPLAY_DVC_INFO) | 0x10000;
		//int status2 = (*NvAPI_GetDVCInfo)(defaultHandle, NULL, &info2);
		//std::cerr << info2.currentLevel << std::endl;

		//info->currentLevel = info2.currentLevel; 
		//info->maxLevel = info2.maxLevel; 
		//info->minLevel = info2.minLevel; 

		info->version = sizeof(NV_DISPLAY_DVC_INFO) | 0x10000;
		_NvAPI_Status status = (_NvAPI_Status)(*NvAPI_GetDVCInfo)(defaultHandle, 0, info);


		if(status != NVAPI_OK)
		{
			return false;
		}
		return true;
	}

	bool vibrance::setDVCLevel(int defaultHandle, int level)
	{
		_NvAPI_Status status = (_NvAPI_Status)(*NvAPI_SetDVCLevel)(defaultHandle, 0, level);
		if(status != NVAPI_OK)
		{
			return false;
		}
		return true;
	}

	int vibrance::enumerateNvidiaDisplayHandle()
	{
		int defaultHandle = 0;
		_NvAPI_Status status = (_NvAPI_Status)(*NvAPI_EnumNvidiaDisplayHandle)(0, &defaultHandle);
		if(status != 0)
		{
			return 0;
		}
		return defaultHandle;
	}

	bool vibrance::getInterfaceVersionString(char* szVersion)
	{
		_NvAPI_Status status = (_NvAPI_Status)(*NvAPI_GetInterfaceVersionString)(szVersion);
		if(status != 0)
		{
			return false;
		}
		return true;
	}

	void vibrance::enumeratePhsyicalGPUs(int *gpuHandles[])
	{
		int          gpuCount = 0;
		unsigned int gpuUsages[NVAPI_MAX_USAGES_PER_GPU] = { 0 };

		// gpuUsages[0] must be this value, otherwise NvAPI_GPU_GetUsages won't work
		gpuUsages[0] = (NVAPI_MAX_USAGES_PER_GPU * 4) | 0x10000;

		_NvAPI_Status status = (_NvAPI_Status)(*NvAPI_EnumPhysicalGPUs)(gpuHandles, &gpuCount);
		if(status != 0)
		{
			return;
		}
	}

	bool vibrance::getGpuName(int *gpuHandles[], char* szName)
	{
		_NvAPI_Status status = (_NvAPI_Status)(*NvAPI_GPU_GetFullName)(gpuHandles[0], szName);
		if(status != 0)
		{
			return false;
		}
		return true;
	}

	int vibrance::getActiveOutputs(int *gpuHandles[])
	{
		int outputId = 0; 
		_NvAPI_Status status = (_NvAPI_Status)(*NvAPI_GPU_GetActiveOutputs)(gpuHandles[0], &outputId);
		if(status != 0)
		{
			return status;
		}
		return outputId;
	}

	int vibrance::getAssociatedNvidiaDisplayHandle(const char *szDisplayName, int length)
	{
		int outputId = 0; 

		_NvAPI_Status status = (_NvAPI_Status)(*NvAPI_GetAssociatedNvidiaDisplayHandle)("\\\\.\\DISPLAY2", &outputId);

		if(status == 0)
		{
			return outputId;
		}
		return -1;
	}


	bool vibrance::unloadLibrary()
	{	
		int ret = (*NvAPI_Unload)();
		if(ret == 0)
			return true;
		return false;
	}

	bool vibrance::initializeLibrary()
	{
		HMODULE hmod = LoadLibraryA("nvapi.dll");
		if (hmod == NULL)
		{
			return false;
		}

		NvAPI_QueryInterface = (NvAPI_QueryInterface_t) GetProcAddress(hmod, "nvapi_QueryInterface");

		NvAPI_Initialize = (NvAPI_Initialize_t) (*NvAPI_QueryInterface)(0x0150E828);
		NvAPI_Unload = (NvAPI_Unload_t) (*NvAPI_QueryInterface)(0x0D22BDD7E);
		NvAPI_EnumPhysicalGPUs = (NvAPI_EnumPhysicalGPUs_t) (*NvAPI_QueryInterface)(0xE5AC921F);
		NvAPI_GPU_GetFullName = (NvAPI_GPU_GetFullName_t) (*NvAPI_QueryInterface)(0xCEEE8E9F);
		NvAPI_GPU_GetActiveOutputs = (NvAPI_GPU_GetActiveOutputs_t) (*NvAPI_QueryInterface)(0x0E3E89B6F);
		NvAPI_GetDVCInfo = (NvAPI_GetDVCInfo_t) (*NvAPI_QueryInterface)(0x4085DE45);
		NvAPI_SetDVCLevel = (NvAPI_SetDVCLevel_t) (*NvAPI_QueryInterface)(0x172409B4);
		NvAPI_EnumNvidiaDisplayHandle = (NvAPI_EnumNvidiaDisplayHandle_t) (*NvAPI_QueryInterface)(0x9ABDD40D);
		NvAPI_GetInterfaceVersionString = (NvAPI_GetInterfaceVersionString_t) (*NvAPI_QueryInterface)(0x1053FA5);
		NvAPI_GetErrorMessage = (NvAPI_GetErrorMessage_t) (*NvAPI_QueryInterface)(0x6C2D048C);
		NvAPI_GetDVCInfoEx = (NvAPI_GetDVCInfoEx_t) (*NvAPI_QueryInterface)(0x0E45002D);
		NvAPI_GetAssociatedNvidiaDisplayHandle = (NvAPI_GetAssociatedNvidiaDisplayHandle_t) (*NvAPI_QueryInterface)(0x35C29134);


		if (NvAPI_Initialize == NULL || NvAPI_Unload == NULL ||
			NvAPI_EnumPhysicalGPUs == NULL ||NvAPI_GPU_GetFullName == NULL ||
			NvAPI_GPU_GetActiveOutputs == NULL || NvAPI_GetDVCInfo == NULL || 
			NvAPI_SetDVCLevel == NULL || NvAPI_EnumNvidiaDisplayHandle == NULL || 
			NvAPI_GetInterfaceVersionString == NULL || NvAPI_GetErrorMessage == NULL || NvAPI_GetDVCInfoEx == NULL)
		{
			return false;
		}

		int ret = (*NvAPI_Initialize)();
		if(ret == 0)
		{
			return true;
		}
		return false;
	}

	vibrance::vibrance(void)
	{
	}


	vibrance::~vibrance(void)
	{
	}

}