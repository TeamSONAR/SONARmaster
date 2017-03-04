using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNextFloor : MonoBehaviour {

    public GameObject firstFloor;
    public GameObject[] nextFloor; //the prefab of a floor (for now)
    public int floorsize; //How much we add to the transform to position the next floor
    

    
    void Update()
    {
       
    }

	void OnTriggerEnter(Collider Other)
    {
        

        if (Other.tag == "FloorCollider" ) //Right now this calls way more than once per update...
        {
            int chance = Random.Range(0, 5);
            Vector3 current = Other.transform.position;

            //this spawns the floor in one of four cardinal directions
            if (chance == 0)
                current.x += floorsize;
            if (chance == 1)
                current.x -= floorsize;
            if (chance == 2)
                current.z += floorsize;
            if (chance == 3)
                current.z -= floorsize;

             
            Instantiate(nextFloor[Random.Range(0, nextFloor.Length)], current, Quaternion.identity);//Quaternion.Euler(Random.Range(-10, 10) , 0, 0));
            //DanceForMe();
            //Debug.LogErrorFormat("Spawned the floor at " + current);

        }
    }


    public IEnumerator DanceForMe()
    {
        yield return new WaitForSeconds(0.2f);
    }
}
