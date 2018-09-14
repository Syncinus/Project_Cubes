using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnrenderDespawn : MonoBehaviour {
	void Start() {
       Invoke("Despawn", 2f);
	}

	void OnBecomeInvisible() {
		this.gameObject.SetActive (false);
	}

	void Despawn() {
		GameObject.Destroy(this.gameObject);
	}
}
