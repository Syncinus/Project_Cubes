using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallDissolve : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (this.transform.position.y < -10f)
        {
            Debug.Log("BA-BAM");
            Death();
        }
	}

    public void Death()
    {
        if (this.GetComponent<DestroyableObject>() != null)
        {
            this.GetComponent<DestroyableObject>().TakeDamage(10000000000000000f, this.transform.position, this.gameObject);
        }
    }
}
