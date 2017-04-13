using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenSesame : MonoBehaviour
{
    bool hasMoved;


    void Start()
    {
        hasMoved = false;
    }

	void OnTriggerEnter(Collider Other)
    {
        if (Other.tag == "Player")
        {
            Debug.Log("OPENS");
            Vector3 pos = gameObject.transform.position;
            Vector3 newPos = new Vector3(-5, 0, 0);
            if (hasMoved == false)
            {
                gameObject.transform.position = pos + newPos;
                Invoke("Moved", 1);
            }

            if (hasMoved == true)
            {
                gameObject.transform.position = pos - newPos;
                Invoke("MovedBack", 1);
            }
            
            //Destroy(gameObject);
        }
    }



    void Moved()
    {
        hasMoved = true;
    }

    void MovedBack()
    {
        hasMoved = false;
    }
}
