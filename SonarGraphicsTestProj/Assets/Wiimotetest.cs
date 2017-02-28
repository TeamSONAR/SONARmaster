using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using WiimoteApi;

public class Wiimotetest : MonoBehaviour
{

    /*void OnGUI()
    {
        int c = wiimote_count();
        if (c > 0)
        {
            display = "";
            for (int i = 0; i <= c - 1; i++)
            {
                display += "Wiimote " + i + " found!\n";
            }
        }
        else display = "Press the '1' and '2' buttons on your Wii Remote.";

        GUI.Label(new Rect(10, Screen.height - 100, 500, 100), display);
    }*/

    void Start()
    {
        WiimoteManager.FindWiimotes();
    }

    void Update()
    {
        AccelData foo = GetComponent<Wiimote>().Accel;
        float[] ree = foo.GetCalibratedAccelData();
        print(ree.ToString());
    }

    void OnApplicationQuit()
    {
        WiimoteManager.Cleanup(WiimoteManager.Wiimotes[0]);
    }

}