using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;
    public GameObject mainCamera;
    public float lagSpeed = 5.0f;
    public float rotationalSpeed = 75.0f;

    float yaw = 0.0f;
    public Vector3 offset;

    public void UpdateCamera()
    {
        if (target == null)
            return;
        transform.position = Vector3.Lerp(transform.position, target.transform.position + offset, Time.deltaTime * lagSpeed);
        yaw += rotationalSpeed * Input.GetAxis("Mouse X");
        transform.eulerAngles = new Vector3(0, yaw, 0);
    }
}