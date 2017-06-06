#pragma once

#include <Windows.h>
#include <ShlObj.h> //IShellExtInit & IContextMenu
#include <string>
class LANshareShellExt : public IShellExtInit, public IContextMenu
{
public:

	// Interface IUnknown
	IFACEMETHODIMP QueryInterface(REFIID riid, void **ppv);
	IFACEMETHODIMP_(ULONG) AddRef();
	IFACEMETHODIMP_(ULONG) Release();
	
	// Interface IShellExtInit
	IFACEMETHODIMP Initialize(LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hKeyProgID);
	
	// Interface IContextMenu
	IFACEMETHODIMP QueryContextMenu(HMENU hMenu, UINT indexMenu, UINT idCmdFirst, UINT idCmdLast, UINT uFlags);
	IFACEMETHODIMP InvokeCommand(LPCMINVOKECOMMANDINFO pici);
	IFACEMETHODIMP GetCommandString(UINT_PTR idCommand, UINT uFlags, UINT *pwReserved, LPSTR pszName, UINT cchMax);



	LANshareShellExt();
protected:
	~LANshareShellExt();

private:
	//Reference count of component
	long m_cRef;
	
	// The name of the selected file
	std::wstring *m_szSelectedFile = new std::wstring();

	//The method that handles the "display" of the verb
	void OnVerbDisplayFileName(HWND hWnd);

	PWSTR m_pszMenuText;
	HANDLE m_hMenuBmp;
	PCSTR m_pszVerb;
	PCWSTR m_pwszVerb;
	PCSTR m_pszVerbCanonicalName;
	PCWSTR m_pwszVerbCanonicalName;
	PCSTR m_pszVerbHelpText;
	PCWSTR m_pwszVerbHelpText;
};

