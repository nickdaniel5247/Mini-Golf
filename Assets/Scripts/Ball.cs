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
    private const string boundsObjectName = "Bounds";
    private const string managersObjectName = "Managers";

    private GameManager gameManager;
    private UIManager uiManager;
    private AudioManager audioManager;

    public GameObject directionPrefab;
    private GameObject direction;
    public float directionLightRange = 5f;

    private int strokeCount = 0;
    private Vector3 lastShotPos;

    private bool levelCompleted = false;
    private bool mouseDown = false;

    public float maxForce = 350f;
    public float deaccelerationStart = 0.1f;
    public float deaccelerationSpeed = 2.5f;
    public float levelCompleteWaitTime = 2f; //Wait time before ending level

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

        audioManager = AudioManager.Instance;

        if (!audioManager)
        {
            Debug.LogError("No AudioManager instance is set.");
        }

        //Not neccessary but just precautionary
        lastShotPos = gameManager.spawnPoint.transform.position;
    }

    void Update()
    {
        if (GameManager.CurrentState != GameManager.GameState.Playing || !rigidBody.IsSleeping())
        {
            return;
        }

        if (!direction)
        {
            this.transform.rotation = Quaternion.identity;
            direction = Instantiate(directionPrefab, this.transform);
            direction.GetComponent<Light>().range = 0;
        } 
        
        Vector3 freelookCamForward = cinemachineFreeLook.State.FinalOrientation * Vector3.forward;
        //With unit vector of (0, 1), find angle to freelookcam direction
        float angle = Vector2.SignedAngle(new Vector2(freelookCamForward.x, freelookCamForward.z), new Vector2(0f, 1f));
        direction.transform.eulerAngles = new Vector3(0f, angle, 0f);

        if (mouseDown)
        {
            float percent = Mathf.InverseLerp(0, Screen.height, Mathf.Abs(startingMouseHeight - Input.mousePosition.y));
            direction.GetComponent<Light>().range = percent * directionLightRange;
        }

        //mouseDown variable is used to recognize that we processed this input rather than the UI
        if (Input.GetMouseButtonDown(mouseDownCode))
        {
            startingMouseHeight = Input.mousePosition.y;

            cinemachineFreeLook.m_XAxis.m_InputAxisName = "";
            cinemachineFreeLook.m_YAxis.m_InputAxisName = "";

            mouseDown = true;
        } //UI only gives back mouse up so we check we processed a down here
        else if (Input.GetMouseButtonUp(mouseDownCode) && mouseDown)
        {
            shoot(startingMouseHeight - Input.mousePosition.y);

            cinemachineFreeLook.m_XAxis.m_InputAxisName = "Mouse X";
            cinemachineFreeLook.m_YAxis.m_InputAxisName = "Mouse Y";

            mouseDown = false;
            Destroy(direction);
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

        if (strokeCount == gameManager.strokeLimit && rigidBody.IsSleeping() && !levelCompleted)
        {
            gameManager.RestartLevel();
        }
    }

    void shoot(float mouseOffset)
    {
        if (!rigidBody.IsSleeping() || strokeCount == gameManager.strokeLimit || levelCompleted)
        {
            //We are in motion or reached our shoot limit, no further movement should be done
            return;
        }

        lastShotPos = transform.position;

        //Can't shoot backwards
        mouseOffset = math.abs(mouseOffset);

        //Shoot in forward direction relative to camera
        Vector3 freelookCamForward = cinemachineFreeLook.State.FinalOrientation.normalized * Vector3.forward;
        
        //Remove height dimension and scale by mouse delta
        freelookCamForward.y = 0f;
        freelookCamForward *= mouseOffset;

        freelookCamForward = Vector3.ClampMagnitude(freelookCamForward, maxForce);
        rigidBody.AddForce(freelookCamForward);

        ++strokeCount;
        uiManager.UpdateStrokeCount(strokeCount);

        audioManager.PlaySoundEffect(audioManager.ballHit);
    }

    private IEnumerator completedLevel()
    {
        levelCompleted = true;
        audioManager.PlaySoundEffect(audioManager.holeSound);
        yield return new WaitForSeconds(levelCompleteWaitTime);
        gameManager.LevelCompleted();
    }

    void OnTriggerEnter(Collider hole)
    {
        if (hole.name != holeObjectName)
        {
            return;
        }

        StartCoroutine(completedLevel());
    }

    void OnTriggerExit(Collider bounds)
    {
        if (bounds.name != boundsObjectName)
        {
            return;
        }

        gameObject.transform.position = lastShotPos;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
    }
}