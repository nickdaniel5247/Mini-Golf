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

    void shoot(float mouseOffset)
    {
        //Can't shoot backwards
        mouseOffset = math.abs(mouseOffset);

        //Only shoot in Z direction which will be forward relative to camera
        Vector3 freelookCamForward = cinemachineFreeLook.State.FinalOrientation.normalized * Vector3.forward;
        freelookCamForward *= mouseOffset;

        rigidBody.AddForce(freelookCamForward);
    }
}
