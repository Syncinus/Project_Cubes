using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternateCameraScript : MonoBehaviour {
    public Transform target;
    public float walkDistance;
    public float runDistance;
    public float height;
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;

    private Transform _myTransform;
    private float x;
    private float y;
    private bool camButtonDown = false;

    // Use this for initialization
    void Start()
    {
        if (target == null)
            Debug.LogWarning("We do not have a target");

        _myTransform = transform;
        CameraSetUp();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {  //Use the Input Manager to make this user selectable button
            camButtonDown = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            camButtonDown = false;
        }
    }

    //this function is called after all of the Update functions are done.
    void LateUpdate()
    {
        //      _myTransform.position = new Vector3(target.position.x, target.position.y + height, target.position.z - walkDistance);
        //      _myTransform.LookAt(target);       

        if (camButtonDown)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            //      y = ClamAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -walkDistance) + target.position;

            _myTransform.rotation = rotation;
            _myTransform.position = position;

        }
        else
        {
            _myTransform.position = new Vector3(target.position.x, target.position.y + height, target.position.z - walkDistance);
            _myTransform.LookAt(target);
            x = 0;
            y = 0;
        }

    }

    public void CameraSetUp()
    {
        _myTransform.position = new Vector3(target.position.x, target.position.y + height, target.position.z - walkDistance);
        _myTransform.LookAt(target);
    }
}
 

