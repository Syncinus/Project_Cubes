using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour {

    public Transform follow;

    public void FixedUpdate()
    {
        this.transform.position = follow.transform.position;
    }
}
