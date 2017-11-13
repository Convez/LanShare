#pragma once

#include <Windows.h>

//
//   FUNCTION: RegisterInprocServer
//
//   PURPOSE: Registra il dll come componente in-process
//
//   PARAMETERS:
//   * pszModule - Cartella in cui si trova il dll
//   * clsid - Class ID del componente (GUID)
//   * pszFriendlyName - Friendly name
//   * pszThreadModel - Tipo di modello di threading per il componente (single/multi)
//
//   NOTE: Crea la chiave HKCR\CLSID\{<CLSID>}\Inprocserver32 nel registro e setta i valori (Predefinito) e Threadingmodel
HRESULT RegisterInprocServer(PCWSTR pszModule, const CLSID& clsid,
	PCWSTR pszFriendlyName, PCWSTR pszThreadModel);

//
//   FUNCTION: UnregisterInprocServer
//
//   PURPOSE: Rimuove il componente in-process dal registro
//
//   PARAMETERS:
//   * clsid - Class ID del componente(GUID)
//
//   NOTE: Elimina la chiave HKCR\CLSID\{<CLSID>} (E di conseguenza le sottochiavi) dal registro.
//
HRESULT UnregisterInprocServer(const CLSID& clsid);

//
//   FUNCTION: RegisterShellExtContextMenuHandler
//
//   PURPOSE: Registra la "voce del context menu" per ogni file
//
//   PARAMETERS:
//   * clsid - Class ID del componente (GUID)
//   * pszFriendlyName - Friendly name
//
//   NOTE: La funzione crea la chiave di registro HKCR\*\shellex\ContextMenuHandlers\{<CLSID>}
//
HRESULT RegisterShellExtContextMenuHandler(const CLSID& clsid, PCWSTR pszFriendlyName);

//
//   FUNCTION: FolderRegisterShellExtContextMenuHandler
//
//   PURPOSE: Registra la "voce del context menu" per ogni cartella
//
//   PARAMETERS:
//   * clsid - Class ID del componente (GUID)
//   * pszFriendlyName - Friendly name
//
//   NOTE: La funzione crea la chiave di registro HKCR\Directory\shellex\ContextMenuHandlers\{<CLSID>}
//
HRESULT FolderRegisterShellExtContextMenuHandler(const CLSID& clsid, PCWSTR pszFriendlyName);

//
//   FUNCTION: UnregisterShellExtContextMenuHandler
//
//   PURPOSE: Rimuove la "voce del context menu" per ogni file
//
//   PARAMETERS:
//   * clsid - Class ID del componente (GUID)
//
//   NOTE: La funzoine rimuove la chiave HKCR\*\shellex\ContextMenuHandlers\{<CLSID>} dal registro
HRESULT UnregisterShellExtContextMenuHandler(const CLSID& clsid);

//
//   FUNCTION: FolderUnregisterShellExtContextMenuHandler
//
//   PURPOSE: Rimuove la "voce del context menu" per ogni cartella
//
//   PARAMETERS:
//   * clsid - Class ID del componente (GUID)
//
//   NOTE: La funzoine rimuove la chiave HKCR\Directory\shellex\ContextMenuHandlers\{<CLSID>} dal registro
HRESULT FolderUnregisterShellExtContextMenuHandler(const CLSID& clsid);