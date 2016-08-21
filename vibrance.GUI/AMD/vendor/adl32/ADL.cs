

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

namespace vibrance.GUI.AMD.vendor.adl32
{
    public static class Adl
    {
        private static Delegates.AdlMainControlCreate _adlMainControlCreate;
        private static Delegates.AdlMainControlDestroy _adlMainControlDestroy;
        private static Delegates.AdlAdapterNumberOfAdaptersGet _adlAdapterNumberOfAdaptersGet;
        private static Delegates.AdlAdapterAdapterInfoGet _adlAdapterAdapterInfoGet;
        private static Delegates.AdlAdapterActiveGet _adlAdapterActiveGet;
        private static Delegates.AdlDisplayDisplayInfoGet _adlDisplayDisplayInfoGet;
        private static Delegates.AdlDisplayColorSet _adlDisplayColorSet;
        private static Delegates.AdlDisplayColorGet _adlDisplayColorGet;
        private static bool _adlDisplayColorSetCheck;
        private static bool _adlDisplayDisplayInfoGetCheck;
        private static bool _adlAdapterActiveGetCheck;
        private static bool _adlAdapterAdapterInfoGetCheck;
        private static bool _adlAdapterNumberOfAdaptersGetCheck;
        private static bool _adlMainControlDestroyCheck;
        private static bool _adlMainControlCreateCheck;
        private static bool _adlDisplayColorGetCheck;

        public const int AdlDisplayColorBrightness = (1 << 0);
        public const int AdlDisplayColorContrast = (1 << 1);
        public const int AdlDisplayColorSaturation = (1 << 2);
        public const int AdlDisplayColorHue = (1 << 3);
        public const int AdlDisplayColorTemperature = (1 << 4);

        public const int AdlMaxPath = 256;
        public const int AdlMaxAdapters = 40;
        public const int AdlMaxDisplays = 40;
        public const int AdlMaxDevicename = 32;
        public const int AdlSuccess = 0;
        public const int AdlFail = -1;
        public const int AdlDriverOk = 0;
        public const int AdlMaxGlsyncPorts = 8;
        public const int AdlMaxGlsyncPortLeds = 8;
        public const int AdlMaxNumDisplaymodes = 1024;

        public static Delegates.AdlMainMemoryAlloc AdlMainMemoryAlloc = ADL_Main_Memory_Alloc_;
        
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
        
        public static Delegates.AdlMainControlCreate AdlMainControlCreate
        {
            get
            {
                if (!_adlMainControlCreateCheck && null == _adlMainControlCreate)
                {
                    _adlMainControlCreateCheck = true;
                    if (AdlCheckLibrary.IsFunctionValid("ADL_Main_Control_Create"))
                    {
                        _adlMainControlCreate = AdlImport.ADL_Main_Control_Create;
                    }
                }
                return _adlMainControlCreate;
            }
        }
        
        public static Delegates.AdlMainControlDestroy AdlMainControlDestroy
        {
            get
            {
                if (!_adlMainControlDestroyCheck && null == _adlMainControlDestroy)
                {
                    _adlMainControlDestroyCheck = true;
                    if (AdlCheckLibrary.IsFunctionValid("ADL_Main_Control_Destroy"))
                    {
                        _adlMainControlDestroy = AdlImport.ADL_Main_Control_Destroy;
                    }
                }
                return _adlMainControlDestroy;
            }
        }

        public static Delegates.AdlAdapterNumberOfAdaptersGet AdlAdapterNumberOfAdaptersGet
        {
            get
            {
                if (!_adlAdapterNumberOfAdaptersGetCheck && null == _adlAdapterNumberOfAdaptersGet)
                {
                    _adlAdapterNumberOfAdaptersGetCheck = true;
                    if (AdlCheckLibrary.IsFunctionValid("ADL_Adapter_NumberOfAdapters_Get"))
                    {
                        _adlAdapterNumberOfAdaptersGet = AdlImport.ADL_Adapter_NumberOfAdapters_Get;
                    }
                }
                return _adlAdapterNumberOfAdaptersGet;
            }
        }
        
        public static Delegates.AdlAdapterAdapterInfoGet AdlAdapterAdapterInfoGet
        {
            get
            {
                if (!_adlAdapterAdapterInfoGetCheck && null == _adlAdapterAdapterInfoGet)
                {
                    _adlAdapterAdapterInfoGetCheck = true;
                    if (AdlCheckLibrary.IsFunctionValid("ADL_Adapter_AdapterInfo_Get"))
                    {
                        _adlAdapterAdapterInfoGet = AdlImport.ADL_Adapter_AdapterInfo_Get;
                    }
                }
                return _adlAdapterAdapterInfoGet;
            }
        }
        
        public static Delegates.AdlAdapterActiveGet AdlAdapterActiveGet
        {
            get
            {
                if (!_adlAdapterActiveGetCheck && null == _adlAdapterActiveGet)
                {
                    _adlAdapterActiveGetCheck = true;
                    if (AdlCheckLibrary.IsFunctionValid("ADL_Adapter_Active_Get"))
                    {
                        _adlAdapterActiveGet = AdlImport.ADL_Adapter_Active_Get;
                    }
                }
                return _adlAdapterActiveGet;
            }
        }

        public static Delegates.AdlDisplayDisplayInfoGet AdlDisplayDisplayInfoGet
        {
            get
            {
                if (!_adlDisplayDisplayInfoGetCheck && null == _adlDisplayDisplayInfoGet)
                {
                    _adlDisplayDisplayInfoGetCheck = true;
                    if (AdlCheckLibrary.IsFunctionValid("ADL_Display_DisplayInfo_Get"))
                    {
                        _adlDisplayDisplayInfoGet = AdlImport.ADL_Display_DisplayInfo_Get;
                    }
                }
                return _adlDisplayDisplayInfoGet;
            }
        }
        
        public static Delegates.AdlDisplayColorSet AdlDisplayColorSet
        {
            get
            {
                if (!_adlDisplayColorSetCheck && null == _adlDisplayColorSet)
                {
                    _adlDisplayColorSetCheck = true;
                    if (AdlCheckLibrary.IsFunctionValid("ADL_Display_Color_Set"))
                    {
                        _adlDisplayColorSet = AdlImport.ADL_Display_Color_Set;
                    }
                }
                return _adlDisplayColorSet;
            }
        }

        public static Delegates.AdlDisplayColorGet AdlDisplayColorGet
        {
            get
            {
                if (!_adlDisplayColorGetCheck && null == _adlDisplayColorGet)
                {
                    _adlDisplayColorGetCheck = true;
                    if (AdlCheckLibrary.IsFunctionValid("ADL_Display_Color_Get"))
                    {
                        _adlDisplayColorGet = AdlImport.ADL_Display_Color_Get;
                    }
                }
                return _adlDisplayColorGet;
            }
        }
    }
}