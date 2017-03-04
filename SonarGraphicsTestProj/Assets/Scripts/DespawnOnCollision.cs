using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnOnCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	

    void OnTriggerEnter(Collider Other)
    {
        if (Other.tag == "FloorCollider")
        {
            GameObject.Destroy(Other);
            Debug.LogErrorFormat("Despawned a Room");
        }
    }
	
}
