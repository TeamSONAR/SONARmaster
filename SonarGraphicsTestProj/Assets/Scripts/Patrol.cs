using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour {
    Vector3 startPosition;
    Vector3 currentPosition;
    public bool isforward;
    public bool isleft;
    public bool isright;
    public bool isback;
    public int[] lengths;
    public int lengthToMove;
    float moved;

    [SerializeField]
    float speed = 0.1f;

	// Use this for initialization
	void Start () {
        startPosition = gameObject.transform.position;
        currentPosition = startPosition;
        setDirection(0);
        moved = 0;
        lengthToMove = lengths[Random.Range(0, lengths.Length)];


	}
	
	// Update is called once per frame
	void Update () {

        if (isforward)
        {
            currentPosition.x += speed;
            gameObject.transform.position = currentPosition;
            moved += speed;
        }

        else if (isleft)
        {
            currentPosition.z -= speed;
            gameObject.transform.position = currentPosition;
            moved += speed;
        }
        else if (isright)
        {
            currentPosition.z += speed;
            gameObject.transform.position = currentPosition;
            moved += speed;
        }
        else if (isback)
        {
            currentPosition.x -= speed;
            gameObject.transform.position = currentPosition;
            moved += speed;
        }



        if (moved >= lengthToMove)
        {
            if (isforward)
            {
                gameObject.GetComponent<Transform>().Rotate(0, 90, 0);
                setDirection(1);
                lengthToMove = lengths[Random.Range(0, lengths.Length)];
            }
            else if (isleft)
            {
                gameObject.GetComponent<Transform>().Rotate(0, 90, 0);
                setDirection(3);
                lengthToMove = lengths[Random.Range(0, lengths.Length)];
            }

            else if (isback)
            {


                gameObject.GetComponent<Transform>().Rotate(0, 90, 0);
                setDirection(2);
                lengthToMove = lengths[Random.Range(0, lengths.Length)];
            }
            else if (isright)
            {
                gameObject.GetComponent<Transform>().Rotate(0, 90, 0);
                setDirection(0);
                lengthToMove = lengths[Random.Range(0, lengths.Length)];
            }
        }
       


    }


    void setDirection(int x)
    {
        
        if (x == 0)
        {
            isforward = true;
            isleft = false;
            isright = false;
            isback = false;     
        }
        if (x == 1)
        {
            isforward = false;
            isleft = true;
            isright = false;
            isback = false;
        }
        if (x == 2)
        {
            isforward = false;
            isleft = false;
            isright = true;
            isback = false;
        }
        if (x == 3)
        {
            isforward = false;
            isleft = false;
            isright = false;
            isback = true;
        }
        moved = 0;

    }


    void OnTriggerEnter(Collider Other)
    {
        if (Other.tag == "PlayerController")
        {
            Debug.Log("OUCH!");
        }
    }

}
