#include "stdafx.h"
#include "ClassFactory.h"
#include "LANshareShellExt.h"
#include <new>
#include <Shlwapi.h>
extern long g_cDllRef;


//Incrementa reference count al dll (Lo sto usando)
ClassFactory::ClassFactory() : m_cRef(1)
{
	InterlockedIncrement(&g_cDllRef);
}

//Decrementa reference count al dll (ho finito di usarlo)
ClassFactory::~ClassFactory()
{
	InterlockedDecrement(&g_cDllRef);
}

//Interface IUnknown

//Chiede se sono implementati i metodi dell'interfaccia IClassFactory
//Assegna a ppv il puntatore alla sezione di ClassFactory contenente l'interfaccia
IFACEMETHODIMP ClassFactory::QueryInterface(REFIID riid, void ** ppv)
{
	static const QITAB qit[] = {
		QITABENT(ClassFactory,IClassFactory),
		{ 0 },
	};
	return QISearch(this, qit, riid, ppv);
}

//Aumenta contatore reference della classe (La sto usando)
IFACEMETHODIMP_(ULONG) ClassFactory::AddRef()
{
	return InterlockedIncrement(&m_cRef);
}

//Decrementa contatore reference della classe (Posso distruggerla)
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

		//Crea il componente
		LANshareShellExt *pExt = new (std::nothrow) LANshareShellExt();
		if(pExt)
		{
			hr = pExt->QueryInterface(riid, ppv);
			pExt->Release();
		}
	}
	return hr;
}

//Blocca l'oggetto in memoria
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
