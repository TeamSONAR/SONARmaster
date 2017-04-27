using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using WiimoteApi;

public class PlayerController : MonoBehaviour
{ 
    // controller vars
    public bool useController = true;
    public float controllerSpeed = 5;
    private float lstick_x;
    private float lstick_y;
    private float rstick_x;
    private float rstick_y;

    // headtracking vars
    public bool enableHeadTracking = false;
    private Wiimote wiimote;
    private GameObject connectWiimote;
    public Canvas wiimoteCanvas;
    FPSControllerWiimote wiiObj;

    public Camera playerCam;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector2.zero;
    private CharacterController m_CharacterController;

    public bool disableCanvas = false;

    void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        if (enableHeadTracking)
        {
            GetComponent<FPSControllerWiimote>().enabled = true; 
            GetComponent<FirstPersonController>().enabled = false;
            if(disableCanvas)
            {
                wiimoteCanvas.enabled = false;
                GetComponent<FPSControllerWiimote>().disableCanvas = true;
            }
                
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
        moveDirection.x = desiredDirection.x * controllerSpeed;
        moveDirection.z = desiredDirection.z * controllerSpeed;
        // check if in air
        if(!characterController.isGrounded)
        {
            moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
        }

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
