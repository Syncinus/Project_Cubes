using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour {

    public float dampTime = 0.05f;
	private Vector3 velocity = Vector3.zero;
	public Transform target;

	public void Update() {
		if (target != null && this.gameObject.GetComponent<Camera>()) {
			Vector3 point = Camera.main.WorldToViewportPoint (target.position);
			Vector3 delta = target.position - Camera.main.WorldToViewportPoint (new Vector3 (0.5f, 0.5f, point.z));
			Vector3 destination = this.transform.position + delta;
			transform.position = Vector3.SmoothDamp (transform.position, destination, ref velocity, dampTime);
		}
	}
}
