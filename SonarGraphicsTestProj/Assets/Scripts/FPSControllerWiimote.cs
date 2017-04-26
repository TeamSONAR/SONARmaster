using UnityEngine;
using System.Collections.Generic;
using WiimoteApi;
using WiimoteApi.Util;
using System.Collections;

public class FPSControllerWiimote : MonoBehaviour
{

    private Wiimote wiimote;
    private NunchuckData nunchuckData; 

    // acceleration data vars  
    private MotionPlusData MPdata;
    private float yaw;
    private float roll;
    private float pitch;
    public Vector3 startingRotation; 
    public float multiplier = 0.05f;
    private Vector3 currentRotation;
    private Vector3 startingAcceleration;
    private Vector3 currentAcceleration;

    public float limit = 2f;

    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public Rigidbody player;
    public List<bool> playerLeds;
    public Camera playerCam;
    public RectTransform IR_pointer;
    public bool setupCompleted { get; set; }

    private bool initIsPressed = false;
    private bool isInit = false;
    private bool isWalking = true;
    private bool isRunning = false;
    private Vector2 currentVelocity;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector2.zero;
    public float moveDirectionSpeed = 150f;

    private Vector2 origin = Vector3.zero;

    void Start()
    {
        setupCompleted = false;
        characterController = GetComponent<CharacterController>();
        isInit = false;
        // Wii Motion Plus takes a moment to be recognized.
        // Wait a second, then see if its activated
        WiimoteManager.FindWiimotes();
        StartCoroutine(WaitForWMP(1f));
        currentRotation = startingRotation;
        startingAcceleration = ReadWiimoteForAccelVector();
        startingAcceleration = ReadWiimoteForAccelVector();

    }

    void Update()
    {
        //Connecti wiimote & IR basic setup
        WiimoteManager.FindWiimotes();
        if (!WiimoteManager.HasWiimote()) { return; }
        

        wiimote = WiimoteManager.Wiimotes[0];
        if (!wiimote.wmp_attached) { return; }
        MPdata = wiimote.MotionPlus;
        if (!isInit)
        {
            wiimote.SetupIRCamera(IRDataType.BASIC);
            isInit = true;
        }
        wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_EXT8); // request accel data
        int response;
        do
        {
            response = wiimote.ReadWiimoteData();
            if (response < 0) { Debug.Log("Error: " + response); }
        } while (response > 0);


        ReadOnlyMatrix<int> ir = wiimote.Ir.ir;
        int dotCount = 4;
        for (int i = 0; i < 4; i++)
        {
            if (ir[i, 0] == -1 || ir[i, 1] == -1)
            {
                dotCount--;
            }
        }
        if (dotCount < 2) { return; }

        //Wiimote camera movement
        float[] pointer = wiimote.Ir.GetPointingPosition();
        Vector2 pointerPos = new Vector2((-1) * pointer[1], pointer[0]);
        if (origin == Vector2.zero)
        {
            origin = pointerPos;
        }

        // show the wiimote pointer on screen
        Vector2 curAnchorMin = IR_pointer.anchorMin;
        Vector2 curAnchorMax = IR_pointer.anchorMax;
        IR_pointer.anchorMin = Vector2.SmoothDamp(curAnchorMin, new Vector2(pointer[0], pointer[1]), ref currentVelocity, 0.1f, 1f, Time.deltaTime);
        IR_pointer.anchorMax = Vector2.SmoothDamp(curAnchorMax, new Vector2(pointer[0], pointer[1]), ref currentVelocity, 0.1f, 1f, Time.deltaTime);

        // grab accel data. Accel data is ignored if its too small to matter, inorder to prevent a shakey camera
        roll = (Mathf.Abs(MPdata.RollSpeed) > limit) ? MPdata.RollSpeed : 0;
        yaw = (Mathf.Abs(MPdata.YawSpeed) > limit) ? MPdata.YawSpeed : 0;
        pitch = (Mathf.Abs(MPdata.PitchSpeed) > limit) ? MPdata.PitchSpeed : 0;


        // if setup, begin headtracking movement
        if (setupCompleted)
        {

            currentRotation = currentRotation + new Vector3((-1) * roll * multiplier, (-1) * yaw * multiplier, 0);
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z); // 0's for x and z because we cant use yaw without wmp, and we dont need roll 


            // these are camera vars
            /*
            // if wiimote moves enough, move camera
            float minDist = 0f;
            if (Mathf.Abs(pointerPos.x - origin.x) > minDist || Mathf.Abs(pointerPos.y - origin.y) > minDist)
            {
                Vector3 diff = new Vector3(pointerPos.x - origin.x, pointerPos.y - origin.y, 0);
                diff *= moveDirectionSpeed;
                playerCam.transform.rotation = Quaternion.Euler(playerCam.transform.rotation.x + diff.x, playerCam.transform.rotation.y + diff.y, playerCam.transform.rotation.z + diff.z);
            }
            */

            if (wiimote != null && wiimote.current_ext != ExtensionController.NONE)
            {
                
                if (wiimote.current_ext == ExtensionController.NUNCHUCK) // nunchuck
                {
                    nunchuckData = wiimote.Nunchuck;
                    isRunning = nunchuckData.z;
                } 
            }
        }
    }

    void FixedUpdate()
    {
        if (wiimote == null && setupCompleted) { return; }
        
        float speed = 0;
        if (isWalking) { speed = walkSpeed; }
        if (isRunning) { speed = runSpeed; }

        if (wiimote != null && wiimote.current_ext != ExtensionController.NONE)
        {
            if (wiimote.current_ext == ExtensionController.NUNCHUCK)
            {
                nunchuckData = wiimote.Nunchuck;
                Vector3 desiredDirection = playerCam.transform.right * AnalogStickMovement(nunchuckData.stick[0]) + playerCam.transform.forward * AnalogStickMovement(nunchuckData.stick[1]);
                moveDirection.x = desiredDirection.x * speed;
                moveDirection.z = desiredDirection.z * speed;

                characterController.Move(moveDirection * Time.fixedDeltaTime);
            }

            Debug.Log(Input.GetAxis("Horizontal"));
        }
    }

    void OnGUI()
    {
        if (wiimote == null) { return; }

        if (!setupCompleted)
        {
            if (StartButtonSequence())
            {
                
                wiimote.RumbleOn = true;
                wiimote.SendStatusInfoRequest();
                wiimote.RumbleOn = false;
                initIsPressed = true;
                // initialize rotation and store it. This is used to calibrate acceleration data headtracking
                transform.rotation = Quaternion.Euler(startingRotation.x, startingRotation.y, startingRotation.z);
                currentRotation = startingRotation;
            }
            else
            {
                if (initIsPressed)
                {
                    setupCompleted = true;
                    initIsPressed = false;
                }
            }
        }

    }

    bool StartButtonSequence()
    { 
        if (wiimote == null) { return false; }


        if (wiimote.current_ext == ExtensionController.NUNCHUCK) // nunchuck
        {
            if (wiimote.Nunchuck.c && wiimote.Nunchuck.z) return true;
        }
        else
        {
            if (wiimote.Button.b && wiimote.Button.a) return true;
        }
        return false;
    }

    float AnalogStickMovement(float data)
    {
        if (data >= 100 && data < 150)
        {
            return 0;
        }
        else if (data >= 200 && data > 150)
        {
            return 1;
        }
        else if (data < 100)
        {
            return -1;
        }
        return 0;
    }

    private Vector3 ReadWiimoteForAccelVector()
    {
        Vector3 accelerationVector;
        if (!WiimoteManager.HasWiimote())
        {
            Debug.LogError("Couldnt find wiimote.");
            return Vector3.zero;
        }

        wiimote = WiimoteManager.Wiimotes[0];

        int ret;
        do
        {
            ret = wiimote.ReadWiimoteData();
        } while (ret > 0);

        // need to call this inorder to update acceleration data
        //wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
        wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_EXT8);

        // get acceleration data as a Vector3
        accelerationVector = GetAccelVector();


        return accelerationVector;
    }


    private Vector3 GetAccelVector()
    {
        float accel_x;
        float accel_y;
        float accel_z;

        float[] accel = wiimote.Accel.GetCalibratedAccelData();
        accel_x = accel[0];
        accel_y = -accel[2];
        accel_z = -accel[1];

        return new Vector3(accel_x, accel_y, accel_z).normalized;
    }

    IEnumerator WaitForWMP(float timeToWait)
    {
        Debug.Log("Waiting for WMP....\n");
        wiimote = WiimoteManager.Wiimotes[0];
        wiimote.RequestIdentifyWiiMotionPlus();
        Debug.Log("Waiting for ssssssss....\n");
        yield return new WaitForSeconds(timeToWait);
        Debug.Log("Waiting for ass....\n");
        if (wiimote.wmp_attached)
        {
            wiimote.ActivateWiiMotionPlus();
            MPdata = wiimote.MotionPlus;

            Debug.Log("Attached!!!!!!!!!!!!\n");
        }
        else
        {
            Debug.LogError("WiiMotion Plus not attached\n");
            StartCoroutine(WaitForWMP(1f));
        }
    }
}
