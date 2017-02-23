#pragma once


#ifdef BlenderToDepthMapDLL_EXPORTS
#define BlenderToDepthMapDLL_API __declspec(dllexport) 
#else
#define BlenderToDepthMapDLL_API __declspec(dllimport) 
#endif


__declspec(dllexport) void* CreateDepthBufMapFile();
__declspec(dllexport) int UnmapDepthBufFile(void* UnmapFileStruct);
__declspec(dllexport) void* OpenDepthBufMapFileToRead();
__declspec(dllexport) void* ReadDepthMapBufFile(void* InDataStruct);
__declspec(dllexport) int CheckDMBFlag(void* InDataStruct);