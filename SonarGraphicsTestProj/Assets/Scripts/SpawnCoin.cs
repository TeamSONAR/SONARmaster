using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCoin : MonoBehaviour {
	float x;
	float y; 
	float z;
	Vector3 pos;
	// Use this for initialization
	void Start () {
		x = Random.Range (0, 5);
		y = 1.5f;
		z = Random.Range (0, 5);
		pos = new Vector3 (x*10, y, z*10);
		transform.position = pos;
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (0,0,50*Time.deltaTime);
	}
}
