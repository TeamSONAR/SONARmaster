// Example low level rendering Unity plugin



#include "PlatformBase.h"
#include "RenderAPI.h"
#include "GLEW/glew.h"

#include <assert.h>
#include <math.h>
#include <fstream>
#include <iostream>

using namespace std;

//#define BUF_SIZE 4+640*480*4

#if UNITY_WIN
#include "Windows.h"


#pragma comment(lib, "user32.lib")


TCHAR szName[] = TEXT("Local\\MyFileMappingObject");
struct FileMappingInfo {
	PVOID BufferLoc;
	HANDLE hMapFile;
};

// --------------------------------------------------------------------------
// SetTimeFromUnity, an example function we export which is called by one of the scripts.

static float g_Time;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTimeFromUnity (float t) { 
	g_Time = t; 
}

static int* PixelBufferPtr;
static void *texHandle;
static HGLRC context;
static HDC Dcontext;
static int depthfunc;
static bool depthtestenabled;
static void* StructPtr;
static int GLDepthFormat = GL_DEPTH_COMPONENT;

void* CreateDepthBufMapFile(int x, int y)
{
#define BUF_SIZE 4 + x * y * 4
	const char *path = ".\\..\\..\\SonarGraphicsTestProj\\Assets\\Plugins\\x86_64\\test.txt";
	ofstream writeFile;
	//writeFile.open(path);
	writeFile.open("dimensions.txt");
	writeFile << x << "\n" << y;
	writeFile.close();
	static FileMappingInfo DataStruct;
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
		return 0;
	}
	pBuf = MapViewOfFile(DataStruct.hMapFile,   // handle to map object
		FILE_MAP_ALL_ACCESS, // read/write permission
		0,
		0,
		BUF_SIZE);

	if (pBuf == NULL)
	{

		CloseHandle(DataStruct.hMapFile);

		return 0;
	}

	DataStruct.BufferLoc = pBuf;
	char* DumyPtr = (char*)pBuf;
	char* DumyPtr2 = DumyPtr + 4;
	PixelBufferPtr = (int*)(DumyPtr2);
	return &DataStruct;
}

int UnmapDepthBufFile() {
	FileMappingInfo *HarUnmapFileStruct = (FileMappingInfo*)StructPtr;
	UnmapViewOfFile(HarUnmapFileStruct->BufferLoc);

	CloseHandle(HarUnmapFileStruct->hMapFile);

	return 1;
}

//exported function that sets up the mapped file and returns the pointer to the buffer
extern "C" unsigned long long UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetupReadPixels(int x, int y) {
	StructPtr = CreateDepthBufMapFile(x,y);
	
	if (StructPtr == 0) {
		return 0;
	}

	FileMappingInfo* DummyStruct = (FileMappingInfo*)StructPtr;
	int* LaserPtr = (int*)DummyStruct->BufferLoc;
	*LaserPtr = 1;

	return (unsigned long long) PixelBufferPtr;
}

//exported function that just calls the unmap function
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnmapFile() {
	UnmapDepthBufFile();
}

// --------------------------------------------------------------------------
// UnitySetInterfaces

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
	
	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

// --------------------------------------------------------------------------
// GraphicsDeviceEvent


static RenderAPI* s_CurrentAPI = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;

// --------------------------------------------------------------------------
// OnRenderEvent
// This will be called for GL.IssuePluginEvent script calls; eventID will
// be the integer passed to IssuePluginEvent. In this example, we just ignore
// that value.

static void  UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	// Unknown / unsupported graphics device type? Do nothing
	if (s_CurrentAPI == NULL)
		return;

	//Harambe* DummyStruct = (Harambe*)StructPtr;
	//int* LaserPtr = (int*)DummyStruct->BufferLoc;
	//*LaserPtr = 1;

	//PixelBufferPtr

	//*LaserPtr = 0;
}

// --------------------------------------------------------------------------
// GetRenderEventFunc, an example function we export which is used to get a rendering event callback function.

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}


static void UNITY_INTERFACE_API  OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	// Create graphics API implementation upon initialization
	if (eventType == kUnityGfxDeviceEventInitialize)
	{
		assert(s_CurrentAPI == NULL);
		s_DeviceType = s_Graphics->GetRenderer();
		s_CurrentAPI = CreateRenderAPI(s_DeviceType);
	}

	// Let the implementation process the device related events
	if (s_CurrentAPI)
	{
		s_CurrentAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
	}

	// Cleanup graphics API implementation upon shutdown
	if (eventType == kUnityGfxDeviceEventShutdown)
	{
		delete s_CurrentAPI;
		s_CurrentAPI = NULL;
		s_DeviceType = kUnityGfxRendererNull;
	}
}



#else
#include <stdio.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/shm.h>

#define SHM_SIZE 4+640*480*4 //1024  /* make it a 1K shared memory segment */

static float g_Time;
key_t key;
int shmid;
int *data;
int mode;
static int * StructPtr;
static int* PixelBufferPtr;



extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTimeFromUnity (float t) {
    g_Time = t;
}

void* CreateDepthBufMapFile(int size)
{
//    if ((key = ftok("./", 'R')) == -1) {
//        perror("ftok");
//        exit(1);
//    }
    key = 400;
    
    /* connect to (and possibly create) the segment: */
//    if ((shmid = shmget(key, SHM_SIZE, 0644 | IPC_CREAT)) == -1) {
    if ((shmid = shmget(key, size, 0644 | IPC_CREAT)) == -1) {
        perror("shmget");
        exit(1);
    }
    
    /* attach to the segment to get a pointer to it: */
    //Equiv to pbuf?
    
    data = (int*)shmat(shmid, (void *)0, 0);
    if (data == (int *)(-1)) {
        perror("shmat");
        exit(1);
    }
//    char* DumyPtr = (char*)data;
//    char* DumyPtr2 = DumyPtr + 4;
//    PixelBufferPtr = (int*)(DumyPtr2);
    
    return &data;
}


int UnmapDepthBufFile()
{
    /* detach from the segment: */
    if (shmdt(data) == -1) {
        perror("shmdt");
//        exit(1);
        return -1;
    }
    else
    {
        return 1;
    }
}

//TODO: FAILURE POINT
////exported function that sets up the mapped file and returns the pointer to the buffer
extern "C" int * UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetupReadPixels(int size) {
    StructPtr = (int*)CreateDepthBufMapFile(size);
    
    if (StructPtr == 0) {
        return 0;
    }
    
//    FileMappingInfo* DummyStruct = (FileMappingInfo*)StructPtr;
//    int* LaserPtr = (int*)DummyStruct->BufferLoc;
//    *LaserPtr = 1;
    
    return (int *) StructPtr; //PixelBufferPtr;
}


////exported function that just calls the unmap function
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnmapFile() {
    UnmapDepthBufFile();
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetSizeMem() {
    return sizeof(StructPtr);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API WriteMem(bool * bytes, int size)
{
    memcpy(data, bytes, size);
//    for(int i = 0; i < size; i++)
//    {
//        data[i] = bytes[i];
//    }
}




//TODO:
//____________________

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_UnityInterfaces = unityInterfaces;
    s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
    s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
    
    // Run OnGraphicsDeviceEvent(initialize) manually on plugin load
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

// --------------------------------------------------------------------------
// GraphicsDeviceEvent


static RenderAPI* s_CurrentAPI = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;

// --------------------------------------------------------------------------
// OnRenderEvent
// This will be called for GL.IssuePluginEvent script calls; eventID will
// be the integer passed to IssuePluginEvent. In this example, we just ignore
// that value.

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
    // Unknown / unsupported graphics device type? Do nothing
    if (s_CurrentAPI == NULL)
        return;
}

// --------------------------------------------------------------------------
// GetRenderEventFunc, an example function we export which is used to get a rendering event callback function.

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
    return OnRenderEvent;
}




static void UNITY_INTERFACE_API  OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    // Create graphics API implementation upon initialization
    if (eventType == kUnityGfxDeviceEventInitialize)
    {
        assert(s_CurrentAPI == NULL);
        s_DeviceType = s_Graphics->GetRenderer();
        s_CurrentAPI = CreateRenderAPI(s_DeviceType);
    }
    
    // Let the implementation process the device related events
    if (s_CurrentAPI)
    {
        s_CurrentAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
    }
    
    // Cleanup graphics API implementation upon shutdown
    if (eventType == kUnityGfxDeviceEventShutdown)
    {
        delete s_CurrentAPI;
        s_CurrentAPI = NULL;
        s_DeviceType = kUnityGfxRendererNull;
    }
}








//int main(int argc, char *argv[])
//{
////    key_t key;
////    int shmid;
////    char *data;
////    int mode;
//    
//    if (argc > 2) {
//        fprintf(stderr, "usage: shmdemo [data_to_write]\n");
//        exit(1);
//    }

    /* make the key: */
//    if ((key = ftok("main.cpp", 'R')) == -1) {
//        perror("ftok");
//        exit(1);
//    }
    
    /* connect to (and possibly create) the segment: */
//    if ((shmid = shmget(key, SHM_SIZE, 0644 | IPC_CREAT)) == -1) {
//        perror("shmget");
//        exit(1);
//    }
    
    /* attach to the segment to get a pointer to it: */
//    data = (char*)shmat(shmid, (void *)0, 0);
//    if (data == (char *)(-1)) {
//        perror("shmat");
//        exit(1);
//    }
    
    /* read or modify the segment, based on the command line: */
//    if (argc == 2) {
//        printf("writing to segment: \"%s\"\n", argv[1]);
//        strncpy(data, argv[1], SHM_SIZE);
//    } else
//        printf("segment contains: \"%s\"\n", data);
//    
//    /* detach from the segment: */
//    if (shmdt(data) == -1) {
//        perror("shmdt");
//        exit(1);
//    }
//    
//    return 0;
//}


#endif
