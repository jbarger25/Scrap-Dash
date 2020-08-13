using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform;

    public Vector3 cameraOffset;

    [Range(0.01f, 1.0f)]
    public float smoothFactor = 0.1f;

    public bool lookAtPlayer = false;
    public bool rotateAroundPlayer = true;
    public float rotationSpeed = 10.0f;
    //// Start is called before the first frame update
    //void Start()
    //{
    //    playerTransform = GameObject.Find("Player").GetComponent<Transform>();
    //    cameraOffset = transform.position - playerTransform.position;
    //}

    // Update is called once per frame
    void Update()
    {
        if (rotateAroundPlayer)
        {
            Quaternion cameraAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationSpeed, Vector3.up);
            cameraOffset = cameraAngle * cameraOffset;
        }
        Vector3 newPosition = playerTransform.position + cameraOffset;
        transform.position = Vector3.Slerp(transform.position, newPosition, smoothFactor);

        if(lookAtPlayer || rotateAroundPlayer)
        {
            transform.LookAt(playerTransform);
        }
    }
}
