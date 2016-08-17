using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using vibrance.GUI.AMD.vendor.adl32;

namespace vibrance.GUI.AMD.vendor
{
    public class AmdAdapter64 : IAmdAdapter
    {
        public bool IsAvailable()
        {
            if (ADL.ADL_Main_Control_Create != null)
            {
                if (ADL.ADL_SUCCESS == ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1))
                {
                    if (ADL.ADL_Main_Control_Destroy != null)
                    {
                        ADL.ADL_Main_Control_Destroy();
                    }

                    return true;
                }
            }

            return false;
        }

        public void SetSaturationOnAllDisplays(int vibranceLevel)
        {
            this.SetSaturationOnDisplay(vibranceLevel, null);
        }

        public void SetSaturationOnDisplay(int vibranceLevel, string displayName)
        {
            SetSaturation((adlDisplayInfo, adlAdapterInfo, adapterIndex) =>
            {
                int infoValue = adlDisplayInfo.DisplayID.DisplayLogicalIndex;

                bool adapterIsAssociatedWithDisplay = adapterIndex == adlDisplayInfo.DisplayID.DisplayLogicalAdapterIndex;
                if (infoValue != -1 && adapterIsAssociatedWithDisplay && adlAdapterInfo.DisplayName == displayName)
                {
                    ADL.ADL_Display_Color_Set(
                        adapterIndex,
                        infoValue,
                        ADL.ADL_DISPLAY_COLOR_SATURATION,
                        vibranceLevel);
                }
            }, displayName);
        }

        private void SetSaturation(Action<ADLDisplayInfo, ADLAdapterInfo, int> handle, string displayName)
        {
            int numberOfAdapters = 0;

            ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);

            if (ADL.ADL_Adapter_NumberOfAdapters_Get != null)
            {
                ADL.ADL_Adapter_NumberOfAdapters_Get(ref numberOfAdapters);
            }

            ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);

            if (numberOfAdapters > 0)
            {
                ADLAdapterInfoArray osAdapterInfoData = new ADLAdapterInfoArray();

                if (ADL.ADL_Adapter_AdapterInfo_Get != null)
                {
                    int size = Marshal.SizeOf(osAdapterInfoData);
                    IntPtr adapterBuffer = Marshal.AllocCoTaskMem(size);
                    Marshal.StructureToPtr(osAdapterInfoData, adapterBuffer, false);

                    int adlRet = ADL.ADL_Adapter_AdapterInfo_Get(adapterBuffer, size);
                    if (adlRet == ADL.ADL_SUCCESS)
                    {
                        osAdapterInfoData = (ADLAdapterInfoArray)Marshal.PtrToStructure(adapterBuffer, osAdapterInfoData.GetType());
                        int isActive = 0;

                        for (int i = 0; i < numberOfAdapters; i++)
                        {
                            ADLAdapterInfo adlAdapterInfo = osAdapterInfoData.ADLAdapterInfo[i];

                            if (adlAdapterInfo.DisplayName != displayName && displayName != null)
                            {
                                continue;
                            }

                            int adapterIndex = adlAdapterInfo.AdapterIndex;

                            if (ADL.ADL_Adapter_Active_Get != null)
                            {
                                adlRet = ADL.ADL_Adapter_Active_Get(adlAdapterInfo.AdapterIndex, ref isActive);
                            }

                            if (ADL.ADL_SUCCESS == adlRet)
                            {
                                ADLDisplayInfo oneDisplayInfo = new ADLDisplayInfo();

                                if (ADL.ADL_Display_DisplayInfo_Get != null)
                                {
                                    IntPtr displayBuffer = IntPtr.Zero;

                                    int numberOfDisplays = 0;
                                    adlRet = ADL.ADL_Display_DisplayInfo_Get(adlAdapterInfo.AdapterIndex, ref numberOfDisplays, out displayBuffer, 1);
                                    if (ADL.ADL_SUCCESS == adlRet)
                                    {
                                        List<ADLDisplayInfo> displayInfoData = new List<ADLDisplayInfo>();
                                        for (int j = 0; j < numberOfDisplays; j++)
                                        {
                                            oneDisplayInfo = (ADLDisplayInfo)Marshal.PtrToStructure(new IntPtr(displayBuffer.ToInt64() + j * Marshal.SizeOf(oneDisplayInfo)), oneDisplayInfo.GetType());
                                            displayInfoData.Add(oneDisplayInfo);
                                        }

                                        for (int j = 0; j < numberOfDisplays; j++)
                                        {
                                            ADLDisplayInfo adlDisplayInfo = displayInfoData[j];

                                            handle(adlDisplayInfo, adlAdapterInfo, adapterIndex);
                                        }
                                    }

                                    if (displayBuffer != IntPtr.Zero)
                                    {
                                        Marshal.FreeCoTaskMem(displayBuffer);
                                    }
                                }
                            }
                        }
                    }

                    if (adapterBuffer != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(adapterBuffer);
                    }
                }
            }

            if (ADL.ADL_Main_Control_Destroy != null)
            {
                ADL.ADL_Main_Control_Destroy();
            }
        }
    }
}