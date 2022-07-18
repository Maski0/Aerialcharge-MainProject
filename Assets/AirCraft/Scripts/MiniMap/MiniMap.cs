using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MiniMap : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public CinemachineSmoothPath racePath;

    public GameObject localPlayer;
    public GameObject ball;
    public GameObject minimapCam;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        int numCheckpoints = (int)racePath.MaxUnit(CinemachinePathBase.PositionUnits.PathUnits);
        lineRenderer.positionCount = numCheckpoints+1;
        for(int x = 0; x < numCheckpoints; x++)
        {
            lineRenderer.SetPosition(x, racePath.m_Waypoints[x].position);
        }
        lineRenderer.startWidth = 35f;
        lineRenderer.endWidth = 35f;
        lineRenderer.SetPosition(numCheckpoints, lineRenderer.GetPosition(0));
    }

    void Update()
    {
        minimapCam.transform.position = (new Vector3(localPlayer.transform.position.x,
            minimapCam.transform.position.y, localPlayer.transform.position.z));
        ball.transform.position = (new Vector3(localPlayer.transform.position.x,
            ball.transform.position.y, localPlayer.transform.position.z));


    }
}
