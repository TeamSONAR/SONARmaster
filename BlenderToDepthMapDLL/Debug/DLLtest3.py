# -*- coding: utf-8 -*-
"""
Created on Sat Oct  8 21:02:52 2016

@author: Colin
"""
import ctypes
import array
import bgl

lib = ctypes.WinDLL('F:/seniorproject/Visual Studio/BlenderToDepthMapDLL/x64/Debug/BlenderToDepthMapDLL.dll')

CreateDepthBufMapFile = lib[1]
WriteDepthMapBufFile = lib[2]
UnmapDepthBufFile = lib[3]


CreateDepthBufMapFile.restype = ctypes.c_void_p
#WriteDepthMapBufFile.argtypes = (ctypes.c_void_p, ctypes.c_short, ctypes.c_int)
HarambePointer = CreateDepthBufMapFile()
print(type(HarambePointer))
teststring = array.array('H')
stringtoinsert = 'Thisisatest'

for character in stringtoinsert:
    teststring.append(ord(character))
    
teststring.append(0)

WriteDepthMapBufFile.argtypes = [ctypes.c_void_p, ctypes.c_void_p, ctypes.c_int]
arr = (ctypes.c_short * len(teststring))(*teststring)
print(type(arr))
kfj = ctypes.pointer(arr)
#print(type(kfj))
kool = bgl.Buffer(bgl.GL_INT, 4)

WriteDepthMapBufFile(ctypes.c_void_p(HarambePointer), kfj, 12)

input('are it done? ')

UnmapDepthBufFile(ctypes.c_void_p(HarambePointer))


