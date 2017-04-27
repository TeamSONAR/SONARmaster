using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using WiimoteApi;

public class PlayerController : MonoBehaviour
{
    public Camera playerCam;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector2.zero;
    private float speed = 2;

    // headtracking vars
    public bool enableHeadTracking = false; 
    private Wiimote wiimote;
    private GameObject connectWiimote;
    public Canvas wiimoteCanvas;
    FPSControllerWiimote wiiObj;


    // controller vars
    public bool useController = true;
    private float lstick_x;
    private float lstick_y;
    private float rstick_x;
    private float rstick_y;

    void Start()
    {
        if(enableHeadTracking)
        {
            GetComponent<FPSControllerWiimote>().enabled = true;

            // disable keyboard movement scripts
            GetComponent<FirstPersonController>().enabled = false;
        }
        else
        {
            // disable wiimote resources
            GetComponent<FPSControllerWiimote>().enabled = false;
            wiimoteCanvas.enabled = false;
        } 

        if(useController)
        {
            characterController = GetComponent<CharacterController>();
        }
        else // use keyboard input to move
        {
            // enable FPS script. This script will do the rest
            GetComponent<FirstPersonController>().enabled = true;
        } 
    }

    void Update()
    {
        if (enableHeadTracking)
        {
            WiimoteManager.FindWiimotes();
            if (!WiimoteManager.HasWiimote()) { return; }
            wiimote = WiimoteManager.Wiimotes[0];
        }
        if(useController)
        {
            ControllerMovement();
        }
    }
    
    void ControllerMovement()
    {
        GetControllerInput();

        // walking movement
        Vector3 desiredDirection = playerCam.transform.right * lstick_x + playerCam.transform.forward * lstick_y;
        moveDirection.x = desiredDirection.x * speed;
        moveDirection.z = desiredDirection.z * speed;
        characterController.Move(moveDirection * Time.fixedDeltaTime);

        // look rotation movement
        Vector3 lookRotation = new Vector3(rstick_y, rstick_x, 0);
        if(lookRotation != Vector3.zero)
        {
            //Debug.Log(rstick_y + ", " + rstick_x + "\n");
            lookRotation *= 3;
            lookRotation += new Vector3(playerCam.transform.rotation.x, playerCam.transform.rotation.y, 0);
            playerCam.transform.Rotate(lookRotation);
            playerCam.transform.rotation = Quaternion.Euler(playerCam.transform.rotation.eulerAngles.x, playerCam.transform.rotation.eulerAngles.y, 0); 
        } 
    }

    void GetControllerInput()
    {
        lstick_x = Input.GetAxis("lstick_horiz");
        lstick_y = (-1)*Input.GetAxis("lstick_vert");
        rstick_x = Input.GetAxis("rstick_horiz");
        rstick_y = Input.GetAxis("rstick_vert");
    }
}
