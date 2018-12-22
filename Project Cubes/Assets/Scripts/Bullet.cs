using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Chronos;

public class Bullet : MonoBehaviourPunCallbacks
{
    public Transform shooter;

    [HideInInspector] public ShotMode Shooting;
    [HideInInspector] public int index;

    public float speed = 1f;
    public float maxRange = 10f;
    public float force = 10f;

    float distanceTraveled;
    Vector3 lastPosition;

    bool active = true;

    public void Start()
    {
        lastPosition = transform.position;
    }

    public void Fire()
    {
        //Do nothing right now. 
        //this.GetComponent<Rigidbody>().velocity = this.transform.forward * speed;
    }

    public void FixedUpdate()
    {
        if (active == true)
        {
            distanceTraveled += Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;

            if (distanceTraveled >= maxRange)
            {
                StartCoroutine(Collide(this.transform.position + -this.transform.forward));
                //PhotonNetwork.Destroy(this.photonView);
            }

            this.transform.Translate(Vector3.forward * speed * this.GetComponent<Timeline>().fixedDeltaTime, Space.Self);
        }
        //this.GetComponent<Rigidbody>().velocity = this.transform.forward * speed;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == shooter || collision.transform.name.Contains("Floor") || collision.gameObject.tag == "Bullets")
        {
            Collider tc = this.transform.GetComponent<Collider>();
            Collider hc = collision.collider;
            Physics.IgnoreCollision(tc, hc);
        }
        else
        {
            ContactPoint contact = collision.contacts[0];
            if (collision.gameObject != null)
            {
                shooter.GetComponent<ShootShots>().Damage(collision.gameObject, contact.point, Shooting, index, shooter.gameObject);
            }
            StartCoroutine(Collide(contact.normal));
        }
    }

    public IEnumerator Collide(Vector3 direction)
    {
        active = false;
        this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        this.GetComponent<Rigidbody>().AddForce(-direction * force, ForceMode.Force);
        this.GetComponent<Rigidbody>().useGravity = true;
        yield return new WaitForSeconds(1.5f);
        PhotonNetwork.Destroy(this.photonView);
    }
}
