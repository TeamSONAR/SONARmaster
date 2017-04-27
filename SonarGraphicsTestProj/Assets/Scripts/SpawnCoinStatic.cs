using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCoinStatic : MonoBehaviour {
	float x;
	float y; 
	float z;
	Vector3 pos;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (0,0,50*Time.deltaTime);
	}
}
