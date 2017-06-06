#include "stdafx.h"
#include "ClassFactory.h"
#include "LANshareShellExt.h"
#include <new>
#include <Shlwapi.h>
extern long g_cDllRef;



ClassFactory::ClassFactory() : m_cRef(1)
{
	InterlockedIncrement(&g_cDllRef);
}


ClassFactory::~ClassFactory()
{
	InterlockedDecrement(&g_cDllRef);
}

//Interface IUnknown
IFACEMETHODIMP ClassFactory::QueryInterface(REFIID riid, void ** ppv)
{
	static const QITAB qit[] = {
		QITABENT(ClassFactory,IClassFactory),
		{ 0 },
	};
	return QISearch(this, qit, riid, ppv);
}

IFACEMETHODIMP_(ULONG) ClassFactory::AddRef()
{
	return InterlockedIncrement(&m_cRef);
}

IFACEMETHODIMP_(ULONG) ClassFactory::Release()
{
	ULONG cRef = InterlockedDecrement(&m_cRef);
	if (cRef == 0)
	{
		delete this;
	}
	return cRef;
}


//Interface IClassFactory

IFACEMETHODIMP ClassFactory::CreateInstance(IUnknown * pUnkOther, REFIID riid, void ** ppv)
{
	HRESULT hr = CLASS_E_NOAGGREGATION;
	

	if(pUnkOther == NULL)
	{
		hr = E_OUTOFMEMORY;

		//Create the COM component
		LANshareShellExt *pExt = new (std::nothrow) LANshareShellExt();
		if(pExt)
		{
			hr = pExt->QueryInterface(riid, ppv);
			pExt->Release();
		}
	}
	return hr;
}

IFACEMETHODIMP ClassFactory::LockServer(BOOL fLock)
{
	if(fLock)
	{
		InterlockedIncrement(&g_cDllRef);
	}
	else
	{
		InterlockedDecrement(&g_cDllRef);
	}
	return S_OK;
}
