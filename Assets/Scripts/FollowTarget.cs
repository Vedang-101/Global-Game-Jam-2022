using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform Target;
    public bool TrackPosition = true;
    public bool TrackRotation = true;

    public Vector3 TrackingOffset;
    public float TrackingLag = 5f;

    void Update()
    {
        if(TrackPosition)
            transform.position = Vector3.Lerp(transform.position, Target.position + TrackingOffset, TrackingLag * Time.deltaTime);
        if(TrackRotation)
            transform.rotation = Target.rotation;
    }
}
