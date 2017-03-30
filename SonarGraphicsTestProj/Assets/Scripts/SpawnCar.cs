using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCar : MonoBehaviour {

    public GameObject car;
    bool spawnedCar = false;
    public bool carMovesForward = true;

	// Update is called once per frame
	void Update ()
    {
        if (!spawnedCar)
        {
            spawnedCar = true;
            Invoke("SpawnItem", Random.Range(2, 10));
        }	
	}


    void SpawnItem()
    {
        spawnedCar = false;
        
        GameObject createdCar = (GameObject)Instantiate(car, transform.position, transform.rotation);
        createdCar.GetComponent<MoveCar>().moveForward = carMovesForward;
    }
}
