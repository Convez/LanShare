#include "stdafx.h"
#include "LANshareShellExt.h"
#include "resource1.h"
#include <strsafe.h>
#include <Shlwapi.h>
#include <shellapi.h>
#include <stdlib.h>
#include <string>
#include <sstream>
#pragma comment(lib,"shlwapi.lib")

extern HINSTANCE g_hInst;
extern long g_cDllRef;
extern wchar_t DllDirectory[MAX_PATH];


#define IDM_DISPLAY	0	//Command's identifier offset

LANshareShellExt::LANshareShellExt() : m_cRef(1),
	m_pszMenuText(L"Share with LANshare"),
	m_pszVerb("cpplanshare"),
	m_pwszVerb(L"cpplanshare"),
	m_pszVerbCanonicalName("CppLanShare"),
	m_pwszVerbCanonicalName(L"CppLanShare"),
	m_pszVerbHelpText("Open with LANshare"),
	m_pwszVerbHelpText(L"Open with LANshare")
{
	InterlockedIncrement(&g_cDllRef);
	//Carica icona: Un bitmap 16x16 dalle resources
	m_hMenuBmp = LoadImage(g_hInst, MAKEINTRESOURCE(IDB_BITMAP3), IMAGE_BITMAP, 0, 0, LR_DEFAULTSIZE | LR_LOADTRANSPARENT);
}


LANshareShellExt::~LANshareShellExt()
{
	if(m_hMenuBmp)
	{
		DeleteObject(m_hMenuBmp);
		m_hMenuBmp = NULL;
	}
	InterlockedDecrement(&g_cDllRef);
}

void LANshareShellExt::OnVerbCallLANshare(HWND hWnd)
{
	wchar_t szMessage[MAX_PATH];
	STARTUPINFO si;
	PROCESS_INFORMATION pi;

	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));
	std::wstringstream ss = std::wstringstream();
	ss << DllDirectory << L"\\LANshare.exe\0";

	std::wstring s = std::wstring();
	std::getline(ss,s);
	
	if (!CreateProcess(
		&s[0],												//Path to exe
		&(*m_szSelectedFile)[0],												//Startup Arguments
		NULL,
		NULL,
		FALSE,
		DETACHED_PROCESS,
		NULL,
		NULL,
		&si,
		&pi)) {
		DWORD error = GetLastError();
	}
	if(pi.hThread)
	{
		CloseHandle(pi.hThread);
	}
	if(pi.hProcess)
	{
		CloseHandle(pi.hProcess);
	}
}

#pragma region IUnknown
IFACEMETHODIMP LANshareShellExt::QueryInterface(REFIID riid, void ** ppv)
{
	static const QITAB qit[] ={
		QITABENT(LANshareShellExt,IContextMenu),
		QITABENT(LANshareShellExt,IShellExtInit),
		{0},
	};
	return QISearch(this, qit, riid, ppv);
}

IFACEMETHODIMP_(ULONG) LANshareShellExt::AddRef()
{
	return InterlockedIncrement(&m_cRef);
}

IFACEMETHODIMP_(ULONG) LANshareShellExt::Release()
{
	ULONG cRef = InterlockedDecrement(&m_cRef);
	if(cRef==0)
	{
		delete this;
	}
	return cRef;
}

#pragma endregion

#pragma region IShellExtInit

//Initialize context menu handler

IFACEMETHODIMP LANshareShellExt::Initialize(LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hKeyProgID)
{
	if(NULL == pDataObj)
	{
		return E_INVALIDARG;
	}
	HRESULT hr = E_FAIL;
	FORMATETC fe = { CF_HDROP, NULL, DVASPECT_CONTENT, -1 , TYMED_HGLOBAL };
	STGMEDIUM stm;

	//The pDataObj pointer contains the objects being acted upon
	if(SUCCEEDED(pDataObj->GetData(&fe,&stm)))
	{
		//Get handle
		HDROP hDrop = static_cast<HDROP>(GlobalLock(stm.hGlobal));
		if(hDrop!=NULL)
		{
			//Get how many files are involved in the operation
			UINT nFiles = DragQueryFile(hDrop, 0xFFFFFFFF, NULL, 0);
			std::wstringstream ss = std::wstringstream();
			wchar_t tmp[MAX_PATH];
			if (DragQueryFile(hDrop, 0, tmp, ARRAYSIZE(tmp)) != 0)
			{
				if (PathRemoveFileSpec(tmp) != 0)
				{
					ss << L"\"" << "rumore" << L" \" ";
					ss << L"\"" << tmp << L"\" ";
				}
			}
			for(int i=0;i<nFiles;i++)
			{
				//get the path of the file
				if (DragQueryFile(hDrop, i, tmp, ARRAYSIZE(tmp))!=0)
				{

					ss <<L"\"" << PathFindFileName(tmp) <<L"\" ";
					hr = S_OK;
				}
			}
			std::getline( ss , (*m_szSelectedFile));
			GlobalUnlock(stm.hGlobal);
		}
		ReleaseStgMedium(&stm);
	}

	//Context menu handler is not displayed if is not returned S_OK
	return hr;
}

#pragma endregion

#pragma region IContextMenu

//The shell calls this function to allow the context menu handler to add
// its menu items to the menu.
//IT passes the hmenu handle. The indexmenu parameter is set to the index to be used for the first menu item that it is to be added
IFACEMETHODIMP LANshareShellExt::QueryContextMenu(HMENU hMenu, UINT indexMenu, UINT idCmdFirst, UINT idCmdLast, UINT uFlags)
{
	if(CMF_VERBSONLY & uFlags)
	{
		return MAKE_HRESULT(SEVERITY_SUCCESS, 0, USHORT(0));
	}
	if(CMF_DEFAULTONLY&uFlags)
	{
		return MAKE_HRESULT(SEVERITY_SUCCESS, 0, USHORT(0));
	}
	MENUITEMINFO mii = { sizeof(mii) };
	mii.fMask = MIIM_BITMAP | MIIM_STRING | MIIM_FTYPE | MIIM_ID | MIIM_STATE;
	mii.wID = idCmdFirst + IDM_DISPLAY;
	mii.fType = MFT_STRING;
	mii.dwTypeData = m_pszMenuText;
	mii.fState = MFS_ENABLED;
	mii.hbmpItem = static_cast<HBITMAP>(m_hMenuBmp);
	if(!InsertMenuItem(hMenu,indexMenu,TRUE,&mii))
	{
		return HRESULT_FROM_WIN32(GetLastError());
	}

	return MAKE_HRESULT(SEVERITY_SUCCESS, 0, USHORT(IDM_DISPLAY + 1));
}
//Called when the user clicks on the menu item

IFACEMETHODIMP LANshareShellExt::InvokeCommand(LPCMINVOKECOMMANDINFO pici)
{
	bool fUnicode = FALSE;
	//Check if unicode strings can be passed
	//The strcture CMINVOKECOMMANDINFOEX has an additional parameter so we can check the size
	if(pici->cbSize == sizeof(CMINVOKECOMMANDINFOEX))
	{
		if(pici->fMask & CMIC_MASK_UNICODE)
		{
			fUnicode = true;
		}
	}
	//check if verb command or offset command

	if(!fUnicode && HIWORD(pici->hwnd))
	{
		//verb. IS it supported by this extension?
		if(StrCmpIA(pici->lpVerb,m_pszVerb)==0)
		{
			OnVerbCallLANshare(pici->hwnd);
		}else
		{
			return E_FAIL;
		}
	}else if (fUnicode && HIWORD(((CMINVOKECOMMANDINFOEX*)pici)->lpVerbW))
	{
		if(StrCmpIW(((CMINVOKECOMMANDINFOEX*)pici)->lpVerbW,m_pwszVerb)==0)
		{
			OnVerbCallLANshare(pici->hwnd);
		}else
		{
			return E_FAIL;
		}
	}else
	{
		if(LOWORD(pici->lpVerb)==IDM_DISPLAY)
		{
			OnVerbCallLANshare(pici->hwnd);
		}
	}
	return S_OK;

}

//Called when is requested the command description
IFACEMETHODIMP LANshareShellExt::GetCommandString(UINT_PTR idCommand, UINT uFlags, UINT * pwReserved, LPSTR pszName, UINT cchMax)
{
	HRESULT hr = E_INVALIDARG;
	if(idCommand == IDM_DISPLAY)
	{
		switch (uFlags)
		{
		case GCS_HELPTEXT:
			hr = StringCchCopy(reinterpret_cast<PWSTR>(pszName), cchMax, m_pwszVerbHelpText);
			break;
		case GCS_VERBW:
			hr = StringCchCopy(reinterpret_cast<PWSTR>(pszName), cchMax, m_pwszVerbCanonicalName);
			break;
		default:
			hr = S_OK;
		}
	}
	return hr;
}

#pragma endregion
