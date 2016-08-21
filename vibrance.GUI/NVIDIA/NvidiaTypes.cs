using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace vibrance.GUI.NVIDIA
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NvDisplayDvcInfo
    {
        public uint version;
        public int currentLevel;
        public int minLevel;
        public int maxLevel;
        public NvDisplayDvcInfo(uint version, int currentLevel, int minLevel, int maxLevel)
        {
            this.version = version;
            this.currentLevel = currentLevel;
            this.minLevel = minLevel;
            this.maxLevel = maxLevel;
        }
    }

    enum NvApiStatus
    {
        NvapiOk = 0, NvapiError = -1, NvapiLibraryNotFound = -2, NvapiNoImplementation = -3,
        NvapiApiNotInitialized = -4, NvapiInvalidArgument = -5, NvapiNvidiaDeviceNotFound = -6, NvapiEndEnumeration = -7,
        NvapiInvalidHandle = -8, NvapiIncompatibleStructVersion = -9, NvapiHandleInvalidated = -10, NvapiOpenglContextNotCurrent = -11,
        NvapiInvalidPointer = -14, NvapiNoGlExpert = -12, NvapiInstrumentationDisabled = -13, NvapiNoGlNsight = -15,
        NvapiExpectedLogicalGpuHandle = -100, NvapiExpectedPhysicalGpuHandle = -101, NvapiExpectedDisplayHandle = -102, NvapiInvalidCombination = -103,
        NvapiNotSupported = -104, NvapiPortidNotFound = -105, NvapiExpectedUnattachedDisplayHandle = -106, NvapiInvalidPerfLevel = -107,
        NvapiDeviceBusy = -108, NvapiNvPersistFileNotFound = -109, NvapiPersistDataNotFound = -110, NvapiExpectedTvDisplay = -111,
        NvapiExpectedTvDisplayOnDconnector = -112, NvapiNoActiveSliTopology = -113, NvapiSliRenderingModeNotallowed = -114, NvapiExpectedDigitalFlatPanel = -115,
        NvapiArgumentExceedMaxSize = -116, NvapiDeviceSwitchingNotAllowed = -117, NvapiTestingClocksNotSupported = -118, NvapiUnknownUnderscanConfig = -119,
        NvapiTimeoutReconfiguringGpuTopo = -120, NvapiDataNotFound = -121, NvapiExpectedAnalogDisplay = -122, NvapiNoVidlink = -123,
        NvapiRequiresReboot = -124, NvapiInvalidHybridMode = -125, NvapiMixedTargetTypes = -126, NvapiSyswow64NotSupported = -127,
        NvapiImplicitSetGpuTopologyChangeNotAllowed = -128, NvapiRequestUserToCloseNonMigratableApps = -129, NvapiOutOfMemory = -130, NvapiWasStillDrawing = -131,
        NvapiFileNotFound = -132, NvapiTooManyUniqueStateObjects = -133, NvapiInvalidCall = -134, NvapiD3D101LibraryNotFound = -135,
        NvapiFunctionNotFound = -136, NvapiInvalidUserPrivilege = -137, NvapiExpectedNonPrimaryDisplayHandle = -138, NvapiExpectedComputeGpuHandle = -139,
        NvapiStereoNotInitialized = -140, NvapiStereoRegistryAccessFailed = -141, NvapiStereoRegistryProfileTypeNotSupported = -142, NvapiStereoRegistryValueNotSupported = -143,
        NvapiStereoNotEnabled = -144, NvapiStereoNotTurnedOn = -145, NvapiStereoInvalidDeviceInterface = -146, NvapiStereoParameterOutOfRange = -147,
        NvapiStereoFrustumAdjustModeNotSupported = -148, NvapiTopoNotPossible = -149, NvapiModeChangeFailed = -150, NvapiD3D11LibraryNotFound = -151,
        NvapiInvalidAddress = -152, NvapiStringTooSmall = -153, NvapiMatchingDeviceNotFound = -154, NvapiDriverRunning = -155,
        NvapiDriverNotrunning = -156, NvapiErrorDriverReloadRequired = -157, NvapiSetNotAllowed = -158, NvapiAdvancedDisplayTopologyRequired = -159,
        NvapiSettingNotFound = -160, NvapiSettingSizeTooLarge = -161, NvapiTooManySettingsInProfile = -162, NvapiProfileNotFound = -163,
        NvapiProfileNameInUse = -164, NvapiProfileNameEmpty = -165, NvapiExecutableNotFound = -166, NvapiExecutableAlreadyInUse = -167,
        NvapiDatatypeMismatch = -168, NvapiProfileRemoved = -169, NvapiUnregisteredResource = -170, NvapiIdOutOfRange = -171,
        NvapiDisplayconfigValidationFailed = -172, NvapiDpmstChanged = -173, NvapiInsufficientBuffer = -174, NvapiAccessDenied = -175,
        NvapiMosaicNotActive = -176, NvapiShareResourceRelocated = -177, NvapiRequestUserToDisableDwm = -178, NvapiD3DDeviceLost = -179,
        NvapiInvalidConfiguration = -180, NvapiStereoHandshakeNotDone = -181, NvapiExecutablePathIsAmbiguous = -182, NvapiDefaultStereoProfileIsNotDefined = -183,
        NvapiDefaultStereoProfileDoesNotExist = -184, NvapiClusterAlreadyExists = -185, NvapiDpmstDisplayIdExpected = -186, NvapiInvalidDisplayId = -187,
        NvapiStreamIsOutOfSync = -188, NvapiIncompatibleAudioDriver = -189, NvapiValueAlreadySet = -190, NvapiTimeout = -191,
        NvapiGpuWorkstationFeatureIncomplete = -192, NvapiStereoInitActivationNotDone = -193, NvapiSyncNotActive = -194, NvapiSyncMasterNotFound = -195,
        NvapiInvalidSyncTopology = -196
    };

    enum NvSystemType
    {
        NvSystemTypeUnknown = 0,
        NvSystemTypeLaptop = 1,
        NvSystemTypeDesktop = 2
    };
}
