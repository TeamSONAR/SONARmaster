using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;
using UnityEditor;

//[ExecuteInEditMode]
public class DepthCopyScript : MonoBehaviour {
    //Load the DLL functions from the DLL
    //DLL is located in Assets/Plugins/x86_64 and must be named RenderPlugin
    [DllImport("RenderingPlugin")]
    private static extern void SetTimeFromUnity(float t);

    [DllImport("RenderingPlugin")]
    private static extern System.IntPtr GetRenderEventFunc();

    [DllImport("RenderingPlugin")]
    private static extern System.IntPtr SetupReadPixels();

    [DllImport("RenderingPlugin")]
    private static extern void UnmapFile();
    

    Texture2D tex;
    public Material mat;
    public Material BlankMat;

    int cpressed = 0;
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
    }

    void OnEnable()
    {
		unmanagedPointer = SetupReadPixels();
		//Set up the dll, get a pointer to the shared named memory space
		if(unmanagedPointer.ToInt32() != 0)
		{
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
        if (Input.GetKey("c"))
        {
            cpressed = 1;
        } else
        {
            cpressed = 0;
        }
            
    }

    void OnPostRender()
    {
        SetTimeFromUnity(Time.timeSinceLevelLoad);

        GL.IssuePluginEvent(GetRenderEventFunc(), 1);
    }


	// BUG HERE!!
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //apply the depth-to-color shader
        Graphics.Blit(source, destination, mat);
        
        RenderTexture.active = destination;

        //Reads the currently active rendertexture to the texture2d
        tex.ReadPixels(new Rect(0, 0, GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight), 0, 0, false);
        tex.Apply();
        bytes = tex.GetRawTextureData();

        //Copies the raw texture data to the pointer to the shared memory space provided by the DLL
        //Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);

        //Call this to display whatever we want on the screen (use a mat if shader is desired)
        if (cpressed == 0)
        {
            Graphics.Blit(source, destination, BlankMat);
        }
        
    }

    void OnDisable()
    {
        print("unmapping");
        //UnmapFile();
    }
}
