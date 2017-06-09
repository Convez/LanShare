#pragma once

#include <Unknwn.h>
#include <Windows.h>



class ClassFactory : public IClassFactory
{
public:

	//Interfaccia IUnknown
	IFACEMETHODIMP QueryInterface(REFIID riid, void **ppv);
	IFACEMETHODIMP_(ULONG) AddRef();
	IFACEMETHODIMP_(ULONG) Release();

	//Interfaccia IClassFactory
	IFACEMETHODIMP CreateInstance(IUnknown *pUnkOther, REFIID riid, void **ppv);
	IFACEMETHODIMP LockServer(BOOL fLock);


	ClassFactory();

private:
	long m_cRef;
protected:
	~ClassFactory();
};

