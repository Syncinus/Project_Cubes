using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : MonoBehaviour {

	// Use this for initialization

    /*
	public void Start () {

    }
    */
	
	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(Vector3.zero, this.transform.position) > 101f)
        {
            Debug.Log("ZZZZZZZZT!");
        }
	}
}
