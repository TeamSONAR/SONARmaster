using UnityEngine;
using System.Collections.Generic;
using WiimoteApi;
using WiimoteApi.Util;
using System.Collections;

public class FPSControllerWiimote : MonoBehaviour
{

    private Wiimote wiimote;
    float[] pointer;
    Vector2 pointerPos;

    // acceleration data vars  
    private MotionPlusData MPdata;
    private float yaw;
    private float roll;
    private float pitch;
    public Vector3 startingRotation = Vector3.zero;
    public float multiplier = 0.05f;
    private Vector3 currentRotation;

    public float limit = 2f;

    public Camera playerCam;
    public RectTransform IR_pointer;
    //public bool disableCanvas = false;
    public bool setupCompleted { get; set; }

    private bool initIsPressed = false;
    private bool isInit = false;
    private Vector2 currentVelocity;
    public float rotationSpeed = 150f;

    private Vector2 origin = Vector3.zero;

    public Quaternion ht_rotation;


    void Start()
    {
        setupCompleted = false;
        isInit = false;
        // Wii Motion Plus takes a moment to be recognized.
        // Wait a second, then see if its activated
        WiimoteManager.FindWiimotes();
        StartCoroutine(WaitForWMP(1f));

        currentRotation = startingRotation;

        //if (disableCanvas)
        //  playerCam.GetComponent<Canvas>().enabled = false;
    }

    void Update()
    {
        if (!WiimoteManager.HasWiimote()) { return; }
        wiimote = WiimoteManager.Wiimotes[0];

        if (!setupCompleted)
        {
            if (StartButtonSequence())
            {

                //wiimote.RumbleOn = true;
                wiimote.SendStatusInfoRequest();
                //wiimote.RumbleOn = false;
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

        MPdata = wiimote.MotionPlus;
        if (!isInit)
        {
            wiimote.SetupIRCamera(IRDataType.BASIC);
            isInit = true;
        }
        //wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_EXT8); // request accel data
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

        bool useAccelerationData = false;
        if (dotCount < 2) useAccelerationData = true;

        Vector2 lastPointerPos = pointerPos;
        GetWiiPointerData();

        // check if wiimote pointer stopped moving
        if (lastPointerPos == pointerPos)
        {
            useAccelerationData = true;
        }
        else
        {
            useAccelerationData = false;
        }

        // Camera and accell data together arent working well
        // so im setting it to just use accelleration data
        useAccelerationData = true;

        // reset camera if you press backpace, or the A button on wiimote
        if (wiimote.Button.a || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Backspace))
        {
            currentRotation = startingRotation;
            MPdata.SetZeroValues();
        }

        if (!setupCompleted)
        {
            transform.rotation = Quaternion.Euler(startingRotation.x, startingRotation.y, startingRotation.z);
            currentRotation = startingRotation;
        }
        else // if setup, begin headtracking movement
        {
            if (playerCam.transform.rotation == Quaternion.Euler(-134.866f, -118.608f, 0.792984f))
            {
                useAccelerationData = true;
            }

            if (useAccelerationData)
            {
                currentRotation = currentRotation + GetAccellData();
                playerCam.transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z); // 0's for x and z because we cant use yaw without wmp, and we dont need roll 
            }
            else
            {
                // if wiimote moves enough, move camera
                float minDist = 0f;
                if (Mathf.Abs(pointerPos.x - origin.x) > minDist || Mathf.Abs(pointerPos.y - origin.y) > minDist)
                {
                    Vector3 diff = new Vector3(pointerPos.x - origin.x, pointerPos.y - origin.y, 0);
                    diff *= rotationSpeed;
                    // hastily added return funcion
                    ht_rotation = Quaternion.Euler(playerCam.transform.rotation.x + diff.x, playerCam.transform.rotation.y + diff.y, playerCam.transform.rotation.z + diff.z);
                    //playerCam.transform.rotation = Quaternion.Euler(playerCam.transform.rotation.x + diff.x, playerCam.transform.rotation.y + diff.y, playerCam.transform.rotation.z + diff.z);
                }
            }
        }
    }

    public Quaternion GetRotation()
    {
        return ht_rotation;
    }

    void GetWiiPointerData()
    {
        pointer = wiimote.Ir.GetPointingPosition();
        pointerPos = new Vector2((-1) * pointer[1], pointer[0]);
        // Debug.Log(pointerPos + "\n");

        //Wiimote camera movement
        if (origin == Vector2.zero)
        {
            origin = pointerPos;
        }

        // show the wiimote pointer on screen
        return;
        //if (disableCanvas) return;
        Vector2 curAnchorMin = IR_pointer.anchorMin;
        Vector2 curAnchorMax = IR_pointer.anchorMax;
        IR_pointer.anchorMin = Vector2.SmoothDamp(curAnchorMin, new Vector2(pointer[0], pointer[1]), ref currentVelocity, 0.1f, 1f, Time.deltaTime);
        IR_pointer.anchorMax = Vector2.SmoothDamp(curAnchorMax, new Vector2(pointer[0], pointer[1]), ref currentVelocity, 0.1f, 1f, Time.deltaTime);
    }

    Vector3 GetAccellData()
    {
        if (!wiimote.wmp_attached) return Vector3.zero;

        wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_EXT8); // request accel data

        // grab accel data.
        roll = (Mathf.Abs(MPdata.RollSpeed) > limit) ? MPdata.RollSpeed : 0;
        yaw = (Mathf.Abs(MPdata.YawSpeed) > limit) ? MPdata.YawSpeed : 0;
        pitch = (Mathf.Abs(MPdata.PitchSpeed) > limit) ? MPdata.PitchSpeed : 0;

        Vector3 accelVector;
        if (!setupCompleted) // Not calibrated yet. 
        {
            accelVector = startingRotation;
        }
        else // calibrated. 
        {
            accelVector = new Vector3(pitch * multiplier, (-1) * yaw * multiplier, 0);
        }
        return accelVector;
    }

    void OnGUI()
    {
        if (wiimote == null) { return; }
    }

    bool StartButtonSequence()
    {
        if (wiimote == null) { return false; }

        if (wiimote.Button.a) return true;
        else
        {
            if (Input.GetKeyDown(KeyCode.Space)) return true;
        }
        return false;
    }

    IEnumerator WaitForWMP(float timeToWait)
    {
        wiimote = WiimoteManager.Wiimotes[0];
        wiimote.RequestIdentifyWiiMotionPlus();
        yield return new WaitForSeconds(timeToWait);
        if (wiimote.wmp_attached)
        {
            wiimote.ActivateWiiMotionPlus();
            MPdata = wiimote.MotionPlus;

            Debug.Log("WMP has been attached!!\n");
        }
        else
        {
            Debug.LogError("WMP not attached\n");
            StartCoroutine(WaitForWMP(1f));
        }
    }
}
