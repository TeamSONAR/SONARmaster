using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Diagnostics;
using UnityEngine;
//using UnityEditor;

//[ExecuteInEditMode]
public class DepthCopyScript : MonoBehaviour {
    //Load the DLL functions from the DLL
    //DLL is located in Assets/Plugins/x86_64 and must be named RenderPlugin
    [DllImport("RenderingPlugin")]
    private static extern void SetTimeFromUnity(float t);

    [DllImport("RenderingPlugin")]
    private static extern System.IntPtr GetRenderEventFunc();

    [DllImport("RenderingPlugin")]
    private static extern System.IntPtr SetupReadPixels(int x, int y);

    [DllImport("RenderingPlugin")]
    private static extern void UnmapFile();

	[DllImport("RenderingPlugin")]
	private static extern void WriteMem(byte[] bytes, int size);

//	[DllImport("RenderingPlugin")]
//	private static extern int GetSizeMem();
    
    Texture2D tex;
    public Material mat;
    public Material BlankMat;
	string fullPath;
	Process process;

	bool processRunning = false;
    bool cpressed = false;
	bool bpressed = false;
    byte[] bytes;
    byte[] infoFile;
    System.IntPtr unmanagedPointer;

    // Use this for initialization
    void Start () {
		
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        tex = new Texture2D(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, TextureFormat.RGBA32, false);

        bytes = new byte[4*GetComponent<Camera>().pixelWidth*GetComponent<Camera>().pixelHeight];
        /*unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
        
        long foo = unmanagedPointer.ToInt64();
        byte[] infoFile = System.BitConverter.GetBytes(foo);
        System.IO.File.WriteAllBytes("infoFile.txt", infoFile);

        print("stuff");
        print(System.BitConverter.ToString(infoFile));
        print(unmanagedPointer.ToString());
        print(infoFile[7].ToString());*/
		bool debugging = false;
		if (debugging) {
			fullPath = Application.dataPath + "/../../SONARBackEnd/x64/Debug/SONARBackEnd.exe";
		} else {
			fullPath = Application.dataPath + "/../BackEnd/SONARBackEnd.exe";
		}

		if (processRunning) {
			process.Kill ();
		}
		bpressed = true;
		print ("PATH IS: " + Application.dataPath + "/../BackEnd/SONARBackEnd.exe");

		print (fullPath);
		process = System.Diagnostics.Process.Start (fullPath);
		processRunning = true;


    }

    void OnEnable()
	{
		bytes = new byte[4 * GetComponent<Camera>().pixelWidth * GetComponent<Camera>().pixelHeight];
		int size = bytes.Length;
		int xSize = GetComponent<Camera> ().pixelWidth;
		int ySize = GetComponent<Camera> ().pixelHeight;
		print("Size is: " + size);
		unmanagedPointer = SetupReadPixels (xSize, ySize);
		//unmanagedPointer = SetupReadPixels(size);
		 

		//Set up the dll, get a pointer to the shared named memory space
		if(unmanagedPointer.ToInt32() != 0)
		{
			print("INT OF UNMANAGEDPTR: " + unmanagedPointer.ToInt32());
			print("Mapping");
		}
		else
		{
			print("failed");
			Application.Quit();

		}

        
    }

    void Update()
    {
		if (Input.GetKeyDown ("c")) {
			cpressed = !cpressed;
		} 
		//if (Input.GetKeyDown ("b")) {
			//if (processRunning) {
			//	process.Kill ();
			//}
			//bpressed = true;
			//print ("PATH IS:" + Application.dataPath);


			//process = System.Diagnostics.Process.Start (fullPath);
			//processRunning = true;
			//cpressed = 0;
		} 		//else {
			//cpressed = 0;
		//}
            
   	// }

    void OnPostRender()
    {
        SetTimeFromUnity(Time.timeSinceLevelLoad);
        GL.IssuePluginEvent(GetRenderEventFunc(), 1);
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //apply the depth-to-color shader
        Graphics.Blit(source, destination, mat);
        
        RenderTexture.active = destination;

        //Reads the currently active rendertexture to the texture2d
        tex.ReadPixels(new Rect(0, 0, GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight), 0, 0, false);
        tex.Apply();
        bytes = tex.GetRawTextureData();

		//for (int index = 0; index < bytes.Length; index++)
		//{
		//	BitConverter.ToBoolean(bytes, index);
		//}

		//Copies the raw texture data to the pointer to the shared memory space provided by the DLL
//		Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
		//int x = GetSizeMem();
		print("new size is: " + bytes.Length);
		//print ("SHMSize is: " + x);

		if (Application.platform == RuntimePlatform.WindowsPlayer) {
			Marshal.Copy (bytes, 0, unmanagedPointer, bytes.Length);
		} 
		else 
		{
			Marshal.Copy (bytes, 0, unmanagedPointer, bytes.Length);
//			WriteMem (bytes, bytes.Length);
//			for (int i = 0; i < 10; i++)
//				print(Convert.ToBoolean(bytes[i]));
//			for (int i = 0; i < 10; i++) {
//				print (bytes [i]);
//			};
		}
		//Marshal.AllocHGlobal(1);

        //Call this to display whatever we want on the screen (use a mat if shader is desired)
        if(cpressed == false)
        {
            Graphics.Blit(source, destination, BlankMat);
        }
        
    }

    void OnDisable()
    {
		UnmapFile();
		process.Kill();
        print("unmapping");
    }
}
