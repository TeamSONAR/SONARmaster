using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCarForward : MonoBehaviour {
    
    Vector3 directionVector = new Vector3(.3f, 0f, 0f);
    public bool moveForward = true;

    void Start()
    {
        if(moveForward)
        {
            directionVector = new Vector3(.3f, 0f, 0f);
        }
        else
        {
            directionVector = new Vector3(-.3f, 0f, 0f);
        }
            
    }
    
	// Update is called once per frame
	void Update () {
        gameObject.transform.position += directionVector;
        
	}
}
