using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathOnContact : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    void OnTriggerEnter(Collider Other)
    {
        if (Other.tag == "PlayerController")
        {
            GameObject foo;
            foo = Other.transform.parent.gameObject;
            GameObject.Destroy(foo);
            
            //GameObject.Destroy(parent);
        }
    }
}
