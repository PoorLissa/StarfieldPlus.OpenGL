// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "powerbase.h"

#pragma comment(lib, "Powrprof.lib")

// -----------------------------------------------------------------------------------------------

/*
    NTSTATUS CallNtPowerInformation(
      [in]  POWER_INFORMATION_LEVEL InformationLevel,
      [in]  PVOID                   InputBuffer,
      [in]  ULONG                   InputBufferLength,
      [out] PVOID                   OutputBuffer,
      [in]  ULONG                   OutputBufferLength
    );
*/

extern "C" __declspec(dllexport) DWORD systemExecutionState()
{
    SYSTEM_POWER_POLICY spp;

    CallNtPowerInformation(POWER_INFORMATION_LEVEL::SystemExecutionState, nullptr, 0, &spp, sizeof(spp));

    return spp.VideoTimeout;
}

extern "C" __declspec(dllexport) DWORD monitorOffTimeout()
{
    SYSTEM_POWER_POLICY spp;

    CallNtPowerInformation(POWER_INFORMATION_LEVEL::SystemPowerPolicyCurrent, nullptr, 0, &spp, sizeof(spp));

    return spp.VideoTimeout;
}

// https://learn.microsoft.com/en-us/windows/win32/power/system-power-information-str
// Note that this structure definition was accidentally omitted from WinNT.h.
// This error will be corrected in the future.
// In the meantime, to compile your application, include the structure definition contained in this topic in your source code
typedef struct _SYSTEM_POWER_INFORMATION {
    ULONG MaxIdlenessAllowed;
    ULONG Idleness;
    ULONG TimeRemaining;
    UCHAR CoolingMode;
} SYSTEM_POWER_INFORMATION, * PSYSTEM_POWER_INFORMATION;

extern "C" __declspec(dllexport) ULONG systemSleepTimeout()
{
    SYSTEM_POWER_INFORMATION spi;

    CallNtPowerInformation(POWER_INFORMATION_LEVEL::SystemPowerInformation, nullptr, 0, &spi, sizeof(spi));

    return spi.TimeRemaining;
}

// -----------------------------------------------------------------------------------------------

BOOL APIENTRY DllMain( HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
        case DLL_PROCESS_ATTACH:
        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
        case DLL_PROCESS_DETACH:
            break;
    }

    return TRUE;
}

// -----------------------------------------------------------------------------------------------
