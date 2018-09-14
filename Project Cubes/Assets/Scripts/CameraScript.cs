using System.Collections;
using UnityEngine;

public class CameraScript : MonoBehaviour {
	public bool smoothMode = false;

	public Transform target;
	public float distance = 3.0f;
	public float height = 3.0f;
	public float damping = 5.0f;
	public bool smoothRotation = true;
	public bool followBehind = true;
	public float rotationDamping = 10.0f;

	// Update is called once per frame
	void Update () {
		if (smoothMode == false) {
			this.transform.position = new Vector3 (target.position.x, target.position.y + 3, target.position.z - 5);
		}

		if (smoothMode == true) {
			Vector3 wantedPosition;
			if (followBehind)
				wantedPosition = target.TransformPoint (0, height, -distance);
			else
				wantedPosition = target.TransformPoint (0, height, distance);

			transform.position = Vector3.Lerp (transform.position, wantedPosition, Time.deltaTime * damping);

			if (smoothRotation) {
				Quaternion wantedRotation = Quaternion.LookRotation (target.position - transform.position, target.up);
				transform.rotation = Quaternion.Lerp (transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
			} else
				transform.LookAt (target, target.up);
		}
	}
}
