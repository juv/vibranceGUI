

/*******************************************************************************
 Copyright(c) 2008 - 2009 Advanced Micro Devices, Inc. All Rights Reserved.
 Copyright (c) 2002 - 2006  ATI Technologies Inc. All Rights Reserved.
 
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
 ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDED BUT NOT LIMITED TO
 THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
 PARTICULAR PURPOSE.
 
 File:        ADL.cs
 
 Purpose:     Implements ADL interface 
 
 Description: Implements some of the methods defined in ADL interface.
              
 ********************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace gui.app.gpucontroller.amd.adl64
{
    public static class ADL
    {
        private static Delegates.ADL_Main_Control_Create ADL_Main_Control_Create_;
        private static Delegates.ADL_Main_Control_Destroy ADL_Main_Control_Destroy_;
        private static Delegates.ADL_Adapter_NumberOfAdapters_Get ADL_Adapter_NumberOfAdapters_Get_;
        private static Delegates.ADL_Adapter_AdapterInfo_Get ADL_Adapter_AdapterInfo_Get_;
        private static Delegates.ADL_Adapter_Active_Get ADL_Adapter_Active_Get_;
        private static Delegates.ADL_Display_DisplayInfo_Get ADL_Display_DisplayInfo_Get_;
        private static Delegates.ADL_Display_Color_Set ADL_Display_Color_Set_;
        private static Delegates.ADL_Display_Color_Get ADL_Display_Color_Get_;
        private static bool ADL_Display_Color_Set_Check;
        private static bool ADL_Display_DisplayInfo_Get_Check;
        private static bool ADL_Adapter_Active_Get_Check;
        private static bool ADL_Adapter_AdapterInfo_Get_Check;
        private static bool ADL_Adapter_NumberOfAdapters_Get_Check;
        private static bool ADL_Main_Control_Destroy_Check;
        private static bool ADL_Main_Control_Create_Check;
        private static bool ADL_Display_Color_Get_Check;

        public const int ADL_DISPLAY_COLOR_BRIGHTNESS = (1 << 0);
        public const int ADL_DISPLAY_COLOR_CONTRAST = (1 << 1);
        public const int ADL_DISPLAY_COLOR_SATURATION = (1 << 2);
        public const int ADL_DISPLAY_COLOR_HUE = (1 << 3);
        public const int ADL_DISPLAY_COLOR_TEMPERATURE = (1 << 4);

        public const int ADL_MAX_PATH = 256;
        public const int ADL_MAX_ADAPTERS = 40;
        public const int ADL_MAX_DISPLAYS = 40;
        public const int ADL_MAX_DEVICENAME = 32;
        public const int ADL_SUCCESS = 0;
        public const int ADL_FAIL = -1;
        public const int ADL_DRIVER_OK = 0;
        public const int ADL_MAX_GLSYNC_PORTS = 8;
        public const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
        public const int ADL_MAX_NUM_DISPLAYMODES = 1024;

        public static Delegates.ADL_Main_Memory_Alloc ADL_Main_Memory_Alloc = ADL_Main_Memory_Alloc_;
        
        private static IntPtr ADL_Main_Memory_Alloc_(int size)
        {
            IntPtr result = Marshal.AllocCoTaskMem(size);
            return result;
        }
        
        public static void ADL_Main_Memory_Free(IntPtr buffer)
        {
            if (IntPtr.Zero != buffer)
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }
        
        public static Delegates.ADL_Main_Control_Create ADL_Main_Control_Create
        {
            get
            {
                if (!ADL_Main_Control_Create_Check && null == ADL_Main_Control_Create_)
                {
                    ADL_Main_Control_Create_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Main_Control_Create"))
                    {
                        ADL_Main_Control_Create_ = ADLImport.ADL_Main_Control_Create;
                    }
                }
                return ADL_Main_Control_Create_;
            }
        }
        
        public static Delegates.ADL_Main_Control_Destroy ADL_Main_Control_Destroy
        {
            get
            {
                if (!ADL_Main_Control_Destroy_Check && null == ADL_Main_Control_Destroy_)
                {
                    ADL_Main_Control_Destroy_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Main_Control_Destroy"))
                    {
                        ADL_Main_Control_Destroy_ = ADLImport.ADL_Main_Control_Destroy;
                    }
                }
                return ADL_Main_Control_Destroy_;
            }
        }

        public static Delegates.ADL_Adapter_NumberOfAdapters_Get ADL_Adapter_NumberOfAdapters_Get
        {
            get
            {
                if (!ADL_Adapter_NumberOfAdapters_Get_Check && null == ADL_Adapter_NumberOfAdapters_Get_)
                {
                    ADL_Adapter_NumberOfAdapters_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Adapter_NumberOfAdapters_Get"))
                    {
                        ADL_Adapter_NumberOfAdapters_Get_ = ADLImport.ADL_Adapter_NumberOfAdapters_Get;
                    }
                }
                return ADL_Adapter_NumberOfAdapters_Get_;
            }
        }
        
        public static Delegates.ADL_Adapter_AdapterInfo_Get ADL_Adapter_AdapterInfo_Get
        {
            get
            {
                if (!ADL_Adapter_AdapterInfo_Get_Check && null == ADL_Adapter_AdapterInfo_Get_)
                {
                    ADL_Adapter_AdapterInfo_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Adapter_AdapterInfo_Get"))
                    {
                        ADL_Adapter_AdapterInfo_Get_ = ADLImport.ADL_Adapter_AdapterInfo_Get;
                    }
                }
                return ADL_Adapter_AdapterInfo_Get_;
            }
        }
        
        public static Delegates.ADL_Adapter_Active_Get ADL_Adapter_Active_Get
        {
            get
            {
                if (!ADL_Adapter_Active_Get_Check && null == ADL_Adapter_Active_Get_)
                {
                    ADL_Adapter_Active_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Adapter_Active_Get"))
                    {
                        ADL_Adapter_Active_Get_ = ADLImport.ADL_Adapter_Active_Get;
                    }
                }
                return ADL_Adapter_Active_Get_;
            }
        }

        public static Delegates.ADL_Display_DisplayInfo_Get ADL_Display_DisplayInfo_Get
        {
            get
            {
                if (!ADL_Display_DisplayInfo_Get_Check && null == ADL_Display_DisplayInfo_Get_)
                {
                    ADL_Display_DisplayInfo_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Display_DisplayInfo_Get"))
                    {
                        ADL_Display_DisplayInfo_Get_ = ADLImport.ADL_Display_DisplayInfo_Get;
                    }
                }
                return ADL_Display_DisplayInfo_Get_;
            }
        }
        
        public static Delegates.ADL_Display_Color_Set ADL_Display_Color_Set
        {
            get
            {
                if (!ADL_Display_Color_Set_Check && null == ADL_Display_Color_Set_)
                {
                    ADL_Display_Color_Set_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Display_Color_Set"))
                    {
                        ADL_Display_Color_Set_ = ADLImport.ADL_Display_Color_Set;
                    }
                }
                return ADL_Display_Color_Set_;
            }
        }

        public static Delegates.ADL_Display_Color_Get ADL_Display_Color_Get
        {
            get
            {
                if (!ADL_Display_Color_Get_Check && null == ADL_Display_Color_Get_)
                {
                    ADL_Display_Color_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Display_Color_Get"))
                    {
                        ADL_Display_Color_Get_ = ADLImport.ADL_Display_Color_Get;
                    }
                }
                return ADL_Display_Color_Get_;
            }
        }
    }
}