using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody rigidBody;
    private float startingMouseHeight;
    private CinemachineFreeLook cinemachineFreeLook;

    private const int mouseDownCode = 0;
    private const float speedEpsilon = 0.01f;

    private const string holeObjectName = "Hole";
    private const string managersObjectName = "Managers";

    private GameManager gameManager;
    private UIManager uiManager;
    private int strokeCount = 0;

    public float maxForce = 350f;
    public float deaccelerationStart = 0.1f;
    public float deaccelerationSpeed = 2.5f;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();

        if (!rigidBody)
        {
            Debug.LogError("No rigidbody attached to golf ball object.");
        }

        cinemachineFreeLook = GetComponentInChildren<CinemachineFreeLook>();

        if (!cinemachineFreeLook)
        {
            Debug.LogError("No cinemachine free look found in children of golf ball object.");
        }

        GameObject managersObject = GameObject.Find(managersObjectName);

        if (!managersObject)
        {
            Debug.LogError("Can't find Managers object.");
        }

        gameManager = managersObject.GetComponentInChildren<GameManager>();

        if (!gameManager)
        {
            Debug.LogError("Can't find GameManager script in Managers's children.");
        }

        uiManager = managersObject.GetComponentInChildren<UIManager>();

        if (!uiManager)
        {
            Debug.LogError("Can't find UIManager script in Managers's children.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(mouseDownCode))
        {
            startingMouseHeight = Input.mousePosition.y;

            cinemachineFreeLook.m_XAxis.m_InputAxisName = "";
            cinemachineFreeLook.m_YAxis.m_InputAxisName = "";
        }
        else if (Input.GetMouseButtonUp(mouseDownCode))
        {
            shoot(startingMouseHeight - Input.mousePosition.y);

            cinemachineFreeLook.m_XAxis.m_InputAxisName = "Mouse X";
            cinemachineFreeLook.m_YAxis.m_InputAxisName = "Mouse Y";
        }
    }

    void FixedUpdate()
    {
        //Help stop ball when slow enough
        if (rigidBody.velocity.magnitude > speedEpsilon && rigidBody.velocity.magnitude < deaccelerationStart)
        {
            float speed = deaccelerationSpeed * Time.deltaTime;

            rigidBody.velocity = new Vector3(Mathf.Lerp(rigidBody.velocity.x, 0, speed), 0, Mathf.Lerp(rigidBody.velocity.z, 0, speed));
            rigidBody.angularVelocity = new Vector3(Mathf.Lerp(rigidBody.angularVelocity.x, 0, speed), 0, Mathf.Lerp(rigidBody.angularVelocity.z, 0, speed));
        }

        if (strokeCount == gameManager.strokeLimit && rigidBody.IsSleeping())
        {
            gameManager.RestartLevel();
        }
    }

    void shoot(float mouseOffset)
    {
        if (!rigidBody.IsSleeping() || strokeCount == gameManager.strokeLimit)
        {
            //We are in motion or reached our shoot limit, no further movement should be done
            return;
        }

        //Can't shoot backwards
        mouseOffset = math.abs(mouseOffset);

        //Only shoot in Z direction which will be forward relative to camera
        Vector3 freelookCamForward = cinemachineFreeLook.State.FinalOrientation.normalized * Vector3.forward;
        freelookCamForward *= mouseOffset;

        freelookCamForward = Vector3.ClampMagnitude(freelookCamForward, maxForce);
        rigidBody.AddForce(freelookCamForward);

        ++strokeCount;
        uiManager.UpdateStrokeCount(strokeCount);
    }

    void OnTriggerEnter(Collider hole)
    {
        if (hole.name != holeObjectName)
        {
            return;
        }

        gameManager.LevelCompleted();
    }
}