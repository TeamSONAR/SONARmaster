using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GlobalControllerData : MonoBehaviour {

    public GameObject controller;
    public int numRestarts = 0;
    public bool enableHeadtracking = false;
    public bool enableGamepad = false;
    public bool restartScript = false;

    private void Update()
    {
        if(restartScript)
        {
            numRestarts += 1;
            controller.GetComponent<FirstPersonController>().enabled = false;
            controller.GetComponent<FirstPersonController>().enabled = true;
        }
    }

}
