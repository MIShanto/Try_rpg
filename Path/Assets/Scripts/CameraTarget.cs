using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraTarget : MonoBehaviour
{
    public List<Transform> targets;

    public Vector3 offset;
    public float smoothTime;

    public float minZoom;
    public float maxZoom;
    public float zoomLimiter;

    Vector3 velocity;

    CinemachineVirtualCamera cam;
    [SerializeField] Transform camFollow;

    private void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
    }

    private void LateUpdate()
    {
        if(targets.Count == 0)
        {
            return;
        }

        Move();
        Zoom();
    }

    void Zoom()
    {
        //Debug.LogError(GetGreatestDistance());
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.m_Lens.OrthographicSize = Mathf.Lerp(cam.m_Lens.OrthographicSize, newZoom, Time.deltaTime);
    }

    private float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.size.x;
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;
        camFollow.transform.position = Vector3.SmoothDamp(camFollow.transform.position, newPosition, ref velocity, smoothTime);
    }

    private Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
            return targets[0].position;

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.center;
    }
}
