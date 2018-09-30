using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvalidDestroyer : MonoBehaviour {

    private void FixedUpdate()
    {
        if (Vector3.Distance(Vector3.zero, this.transform.position) > 200f) {
            EnemyAI ai = this.GetComponent<EnemyAI>();
            DestroyableObject desObj = this.GetComponent<DestroyableObject>();
            if (ai != null)
            {
                ai.Die();
            }
            if (desObj != null)
            {
                desObj.TakeDamage(13000000000000f, Vector3.zero);
            }

            DestroyImmediate(this.gameObject);
        }
    }
}
