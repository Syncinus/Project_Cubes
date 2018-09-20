using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnrenderDespawn : MonoBehaviour {
	void Start() {
       Invoke("Despawn", 4f);
	}

	void OnBecameInvisible()
    {
        this.gameObject.SetActive(false);
    }

	void Despawn() {
		GameObject.Destroy(this.gameObject);
	}
}
