// BlenderToDepthMapDLL.cpp : Defines the exported functions for the DLL application.
//

#include "BlenderToDepthMapDLL.h"

#ifdef _WIN32

#include "stdafx.h"
#define BUF_SIZE 4+640*480*4
TCHAR szName[] = TEXT("Local\\MyFileMappingObject");
TCHAR szMsg[] = TEXT("Message from first process.");

#pragma comment(lib, "user32.lib")

struct Harambe {
	PVOID BufferLoc;
	HANDLE hMapFile;
};

void* CreateDepthBufMapFile()
{
	static Harambe DataStruct;
	void *pBuf;

	DataStruct.hMapFile = CreateFileMapping(
		INVALID_HANDLE_VALUE,    // use paging file
		NULL,                    // default security
		PAGE_READWRITE,          // read/write access
		0,                       // maximum object size (high-order DWORD)
		BUF_SIZE,                // maximum object size (low-order DWORD)
		szName);                 // name of mapping object

	if (DataStruct.hMapFile == NULL)
	{
		_tprintf(TEXT("Could not create file mapping object (%d).\n"),
			GetLastError());
		return 0;
	}
	pBuf = MapViewOfFile(DataStruct.hMapFile,   // handle to map object
		FILE_MAP_ALL_ACCESS, // read/write permission
		0,
		0,
		BUF_SIZE);

	if (pBuf == NULL)
	{
		_tprintf(TEXT("Could not map view of file (%d).\n"),
			GetLastError());

		CloseHandle(DataStruct.hMapFile);

		return 0;
	}

	DataStruct.BufferLoc = pBuf;
	return &DataStruct;
}

int UnmapDepthBufFile(void* UnmapFileStruct) {
	Harambe *HarUnmapFileStruct = (Harambe*)UnmapFileStruct;
	UnmapViewOfFile(HarUnmapFileStruct->BufferLoc);

	CloseHandle(HarUnmapFileStruct->hMapFile);

	return 47;
}

void* OpenDepthBufMapFileToRead()
{
	static Harambe DataStruct;
	void *pBuf;

	DataStruct.hMapFile = OpenFileMapping(
		FILE_MAP_ALL_ACCESS,   // read/write access
		FALSE,                 // do not inherit the name
		szName);               // name of mapping object

	if (DataStruct.hMapFile == NULL)
	{
		_tprintf(TEXT("Could not open file mapping object (%d).\n"),
			GetLastError());
		return 0;
	}

	pBuf = MapViewOfFile(DataStruct.hMapFile, // handle to map object
		FILE_MAP_ALL_ACCESS,  // read/write permission
		0,
		0,
		BUF_SIZE);

	if (pBuf == NULL)
	{
		_tprintf(TEXT("Could not map view of file (%d).\n"),
			GetLastError());

		CloseHandle(DataStruct.hMapFile);

		return 0;
	}

	DataStruct.BufferLoc = pBuf;
	return &DataStruct;
}

void* ReadDepthMapBufFile(void* InDataStruct) {
	Harambe *HarDataStruct = (Harambe*)InDataStruct;
	char* DumyPtr = (char*) HarDataStruct->BufferLoc;

	return (void*) (DumyPtr + 4);
}

int CheckDMBFlag(void* InDataStruct) {
	Harambe *HarDataStruct = (Harambe*)InDataStruct;
	int* DumyPtr = (int*)HarDataStruct->BufferLoc;

	return *DumyPtr;
}

/*int WriteDepthMapBufFile(void* InDataStruct, void* PtrInputBuf, int BufLen) {
Harambe *HarDataStruct = (Harambe*)InDataStruct;
float *InputBuf = (float*)PtrInputBuf;

CopyMemory(HarDataStruct->BufferLoc, InputBuf, (BufLen * sizeof(float)));

return 0;
}*/

#else
// Unix 

#define SHMSIZE 1024

#include <sys/types.h>
#include <sys/ipc.h> 
#include <sys/shm.h> 
#include <stdio.h>

key_t key; /* key to be passed to shmget() */ 
int shmflg; /* shmflg to be passed to shmget() */ 
int shmid; /* return value from shmget() */ 
int size; /* size to be passed to shmget() */ 



shmid = shmget ()


#endif