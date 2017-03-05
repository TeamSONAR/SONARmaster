using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{

    public GameObject[] nextFloor; //the prefab of a floor (for now)
    public Material[] colors;
    public GameObject Goal; // Where the player needs to get to?
    public GameObject Enemy;
    public int floorsize; //How much we add to the transform to position the next floor
    public int worldsize; //How many rooms in each direction?
    bool goalExists = false;
    int endgoal; // = Random.Range(worldsize * worldsize / 2, worldsize * worldsize - 2); //where we put the goal (this was giving us issues with initilizing, so it'll have to happen in a function
    int roomnum = 1; //counter for which room is being spawned (note that we start with a room)


    // Use this for initialization
    void Start()
    {
        endgoal = Random.Range(worldsize * worldsize / 2, worldsize * worldsize);
        Vector3 V = Vector3.zero;

        for (int i = 0; i < worldsize; i++) //
        {
            for (int j = 0; j < worldsize; j++)
            {
                //V.x += floorsize; //set transform to next open space
                int next = Random.Range(0, nextFloor.Length);
                GameObject room = Instantiate(nextFloor[next], V, Quaternion.identity); //instantiate the nextfloor in that space
                if (next != 4)
                    room.GetComponent<Renderer>().material = colors[Random.Range(0, colors.Length)];
                int rando = Random.Range(0, 10);
                if (rando == 0)
                {
                    Vector3 W = V;
                    //W.y += 1; only for the deathblock. Not for robokyle
                    Instantiate(Enemy, W, Quaternion.Euler(0, 90, 0));
                }
                if (CheckForGoal())
                    Instantiate(Goal, V, Quaternion.identity);
                roomnum++;
                V.x += floorsize; //set transform to next open space

            } //end for j

            V.x -= 10 * worldsize; //reset x position
            V.z += floorsize; //set start location for next row
            Instantiate(nextFloor[Random.Range(0, nextFloor.Length)], V, Quaternion.identity); //start the next row
            roomnum++;
            if (CheckForGoal())
                Instantiate(Goal, V, Quaternion.identity);
        } //end for i
        if (!goalExists) //if we get through all the rooms and don't have it, put it in the last one.
        {
            Instantiate(Goal, V, Quaternion.identity);
        }

    }



    bool CheckForGoal()
    {
        if (!goalExists) //if the goal doesn't already exist
        {
            if (roomnum == endgoal) //and the roomnumber is equal to the goal
            {
                goalExists = true;
                return true;
            }
            else;
            return false;
        }
        else;
            return false;
    }

}







