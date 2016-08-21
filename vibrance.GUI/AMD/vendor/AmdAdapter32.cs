using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using vibrance.GUI.AMD.vendor.adl32;

namespace vibrance.GUI.AMD.vendor
{
    public class AmdAdapter32 : IAmdAdapter
    {
        private List<Display> displays;
        private Disposer disposer;

        public void Init()
        {
            displays = new List<Display>();
            disposer = new Disposer();
            
            int numberOfAdapters = 0;

            Adl.AdlMainControlCreate(Adl.AdlMainMemoryAlloc, 1);

            if (Adl.AdlAdapterNumberOfAdaptersGet != null)
            {
                Adl.AdlAdapterNumberOfAdaptersGet(ref numberOfAdapters);
            }

            Adl.AdlMainControlCreate(Adl.AdlMainMemoryAlloc, 1);

            if (numberOfAdapters > 0)
            {
                AdlAdapterInfoArray osAdapterInfoData = new AdlAdapterInfoArray();

                if (Adl.AdlAdapterAdapterInfoGet != null)
                {
                    int size = Marshal.SizeOf(osAdapterInfoData);
                    IntPtr adapterBuffer = Marshal.AllocCoTaskMem(size);
                    Marshal.StructureToPtr(osAdapterInfoData, adapterBuffer, false);

                    int adlRet = Adl.AdlAdapterAdapterInfoGet(adapterBuffer, size);
                    if (adlRet == Adl.AdlSuccess)
                    {
                        osAdapterInfoData = (AdlAdapterInfoArray)Marshal.PtrToStructure(adapterBuffer, osAdapterInfoData.GetType());
                        int isActive = 0;

                        for (int i = 0; i < numberOfAdapters; i++)
                        {
                            AdlAdapterInfo adlAdapterInfo = osAdapterInfoData.ADLAdapterInfo[i];

                            int adapterIndex = adlAdapterInfo.AdapterIndex;

                            if (Adl.AdlAdapterActiveGet != null)
                            {
                                adlRet = Adl.AdlAdapterActiveGet(adlAdapterInfo.AdapterIndex, ref isActive);
                            }

                            if (Adl.AdlSuccess == adlRet)
                            {
                                AdlDisplayInfo oneDisplayInfo = new AdlDisplayInfo();

                                if (Adl.AdlDisplayDisplayInfoGet != null)
                                {
                                    IntPtr displayBuffer = IntPtr.Zero;

                                    int numberOfDisplays = 0;
                                    adlRet = Adl.AdlDisplayDisplayInfoGet(adlAdapterInfo.AdapterIndex, ref numberOfDisplays, out displayBuffer, 1);
                                    if (Adl.AdlSuccess == adlRet)
                                    {
                                        List<AdlDisplayInfo> displayInfoData = new List<AdlDisplayInfo>();
                                        for (int j = 0; j < numberOfDisplays; j++)
                                        {
                                            oneDisplayInfo = (AdlDisplayInfo)Marshal.PtrToStructure(new IntPtr(displayBuffer.ToInt64() + j * Marshal.SizeOf(oneDisplayInfo)), oneDisplayInfo.GetType());
                                            displayInfoData.Add(oneDisplayInfo);
                                        }

                                        for (int j = 0; j < numberOfDisplays; j++)
                                        {
                                            AdlDisplayInfo adlDisplayInfo = displayInfoData[j];

                                            if (adlDisplayInfo.DisplayID.DisplayLogicalAdapterIndex == -1)
                                            {
                                                continue;
                                            }

                                            displays.Add(new Display
                                            {
                                                DisplayInfo = adlDisplayInfo,
                                                AdapterInfo = adlAdapterInfo,
                                                Index = adapterIndex,
                                            });
                                        }
                                    }

                                    disposer.DisplayBufferList.Add(displayBuffer);
                                }
                            }
                        }
                    }

                    disposer.AdapterBuffer = adapterBuffer;
                }
            }
        }

        public bool IsAvailable()
        {
            if (Adl.AdlMainControlCreate != null)
            {
                if (Adl.AdlSuccess == Adl.AdlMainControlCreate(Adl.AdlMainMemoryAlloc, 1))
                {
                    if (Adl.AdlMainControlDestroy != null)
                    {
                        Adl.AdlMainControlDestroy();
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
                if (adapterIsAssociatedWithDisplay && (adlAdapterInfo.DisplayName == displayName || displayName == null))
                {
                    Adl.AdlDisplayColorSet(
                        adapterIndex,
                        infoValue,
                        Adl.AdlDisplayColorSaturation,
                        vibranceLevel);
                }
            });
        }

        private void SetSaturation(Action<AdlDisplayInfo, AdlAdapterInfo, int> handle)
        {
            foreach (var display in displays)
            {
                handle(display.DisplayInfo, display.AdapterInfo, display.Index);
            }
        }

        public void Dispose()
        {
            disposer?.Dispose();
        }

        class Display
        {
            public AdlDisplayInfo DisplayInfo { get; set; }

            public AdlAdapterInfo AdapterInfo { get; set; }

            public int Index { get; set; }
        }

        class Disposer : IDisposable
        {
            public Disposer()
            {
                DisplayBufferList = new List<IntPtr>();
            }

            public List<IntPtr> DisplayBufferList { get; set; }
            public IntPtr AdapterBuffer { get; set; }

            public void Dispose()
            {
                foreach (var intPtr in DisplayBufferList)
                {
                    if (intPtr != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(intPtr);
                    }
                }

                if (AdapterBuffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(AdapterBuffer);
                }

                if (Adl.AdlMainControlDestroy != null)
                {
                    Adl.AdlMainControlDestroy();
                }
            }
        }
    }
}