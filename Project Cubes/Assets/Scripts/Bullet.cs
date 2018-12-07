using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPunCallbacks
{
    public GameObject shooter;

    [HideInInspector] public ShotMode Shooting;
    [HideInInspector] public int index;

    public float speed = 1f;
    public float maxRange = 10f;

    float distanceTraveled;
    Vector3 lastPosition;

    public void Start()
    {
        lastPosition = transform.position;
    }

    public void Fire()
    {
        this.GetComponent<Rigidbody>().velocity = this.transform.forward * speed;
    }

    public void FixedUpdate()
    {
        distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        if (distanceTraveled >= maxRange)
        {
            PhotonNetwork.Destroy(this.photonView);
        }

        this.GetComponent<Rigidbody>().velocity = this.transform.forward * speed;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.name == shooter.transform.name)
        {
            Physics.IgnoreCollision(this.GetComponent<Collider>(), collision.collider);
        }
        else
        {
            ContactPoint contact = collision.contacts[0];
            if (collision.gameObject != null)
            {
                shooter.GetComponent<ShootShots>().Damage(collision.gameObject, contact.point, Shooting, index);
            }
            PhotonNetwork.Destroy(this.photonView);
        }
    }
}
