using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinOnEnter : MonoBehaviour {

	void OnTriggerEnter(Collider Other)
    {
        if (Other.tag == "PlayerController")
            {
                Debug.Log("GOOD WORK!");
                Application.Quit();
            }
    }
}
