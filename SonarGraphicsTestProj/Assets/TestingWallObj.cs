using UnityEngine;

public class TestingWallObj : MonoBehaviour {

    // i know exactly what im doing
    [HideInInspector]
    public GameObject wallPart;
    [HideInInspector]
    public Camera playerView;

    // wall parts that are summoned on start
    GameObject middleLeft;
    GameObject middleRight;
    GameObject upLeft;
    GameObject upMiddle;
    GameObject upRight;
    GameObject downLeft;
    GameObject downMiddle;
    GameObject downRight;

    float wallPartWidth;
    Quaternion collectiveRotation;
     
    // vars for moving wall back and forth
    public bool MoveBackAndForth = true;
    public float speed = 5f;
    [Range(1.0f, 10.0f)] 
    public float distance = 1f;
    Vector3 origin;
    Vector3 directionVector;
    bool movingBack = true;

    // vars for activating and deactivating wall parts
    public bool MiddleLeft_Activated = true;
    public bool MiddleMiddle_Activated = true;
    public bool MiddleRight_Activated = true;
    public bool UpLeft_Activated = true;
    public bool UpMiddle_Activated = true;
    public bool UpRight_Activated = true;
    public bool DownLeft_Activated = true;
    public bool DownMiddle_Activated = true;
    public bool DownRight_Activated = true;
    

    void Start () {

        origin = gameObject.transform.position;
        directionVector = new Vector3(0f, 0f, -.1f);
        wallPartWidth = gameObject.GetComponent<Renderer>().bounds.size.y;

        // initialize wall parts
        
        middleLeft = (GameObject)Instantiate(wallPart, gameObject.transform.position + Vector3.left * wallPartWidth, transform.rotation);
        // middleMiddle is the test wall controller. Its called with gameObject
        middleRight = (GameObject)Instantiate(wallPart, gameObject.transform.position + Vector3.right * wallPartWidth, transform.rotation);
        upLeft = (GameObject)Instantiate(wallPart, gameObject.transform.position + (Vector3.up + Vector3.left) * wallPartWidth, transform.rotation);
        upMiddle = (GameObject)Instantiate(wallPart, gameObject.transform.position + Vector3.up * wallPartWidth, transform.rotation);
        upRight = (GameObject)Instantiate(wallPart, gameObject.transform.position + (Vector3.up + Vector3.right) * wallPartWidth, transform.rotation);
        downLeft = (GameObject)Instantiate(wallPart, gameObject.transform.position + (Vector3.down + Vector3.left) * wallPartWidth, transform.rotation);
        downMiddle = (GameObject)Instantiate(wallPart, gameObject.transform.position + Vector3.down * wallPartWidth, transform.rotation);
        downRight = (GameObject)Instantiate(wallPart, gameObject.transform.position + (Vector3.down + Vector3.right) * wallPartWidth, transform.rotation);


    }

    void Update ()
    {

        // deactivate any selected wall parts
        middleLeft.GetComponent<MeshRenderer>().enabled = MiddleLeft_Activated;
        gameObject.GetComponent<MeshRenderer>().enabled = MiddleMiddle_Activated;
        middleRight.GetComponent<MeshRenderer>().enabled = MiddleRight_Activated;
        upLeft.GetComponent<MeshRenderer>().enabled = UpLeft_Activated;
        upMiddle.GetComponent<MeshRenderer>().enabled = UpMiddle_Activated;
        upRight.GetComponent<MeshRenderer>().enabled = UpRight_Activated;
        downLeft.GetComponent<MeshRenderer>().enabled = DownLeft_Activated;
        downMiddle.GetComponent<MeshRenderer>().enabled = DownMiddle_Activated;
        downRight.GetComponent<MeshRenderer>().enabled = DownRight_Activated;

        if (MoveBackAndForth)
        {
            // determine if wall should change direction
            if (movingBack && Mathf.Abs(gameObject.transform.position.z - origin.z) > distance)
            {
                directionVector *= -1;
                movingBack = false;
            }
            else if (!movingBack && Mathf.Abs(gameObject.transform.position.z - origin.z) < .5f)
            {
                directionVector *= -1;
                movingBack = true;
            }

            // move all wallparts now
            gameObject.transform.position += (directionVector * speed);
            middleLeft.transform.position += (directionVector * speed);
            middleRight.transform.position += (directionVector * speed);
            upLeft.transform.position += (directionVector * speed);
            upMiddle.transform.position += (directionVector * speed);
            upRight.transform.position += (directionVector * speed);
            downLeft.transform.position += (directionVector * speed);
            downMiddle.transform.position += (directionVector * speed);
            downRight.transform.position += (directionVector * speed);
        }
    }
}
