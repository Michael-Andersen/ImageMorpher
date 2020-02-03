#include <Windows.h>
#include "Morpher.h"


INT height;
INT stride;
INT lsize;


INT WINAPI DllMain(HINSTANCE hInstance, DWORD fdwReason, PVOID pvReserved)
{
	return TRUE;
}

EXPORT VOID SetSize(int ht, int sd, int lsz) {
	height = ht;
	stride = sd;
	lsize = lsz;
} 

EXPORT BYTE* CreateFrame(BYTE* src, BYTE* dest, float* lines, int frameNum, int numFrames ) {
	int bwidth = stride * sizeof(src[0]);
	int blsize = lsize * sizeof(lines[0]);
	byte* result1 = (byte*)_aligned_malloc(sizeof(BYTE) * height * stride, 4);
	byte* result2 = (byte*)_aligned_malloc(sizeof(BYTE) * height * stride, 4);
	byte** results = (byte**)malloc(sizeof(BYTE*) * 2);
	results[0] = result1;
	results[1] = result2;
	float coords[] = { 0.0f, 0.0f, 0.0f, 1.0f };
	int* sizeInfo = (int*)malloc(6 * sizeof(int));
	sizeInfo[0] = height;
	sizeInfo[1] = stride / 8.0;
	sizeInfo[2] = blsize;
	_asm {
		push eax //for sizes
		push ebx // for src
		push ecx //for counting
		push edx //for dest
		push esi //for size constants
		push edi //for lines
		push ebp //for resulting byte[]
		movups xmm0, [coords]
		mov esi, sizeInfo
		mov ebx, src
		mov edx, dest
		mov edi, lines
		mov ebp, results
		xorps xmm7, xmm7
		xorps xmm1, xmm1
		xor ecx, ecx
		START : mov eax, [esi]
				cmp ecx, eax
				jge DONE
				push ecx
				xor ecx, ecx
				INNERSTARTA : mov eax, [esi + 4]
							  cmp ecx, eax
							  jge INNERDONEA
					 LASTODD: push ecx
							  xor ecx, ecx
							  add ecx, 160
							  xorps xmm6, xmm6
							  movups[edi + 48], xmm6
							  INNERSTARTB : mov eax, [esi + 8]
											cmp ecx, eax
											jge INNERDONEB
											add ecx, 64
											movups xmm5, [edi + ecx] //frame start points
											vsubps xmm6, xmm5, xmm0 // get xp
											add ecx, 48
											movups xmm3, [edi + ecx] //get frame norm
											vdpps xmm4, xmm3, xmm6, 0x31 //get xp dot norm1
											dpps xmm3, xmm6, 0xC4 //get xp dot norm2
											addps xmm3, xmm4 //dots in spot 0 and spot 2
											add ecx, 16
											movups xmm6, [edi + ecx] //get frame lengths
											divps xmm3, xmm6 // distance in spot 0 and spot 2
											vsubps xmm4, xmm0, xmm5 // get px
											sub ecx, 32
											movups xmm6, [edi + ecx] //get frame vector
											vdpps xmm5, xmm6, xmm4, 0x31 //get px dot vector
											dpps xmm6, xmm4, 0xC4 //get px dot vector 2
											addps xmm5, xmm6 //dots in spot 0 and spot 2
											add ecx, 48
											movups xmm6, [edi + ecx] //get frame sq length
											divps xmm5, xmm6 //fraction length in spot 0 and spot 2
											sub ecx, 112
											shufps xmm5, xmm5, 0xA0 //fractional length
											shufps xmm3, xmm3, 0xA0 //distance
											movups xmm6, [edi + ecx] //get src vector
											mulps xmm6, xmm5 //fl * src vect
											add ecx, 16
											movups xmm4, [edi + ecx] //src norm/normalized
											mulps xmm4, xmm3 //dist * nrm/normalized
											subps xmm6, xmm4 //fl* src vert - dist * nrm/normalized
											sub ecx, 48
											movups xmm4, [edi + ecx] //src start point
											addps xmm4, xmm6 //src new point
											subps xmm4, xmm0 //src delta x
											add ecx, 192
											movups xmm6, [edi + ecx] //get dest vector
											mulps xmm6, xmm5 //fl * dest vect
											add ecx, 16
											movups xmm2, [edi + ecx] //dest norm/normalized
											mulps xmm2, xmm3 //dist * nrm/normalized
											subps xmm6, xmm2 //fl* dest vert - dist * nrm/normalized
											sub ecx, 48
											movups xmm2, [edi + ecx] //dest start point
											addps xmm2, xmm6 //dest new point
											subps xmm2, xmm0 //dest delta x
											movups xmm6, [edi + 64]
											comiss xmm5, xmm6 //check frac length less than 0
											jb SDIST
											movups xmm6, [edi + 128]
											comiss xmm5, xmm6 //check frac length greater than 1
											ja EDIST
											movups xmm6, [edi + 32] // a value
											addps xmm3, xmm6
											mulps xmm3, xmm3 //square (b value =2)
											rcpps xmm3, xmm3 //weight
											jmp WEIGHT
											SDIST : sub ecx, 96
													movups xmm6, [edi + ecx] //frame start point
													vsubps xmm3, xmm6, xmm0 //px
													vdpps xmm5, xmm3, xmm3, 0x31 //
													dpps xmm3, xmm3, 0xC4 //get dot
													addps xmm3, xmm5 //dots in spot 0 and spot 2
													sqrtps xmm3, xmm3 //length in spot 0 and spot 2
													shufps xmm3, xmm3, 0xA0
													movups xmm6, [edi + 32] // a value
													addps xmm3, xmm6
													mulps xmm3, xmm3 //square (b value =2)
													rcpps xmm3, xmm3 //weight
													add ecx, 96
													jmp WEIGHT
													EDIST : sub ecx, 80
															movups xmm6, [edi + ecx] //frame end point
															vsubps xmm5, xmm6, xmm0 //qx
															vdpps xmm3, xmm5, xmm5, 0x31 //
															dpps xmm5, xmm5, 0xC4 //get dot
															addps xmm3, xmm5 //dots in spot 0 and spot 2
															sqrtps xmm3, xmm3 //length in spot 0 and spot 2
															shufps xmm3, xmm3, 0xA0
															movups xmm6, [edi + 32] // a value
															addps xmm3, xmm6
															mulps xmm3, xmm3 //square (b value =2)
															rcpps xmm3, xmm3 //weight
															add ecx, 80
															WEIGHT:movups xmm6, [edi + 48] //get total weight
																   addps xmm6, xmm3 //add to total weight
																   movups[edi + 48], xmm6 //put total weight back
																   mulps xmm4, xmm3 //weight src delta
																   mulps xmm2, xmm3 //weigth dest delta
																   addps xmm1, xmm4 // add to src delta weight numerator
																   addps xmm7, xmm2 // add to dest delta weight numerator
																   add ecx, 64
																   jmp INNERSTARTB
																   INNERDONEB : pop ecx
																				movups xmm2, [edi + 48] //get total weight
																				divps xmm1, xmm2 //divide src delta by total weight
																				divps xmm7, xmm2 // divide dest delta by total weight
																				addps xmm1, xmm0 //add avg delta to point src
																				addps xmm7, xmm0 // add avg delta to point dest
																				roundps xmm1, xmm1, 0xFF //make src coord int
																				roundps xmm7, xmm7, 0xFF// make dest coord int
																				movups xmm2, [edi + 64] //all zeroes
																				maxps xmm1, xmm2 // check not below 0 src
																				maxps xmm7, xmm2 // check not below 0 dest
																				movups xmm2, [edi + 80] //width and height
																				minps xmm1, xmm2 //check within pic bounds src
																				minps xmm7, xmm2 //check within pic bounds dest
																				vshufps xmm3, xmm1, xmm1, 0xA0 //height x2 height x2 src
																				vshufps xmm2, xmm1, xmm1, 0xF5// width x2 width x2 src
																				movups xmm4, [edi + 96] // all 4s
																				movups xmm1, [edi + 112] // get width constant
																				mulps xmm3, xmm1
																				addps xmm2, xmm3 //width x height
																				mulps xmm2, xmm4 //pixel number src
							
																				vshufps xmm5, xmm2, xmm2, 0x00 //src point 1
																				shufps xmm2, xmm2, 0xAA //src point 2
																				
																				cvttss2si eax, xmm5 //get src point 1
																				movd xmm5, [ebx + eax] //get src pixels 1
																				
																				push ecx
																				mov ecx, [ebp] //get result1
																				//paddusb xmm3, xmm5 //combine pixels 1
																				vshufps xmm6, xmm0, xmm0, 0xA0 //height x2 height x2 coords
																				vshufps xmm3, xmm0, xmm0, 0xF5 //width x2 width x2 coords
																				mulps xmm6, xmm1 //width x height
																				addps xmm3, xmm6
																				mulps xmm3, xmm4 //pixel number coords
																				vshufps xmm6, xmm3, xmm3, 0x00 //coords point 1
																				shufps xmm3, xmm3, 0xAA //coords point 2
																				cvttss2si eax, xmm6 //get coords point 1
																				movd[ecx + eax], xmm5 //set pixel 1 src
																				cvttss2si eax, xmm2 //get src point 2
																				movd xmm2, [ebx + eax] //get src pixels 2
																				cvttss2si eax, xmm3 // get coords point 2
																				movd [ecx + eax], xmm2 // set pixel 2 src
																				vshufps xmm2, xmm7, xmm7, 0xA0 //height x2 height x2 dest
																				vshufps xmm5, xmm7, xmm7, 0xF5 //width x2 widht x2 dest
																				mulps xmm2, xmm1  //width x height
																				addps xmm5, xmm2 //add width
																				mulps xmm5, xmm4 //pixel number dest
																				vshufps xmm2, xmm5, xmm5, 0x00 //dest point 1
																				shufps xmm5, xmm5, 0xAA //dest point 2
																				cvttss2si eax, xmm2 // get dest point 1
																				movd xmm2, [edx + eax] //get dest pixels 1
																				cvttss2si eax, xmm5 // get dest point 2
																				movd xmm5, [edx + eax] //get dest pixels 2
																				//paddusb xmm6, xmm2 //combine pixels 2
																				mov ecx, [ebp + 4]
																				cvttss2si eax, xmm6 //get coords point 1
																				movd [ecx + eax], xmm2 //set pixel 1 dest
																				cvttss2si eax, xmm3 //get coords point 2
																				movd[ecx + eax], xmm5 //set pixel 2 dest
																				//movd[ebp + eax], xmm6 //set pixel 2*/
																				pop ecx
																				add ecx, 1
																				movups xmm1, [edi]
																				addps xmm0, xmm1
																				xorps xmm1, xmm1
																				xorps xmm7, xmm7
																				jmp INNERSTARTA
																							INNERDONEA : vshufps xmm3, xmm0, xmm0, 0x55
																							movups xmm5, [edi + 112]
																							comiss xmm3, xmm5 //check if coord less than width
																							jb FIXODD
																							 
																							 movups xmm1, [edi + 16]
																							 addsubps xmm0, xmm1
																							 xorps xmm1, xmm1
																							 pop ecx
																							 add ecx, 1
																							 jmp START
																							 FIXODD: movups xmm3, [edi + 144]
																							 addps xmm0, xmm3 //fix coords for odd widths
																							 jmp LASTODD
																							 DONE : pop ebp
																									pop edi
																									pop esi
																									pop edx
																									pop ecx
																									pop ebx
																									pop eax
	} 
	return cross_dissolve(results, sizeof(BYTE) * height * stride, frameNum, numFrames);
}

BYTE* cross_dissolve(BYTE** results, int size, int frameNum, int NumFrames) {
	for (int i = 0; i < size; i++) {
		if (i > size) {
			results[0][i] = 255;
		}
		else {
			results[0][i] = ((results[0][i] * (NumFrames + 1.0 - frameNum) / (NumFrames + 1.0))
				+ (results[1][i] * ((frameNum) / (NumFrames + 1.0))));
		}
		
	}
	_aligned_free(results[1]);
	byte * return_val = results[0];
	free(results);
	return return_val;
}

EXPORT VOID Free_Pointer(BYTE* ptr) {
	_aligned_free(ptr);
}
