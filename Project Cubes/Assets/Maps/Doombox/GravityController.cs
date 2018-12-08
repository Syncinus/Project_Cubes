using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour {

    private float nextGravityChange;

	void Update () {
		if (Time.time > nextGravityChange)
        {
            ChangeGravity();
        }
	}

    void ChangeGravity()
    {
        int GravityDirection = Random.Range(1, 6);
        float GravityForce = Random.Range(0f, 30f);

        switch(GravityDirection)
        {
            case 1:
                Physics.gravity = GravityForce * Vector3.down;
                break;
            case 2:
                Physics.gravity = GravityForce * Vector3.up;
                break;
            case 3:
                Physics.gravity = GravityForce * Vector3.left;
                break;
            case 4:
                Physics.gravity = GravityForce * Vector3.right;
                break;
            case 5:
                Physics.gravity = GravityForce * Vector3.forward;
                break;
            case 6:
                Physics.gravity = GravityForce * Vector3.back;
                break;
        }
        nextGravityChange = Time.time + 5f;
    }
}
