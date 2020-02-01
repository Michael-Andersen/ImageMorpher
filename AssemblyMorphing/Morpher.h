#pragma once
#include <Windows.h>
#ifdef __cplusplus
#define EXPORT extern "C" __declspec (dllexport)
#else
#define EXPORT __declspec (dllexport)
#endif

EXPORT VOID SetSize(int ht, int sd, int lsz);
EXPORT BYTE* CreateFrame(BYTE* src, BYTE* dest, float* lines, 
	BYTE* result, int frameNum, int numFrames);
byte* cross_dissolve(byte** results, int size, int frameNum, int NumFrames);