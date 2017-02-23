# -*- coding: utf-8 -*-
"""
Created on Sat Oct  8 20:59:01 2016

@author: Colin
"""

import ctypes

# Load DLL into memory.

hllDll = ctypes.WinDLL ('F:/seniorproject/Visual Studio/BlenderToDepthMapDLL/x64/Debug/BlenderToDepthMapDLL.dll')

# Set up prototype and parameters for the desired function call.
# HLLAPI

hllApiProto = ctypes.WINFUNCTYPE (
    ctypes.c_void_p)

# Actually map the call ("HLLAPI(...)") to a Python name.

hllApi = hllApiProto (("CreateDepthBufMapFile", hllDll))

# This is how you can actually call the DLL function.
# Set up the variables and call the Python name with them.

zoinks = hllApi ()
print(zoinks)