// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "powerbase.h"

#pragma comment(lib, "Powrprof.lib")

// -----------------------------------------------------------------------------------------------

extern "C" __declspec(dllexport) DWORD monitorOffTimeout()
{
    SYSTEM_POWER_POLICY spp;

    CallNtPowerInformation(POWER_INFORMATION_LEVEL::SystemPowerPolicyCurrent, nullptr, 0, &spp, sizeof(spp));

    return spp.VideoTimeout;
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
