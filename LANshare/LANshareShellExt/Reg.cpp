#include "stdafx.h"
#include "Reg.h"
#include <strsafe.h>
#include <combaseapi.h>


#pragma region Appoggio
//
//   FUNCTION: SetHKCRRegistryKeyAndValue
//
//   PURPOSE: Crea chiave di registro e imposta il valore
//
//   PARAMETERS:
//   * pszSubKey - chiave di registro da creare sotto HKCR, se non esiste
//   * pszValueName - valore del registro da impostare. se NULL imposta il valore Predefinito
//   * pszData - string da impostare nel valore
//
//   RETURN VALUE: 
//   S_OK se la funzione ha successo, un codice d'errore altrimenti
HRESULT SetHKCRRegistryKeyAndValue(PCWSTR pszSubKey, PCWSTR pszValueName,
	PCWSTR pszData)
{
	HRESULT hr;
	HKEY hKey = NULL;

	// Crea chiave di registro. Se esiste già viene semplicemente aperta
	hr = HRESULT_FROM_WIN32(RegCreateKeyEx(HKEY_CLASSES_ROOT, pszSubKey, 0,
		NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL));

	if (SUCCEEDED(hr))
	{
		if (pszData != NULL)
		{
			//Imposta il valore della chiave
			DWORD cbData = lstrlen(pszData) * sizeof(*pszData);
			hr = HRESULT_FROM_WIN32(RegSetValueEx(hKey, pszValueName, 0,
				REG_SZ, reinterpret_cast<const BYTE *>(pszData), cbData));
		}

		RegCloseKey(hKey);
	}

	return hr;
}


//
//   FUNCTION: GetHKCRRegistryKeyAndValue
//
//   PURPOSE: Apre la chiave di registro e legge il valore
//
//   PARAMETERS:
//   * pszSubKey - chiave di registro. se non esiste ritorna errore
//   * pszValueName - valore da leggere. se NULL legge il Predefinito
//   * pszData - buffer che riceve il dato letto. (paramentro OUT)
//   * cbData - dimensione del buffer in bytes
//
//   RETURN VALUE: S_OK in caso di successo, un valore d'errore altrimenti
// 
HRESULT GetHKCRRegistryKeyAndValue(PCWSTR pszSubKey, PCWSTR pszValueName,
	PWSTR pszData, DWORD cbData)
{
	HRESULT hr;
	HKEY hKey = NULL;

	// Prova ad aprire la chiave
	hr = HRESULT_FROM_WIN32(RegOpenKeyEx(HKEY_CLASSES_ROOT, pszSubKey, 0,
		KEY_READ, &hKey));

	if (SUCCEEDED(hr))
	{
		// Legge il dato associato al valore
		hr = HRESULT_FROM_WIN32(RegQueryValueEx(hKey, pszValueName, NULL,
			NULL, reinterpret_cast<LPBYTE>(pszData), &cbData));

		RegCloseKey(hKey);
	}

	return hr;
}

#pragma endregion


HRESULT RegisterInprocServer(PCWSTR pszModule, const CLSID& clsid,
	PCWSTR pszFriendlyName, PCWSTR pszThreadModel)
{
	if (pszModule == NULL || pszThreadModel == NULL)
	{
		return E_INVALIDARG;
	}

	HRESULT hr;

	wchar_t szCLSID[MAX_PATH];
	StringFromGUID2(clsid, szCLSID, ARRAYSIZE(szCLSID));

	wchar_t szSubkey[MAX_PATH];

	// Create the HKCR\CLSID\{<CLSID>} key.
	hr = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey), L"CLSID\\%s", szCLSID);
	if (SUCCEEDED(hr))
	{
		hr = SetHKCRRegistryKeyAndValue(szSubkey, NULL, pszFriendlyName);

		// Create the HKCR\CLSID\{<CLSID>}\InprocServer32 key.
		if (SUCCEEDED(hr))
		{
			hr = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey),
				L"CLSID\\%s\\InprocServer32", szCLSID);
			if (SUCCEEDED(hr))
			{
				// Set the default value of the InprocServer32 key to the 
				// path of the COM module.
				hr = SetHKCRRegistryKeyAndValue(szSubkey, NULL, pszModule);
				if (SUCCEEDED(hr))
				{
					// Set the threading model of the component.
					hr = SetHKCRRegistryKeyAndValue(szSubkey,
						L"ThreadingModel", pszThreadModel);
				}
			}
		}
	}

	return hr;
}


HRESULT UnregisterInprocServer(const CLSID& clsid)
{
	HRESULT hr = S_OK;

	wchar_t szCLSID[MAX_PATH];
	StringFromGUID2(clsid, szCLSID, ARRAYSIZE(szCLSID));

	wchar_t szSubkey[MAX_PATH];

	// Delete the HKCR\CLSID\{<CLSID>} key.
	hr = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey), L"CLSID\\%s", szCLSID);
	if (SUCCEEDED(hr))
	{
		hr = HRESULT_FROM_WIN32(RegDeleteTree(HKEY_CLASSES_ROOT, szSubkey));
	}

	return hr;
}


HRESULT RegisterShellExtContextMenuHandler(const CLSID& clsid, PCWSTR pszFriendlyName)
{
	PCWSTR pszFileType;
	
	HRESULT hr;

	wchar_t szCLSID[MAX_PATH];
	StringFromGUID2(clsid, szCLSID, ARRAYSIZE(szCLSID));

	wchar_t szSubkey[MAX_PATH];
	pszFileType = L"*";
	// Create the key HKCR\<File Type>\shellex\ContextMenuHandlers\{<CLSID>}
	hr = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey),
		L"%s\\shellex\\ContextMenuHandlers\\%s", pszFileType, szCLSID);
	if (SUCCEEDED(hr))
	{
		// Set the default value of the key.
		hr = SetHKCRRegistryKeyAndValue(szSubkey, NULL, pszFriendlyName);
	}
	wchar_t szSubkey1[MAX_PATH];
	pszFileType = L"lnkfile";
	hr = StringCchPrintf(szSubkey1, ARRAYSIZE(szSubkey1),L"%s\\shellex\\ContextMenuHandlers\\%s", pszFileType, szCLSID);
	if(SUCCEEDED(hr))
	{
		hr = SetHKCRRegistryKeyAndValue(szSubkey1, NULL, pszFriendlyName);
	}

	return hr;
}

HRESULT FolderRegisterShellExtContextMenuHandler(const CLSID& clsid, PCWSTR pszFriendlyName)
{
	PCWSTR pszFileType;

	HRESULT hr;

	wchar_t szCLSID[MAX_PATH];
	StringFromGUID2(clsid, szCLSID, ARRAYSIZE(szCLSID));

	wchar_t szSubkey[MAX_PATH];
	pszFileType = L"Directory";
	// Create the key HKCR\<File Type>\shellex\ContextMenuHandlers\{<CLSID>}
	hr = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey),
		L"%s\\shellex\\ContextMenuHandlers\\%s", pszFileType, szCLSID);
	if (SUCCEEDED(hr))
	{
		// Set the default value of the key.
		hr = SetHKCRRegistryKeyAndValue(szSubkey, NULL, pszFriendlyName);
	}

	return hr;
}


HRESULT UnregisterShellExtContextMenuHandler( const CLSID& clsid)
{
	PCWSTR pszFileType;

	HRESULT hr;

	wchar_t szCLSID[MAX_PATH];
	StringFromGUID2(clsid, szCLSID, ARRAYSIZE(szCLSID));

	wchar_t szSubkey[MAX_PATH];

	pszFileType = L"*";
	// Remove the HKCR\<File Type>\shellex\ContextMenuHandlers\{<CLSID>} key.
	hr = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey),
		L"%s\\shellex\\ContextMenuHandlers\\%s", pszFileType, szCLSID);
	if (SUCCEEDED(hr))
	{
		hr = HRESULT_FROM_WIN32(RegDeleteTree(HKEY_CLASSES_ROOT, szSubkey));
	}

	wchar_t szSubkey1[MAX_PATH];
	pszFileType = L"lnkfile";
	// Remove the HKCR\<File Type>\shellex\ContextMenuHandlers\{<CLSID>} key.
	hr = StringCchPrintf(szSubkey1, ARRAYSIZE(szSubkey1),
		L"%s\\shellex\\ContextMenuHandlers\\%s", pszFileType, szCLSID);
	if (SUCCEEDED(hr))
	{
		hr = HRESULT_FROM_WIN32(RegDeleteTree(HKEY_CLASSES_ROOT, szSubkey1));
	}
	return hr;
}

HRESULT FolderUnregisterShellExtContextMenuHandler(const CLSID& clsid)
{
	PCWSTR pszFileType;

	HRESULT hr;

	wchar_t szCLSID[MAX_PATH];
	StringFromGUID2(clsid, szCLSID, ARRAYSIZE(szCLSID));

	wchar_t szSubkey[MAX_PATH];

	pszFileType = L"Directory";
	// Remove the HKCR\<File Type>\shellex\ContextMenuHandlers\{<CLSID>} key.
	hr = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey),
		L"%s\\shellex\\ContextMenuHandlers\\%s", pszFileType, szCLSID);
	if (SUCCEEDED(hr))
	{
		hr = HRESULT_FROM_WIN32(RegDeleteTree(HKEY_CLASSES_ROOT, szSubkey));
	}

	return hr;
}