using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPunCallbacks {

    private GameObject finder;
    private string objName;
    private List<GameObject> objects = new List<GameObject>();
    [HideInInspector] public bool isBlockBlaster = false;
    public bool isExplosive = false;
    public float explosionForce = 30000f;
    public float damage = 100f;
    public bool goesForward = true;
    public bool breaks = true;
    public bool mineMode = false;
    public Transform shooter;

    public virtual void Update()
    {
        if (goesForward == true)
        {
            transform.position += transform.forward * Time.deltaTime * 2;
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet" || collision.transform.name == "Bullet(Clone)" || collision.transform.name == "Piece (1)" || collision.transform.name == "Piece (2)" || collision.transform.name == "Piece (3)" || collision.transform.name == "Piece (4)" || collision.transform.name == "Box (Clone)" || collision.gameObject.tag == "Bullet")
        {
            Collider collider = this.GetComponent<Collider>();
            Physics.IgnoreCollision(collision.collider, collider);
            return;
        }

        if (collision.gameObject != shooter.gameObject && collision.transform.name != "Tile(Clone)" && collision.transform.name != "MapGenerator" && collision.transform.name != "Bullet (Clone)" && collision.transform.name != "Floor" || collision.transform.name == "Piece (1)" || collision.transform.name == "Piece (2)" || collision.transform.name == "Piece (3)" || collision.transform.name == "Piece (4)")
        {
            
            //Debug.Log(collision.transform.name);

            if (isBlockBlaster == true)
            {   
                finder = collision.gameObject;
                objName = collision.transform.name;
            }

            GameObject obj = collision.gameObject;

            ShootShots shootingScript = shooter.GetComponent<ShootShots>();
            TerrorizerAI terrorizer = shooter.GetComponent<TerrorizerAI>();

            if (shootingScript != null)
            {
                //shootingScript.OnBulletHit(obj, collision);
            }
            if (terrorizer != null)
            {
                terrorizer.OnBulletHit(obj, collision, damage);
            }

            if (isExplosive == true)
            {
                //Debug.Log("Boom!");
                RaycastHit[] hits = Physics.SphereCastAll(this.transform.position, 10f, this.transform.position, 10f);

                foreach (RaycastHit hit in hits)
                {
                    Rigidbody rigid = hit.transform.GetComponent<Rigidbody>();
                    if (hit.transform != shooter)
                    {
                        if (rigid != null)
                        {
                            rigid.AddForce(-hit.point * 500, ForceMode.Impulse);
                            if (mineMode == true)
                            {
                                rigid.velocity += new Vector3(0, 1, 0);
                            }
                        }
                    }
                }
            }

            if (breaks == true)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }
}
