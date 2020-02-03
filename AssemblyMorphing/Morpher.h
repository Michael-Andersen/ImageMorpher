#pragma once
#include <Windows.h>
#ifdef __cplusplus
#define EXPORT extern "C" __declspec (dllexport)
#else
#define EXPORT __declspec (dllexport)
#endif

EXPORT VOID SetSize(int ht, int sd, int lsz);
EXPORT BYTE* CreateFrame(BYTE* src, BYTE* dest, float* lines, 
	 int frameNum, int numFrames);
BYTE* cross_dissolve(BYTE** results, int size, int frameNum, int NumFrames);
EXPORT VOID Free_Pointer(BYTE* ptr);