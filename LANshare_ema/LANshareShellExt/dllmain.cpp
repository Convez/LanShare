// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"
#include "guid.h"
#include "Reg.h"
#include "ClassFactory.h"
#include <Shlwapi.h>

//Istanza corrente
HINSTANCE g_hInst = NULL;
//Numero riferimenti al dll
long g_cDllRef = 0;
//Path del dll
wchar_t DllDirectory[MAX_PATH];

//Prima funzione chiamata al momento dell'attivazione del dll. Setta variabili globali
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

//
//   FUNCTION: DllGetClassObject
//
//   PURPOSE: Richiama classFactory per creazione oggetto LANshareShellExt
//
//   PARAMETERS:
//   * rclsid - GUID usata per chiamare il dll. Serve ad associare il codice corretto
//   * riid - Identificatore al chiamante, usato per comunicare con l'oggetto.
//   * ppv - Puntatore all'interfaccia richeista dall'riid. (Parametro out)
//			Se la richiesta fallisce sarà NULL
//
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

//Controlla se è possibile liberare la memoria dal dll
//Si può liberare se il suo reference count è a zero (nessuno lo sta usando)
STDAPI DllCanUnloadNow(void)
{
	return g_cDllRef > 0 ? S_FALSE : S_OK;
}

//Chiamata da Regsvr32.exe in fase di registrazione del dll. Richiama le funzioni che scrivono le chiavi di registro
STDAPI DllRegisterServer(void)
{
	HRESULT hr;
	wchar_t szModule[MAX_PATH];
	//Leggi cartella in cui si trova il dll
	if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}
	//Registra il componente in-process (chiamerà la classe LANshareShellExt) Apartment = single thread
	hr = RegisterInprocServer(szModule, GUID_LANshareShellExt, L"LANshareShellExt.LANshareShellExt Class", L"Apartment");
	//Aggiungi voce dei menu
	if (SUCCEEDED(hr))
	{
		hr = RegisterShellExtContextMenuHandler(GUID_LANshareShellExt, L"LANshareShellExt.LANshareShellExt");
		if(SUCCEEDED(hr))
		{
			hr=FolderRegisterShellExtContextMenuHandler(GUID_LANshareShellExt, L"LANshareShellExt.LANshareShellExt");
		}
	}
	return hr;
}
//Chiamata da Regsvr.exe in fase di rimozione del dll. Richiama le funzioni che eliminano le chiavi di registro
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = S_OK;
	wchar_t szModule[MAX_PATH];
	if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}
	//Rimuove il Componente in-process
	hr = UnregisterInprocServer(GUID_LANshareShellExt);
	//Rimuove voce menu contestuale
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
