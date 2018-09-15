using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Photon.Pun;
using Photon.Realtime;

public class Missile : Bullet {

    public Transform target;
    public float driveSpeed = 10f;
    public float damping = 10f;
    public string ogName;

    public bool hasSplit = false;

    public bool splitMissile = false;
    public int amountToSplit = 2;
    public float retargetRadius = 100.0f;
    public List<Collider> listColliders;

    Collider[] colliders;

    private bool flying = true;

    private Vector3 velocity = Vector3.zero;
    private float timeSurvived;
    private float launchTime;

    public void Start()
    {
        launchTime = Time.time;
        timeSurvived = launchTime;
    }

    public override void Update()
    {
        timeSurvived += Time.deltaTime;
        if (timeSurvived >= launchTime + 4)
        {
            flying = false;
            this.GetComponent<Rigidbody>().mass = 10f;
            this.GetComponent<Rigidbody>().useGravity = true;
            Destroy(gameObject, 1.0f);
        }

        if (target != null)
        {
            //Quaternion rotation = Quaternion.LookRotation(target.position - this.transform.position);

            //this.transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, Time.deltaTime * damping);

            Quaternion targetRotation = Quaternion.LookRotation(target.position - this.transform.position, Vector3.up);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * damping);

            if (splitMissile == true)
            {
                if (Vector3.Distance(target.position, this.transform.position) < 10.0f && hasSplit == false)
                {
                    bool spawnUp = false;

                    for (int x = 0; x < amountToSplit; x++)
                    {
                        GameObject rocketClone;

                        //Quaternion randomRot = this.transform.rotation;
                        //float randY = UnityEngine.Random.Range(-4.0f, 4.0f);
                        //randomRot.y += randY;

                        Vector3 posToSpawn = this.transform.position;
                        //Vector3 randomPos = this.transform.position;

                        //float randX = UnityEngine.Random.Range(-4.0f, 4.0f);
                        //.x += randX;
                        //randomPos.x += randX;
                        //randomPos.y += randY;

                        
                        if (spawnUp == false)
                        {
                            posToSpawn = this.transform.position + this.transform.up * 0.4f;
                            spawnUp = true;
                        } else
                        {
                            posToSpawn = this.transform.position + (-this.transform.up) * 0.4f;
                            spawnUp = false;
                        }
                        

                        rocketClone = PhotonNetwork.Instantiate(ogName, posToSpawn, this.transform.rotation, 0);
                        rocketClone.transform.LookAt(target);
                        Destroy(rocketClone.GetComponent<Bullet>());
                        rocketClone.transform.localScale = new Vector3(this.transform.localScale.x / 2, this.transform.localScale.y / 2, this.transform.localScale.z / 2);
                        rocketClone.transform.SetParent(GameObject.Find("TempStorage").transform);
                        rocketClone.gameObject.AddComponent<Missile>();
                        rocketClone.GetComponent<Missile>().target = target;
                        rocketClone.GetComponent<Missile>().shooter = shooter;
                        rocketClone.GetComponent<Missile>().hasSplit = true;
                        rocketClone.GetComponent<Missile>().StartCoroutine(SpeedUpToFast(driveSpeed * 2, 0.3f));
                        if (isExplosive == true)
                        {
                            rocketClone.GetComponent<Missile>().isExplosive = true;
                        }
                    }

                    hasSplit = true;
                    flying = false;
                    this.GetComponent<Rigidbody>().mass = 5f;
                    this.GetComponent<Rigidbody>().useGravity = true;
                    Destroy(gameObject, 1.0f);
                }
            }
        }
        
        if (target == null)
        {
            //RaycastHit hitInfo;
            colliders = Physics.OverlapSphere(this.transform.position, retargetRadius);
            listColliders = colliders.ToList();

            for (int x = 0; x < listColliders.Count; x++)
            {
                if (listColliders.ElementAt(x).transform.GetComponent<PlayerCube>() == null)
                {
                    listColliders.RemoveAt(x);
                }
            }

            target = listColliders.ElementAt(0).transform;
        }

        if (flying == true)
        {
            //this.transform.Translate(Vector3.forward * Time.deltaTime * driveSpeed);
            this.transform.position = Vector3.SmoothDamp(this.transform.position, target.TransformPoint(UnityEngine.Random.Range(0f, 0.5f), UnityEngine.Random.Range(0f, 0.5f), UnityEngine.Random.Range(0f, 0.5f)), ref velocity, 0.3f, driveSpeed);
            driveSpeed += 0.01f;
        }
    }

    public IEnumerator SpeedUpToFast(float MaximumSpeed, float Time)
    {
        float StartSpeed = 0.1f;
        float T = 0f;
        
        while (T < Time)
        {
            driveSpeed = Mathf.Lerp(StartSpeed, MaximumSpeed, T / Time);
            yield return null;
        }

        driveSpeed = MaximumSpeed;
    }
}
