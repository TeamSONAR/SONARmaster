using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour {


    public GameObject[] objects;
	
    // Use this for initialization
	void Start ()
    {
        int rando = Random.Range(0, objects.Length);
        Vector3 newitem = gameObject.transform.position;
        newitem.x += Random.Range(0, 5);
        Instantiate(objects[rando], newitem, Quaternion.identity);
	}
	
}
