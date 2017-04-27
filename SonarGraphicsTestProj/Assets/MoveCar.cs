using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCar : MonoBehaviour {

    Vector3 directionVector;
    public bool moveForward = true;

    void Start()
    {
        if (moveForward)
        {
            directionVector = new Vector3(.1f, 0f, 0f);
        }
        else
        {
            directionVector = new Vector3(-.1f, 0f, 0f);
        }
        gameObject.transform.rotation = Quaternion.LookRotation(directionVector);
    }
    
	void Update()
    {
        // check if falling
        if(gameObject.transform.position.y < -50f)
        {
            Destroy(gameObject);
        }
        else // move forward
        {
            gameObject.transform.position += directionVector;
        }
        
    }
}
