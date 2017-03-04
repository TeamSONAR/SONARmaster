using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawnController : MonoBehaviour {

    public GameObject[] Rooms;
    public GameObject startRoom;
    public GameObject currentRoom;
    private GameObject nextRoom;
    public Transform nextSpawn;

    void Start()
    {
        currentRoom = startRoom;
    }


	void onTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            nextRoom = Rooms[Random.Range(0, Rooms.Length)];
            Instantiate(nextRoom, currentRoom.GetComponent<Transform>());
        }
    }


}
