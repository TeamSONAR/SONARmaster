using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCarForward : MonoBehaviour {

    public Transform tf;
    Vector3 current;
    Vector3 forwardVector = new Vector3(.3f, 0f, 0f);
	
	// Update is called once per frame
	void Update () {
        current = tf.position;
        tf.position = current + forwardVector;
        
	}
}
