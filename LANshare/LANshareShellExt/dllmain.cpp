// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"
#include "guid.h"
#include "Reg.h"
#include "ClassFactory.h"
#include <Shlwapi.h>


HINSTANCE g_hInst = NULL;
long g_cDllRef = 0;
wchar_t DllDirectory[MAX_PATH];

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	wchar_t currP[MAX_PATH];
	GetModuleFileName(hModule, currP, MAX_PATH);
	PathRemoveFileSpec(currP);
	StrCpy(DllDirectory, currP);

	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		g_hInst = hModule;
		DisableThreadLibraryCalls(hModule);
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, void **ppv)
{
	HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;
	if (IsEqualCLSID(GUID_LANshareShellExt, rclsid))
	{
		hr = E_OUTOFMEMORY;
		ClassFactory *pClassFactory = new ClassFactory();
		if (pClassFactory)
		{
			hr = pClassFactory->QueryInterface(riid, ppv);
			pClassFactory->Release();
		}
	}
	return hr;
}

STDAPI DllCanUnloadNow(void)
{
	return g_cDllRef > 0 ? S_FALSE : S_OK;
}

STDAPI DllRegisterServer(void)
{
	HRESULT hr;
	wchar_t szModule[MAX_PATH];
	if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}
	hr = RegisterInprocServer(szModule, GUID_LANshareShellExt, L"LANshareShellExt.LANshareShellExt Class", L"Apartment");
	if (SUCCEEDED(hr))
	{
		hr = RegisterShellExtContextMenuHandler(GUID_LANshareShellExt, L"LANshareShellExt.LANshareShellExt");
		if(SUCCEEDED(hr))
		{
			FolderRegisterShellExtContextMenuHandler(GUID_LANshareShellExt, L"LANshareShellExt.LANshareShellExt");
		}
	}
	return hr;
}

STDAPI DllUnregisterServer(void)
{
	HRESULT hr = S_OK;
	wchar_t szModule[MAX_PATH];
	if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}
	//Unregister the component
	hr = UnregisterInprocServer(GUID_LANshareShellExt);
	if (SUCCEEDED(hr))
	{
		hr = UnregisterShellExtContextMenuHandler(GUID_LANshareShellExt);
		if(SUCCEEDED(hr))
		{
			hr = FolderUnregisterShellExtContextMenuHandler(GUID_LANshareShellExt);
		}
	}
	return hr;
}
