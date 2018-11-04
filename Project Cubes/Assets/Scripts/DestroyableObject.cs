using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using EZCameraShake;
using Photon.Pun;
using Photon.Realtime;

public class DestroyableObject : MonoBehaviourPunCallbacks {

	public float maxHealth = 50f;
    public float ScoreGiven = 1f;
	public float health;
	public bool isAlive = true;
	public GameObject brokenVersion;
	public bool brokenModel = false;
	public Transform partStorage;
	public bool layeredExplosion = true;

	private GameObject brokenModelSpawned;
	private new PhotonView photonView;
	private bool explosionPossible = true;

	public void Start() {
		if (this.transform.name == "Obstacle(Clone)") {
			maxHealth = Mathf.Round(this.transform.localScale.y * 100000);
		}
		partStorage = GameObject.Find ("TempStorage").transform;
		health = maxHealth;
	    photonView = PhotonView.Get(this);
    }

	public void FixedUpdate() {
		if (health <= 0f) {
			isAlive = false;
		}

		if (isAlive == false) {
			if (this.GetComponent<EnemyAI>() != null) {
                this.GetComponent<EnemyAI>().Die();
				Destroy(this.gameObject);
			} else {
				if (this.GetComponent<PlayerCube>() != null) {
                    Transform armHolder = this.transform.Find("ArmorHolder");
                    Transform armorHolder = Instantiate(armHolder, armHolder.position, armHolder.transform.rotation);
                    armorHolder.transform.SetParent(this.transform.parent);

                    foreach (Transform armorPiece in armorHolder)
                    {
                        //armorPiece.transform.SetParent(this.transform.parent);
       
                        foreach (Transform piece in armorPiece)
                        {
                            piece.gameObject.AddComponent<Rigidbody>();
                            piece.gameObject.AddComponent<UnrenderDespawn>();
                            Rigidbody body = piece.GetComponent<Rigidbody>();
                            body.mass = 1f;
                            body.useGravity = true;
                            body.AddForce(Vector3.forward * 100, ForceMode.Impulse);
                        }
                    }

                    this.gameObject.SetActive(false);
                    Invoke("RPCRespawn", 1f);
				} else {
					Destroy(this.gameObject);
				}
			}
		}
	}

    
	public void TakeDamage (float amount, Vector3 point, GameObject attacker) {
		//health -= amount;
		if (photonView != null && this.GetComponent<EnemyAI>() == null) {
		    photonView.RPC("PunTakeDamage", RpcTarget.All, amount, point, attacker);
		} else {
            AntiNetworkTakeDamage(amount, point, attacker);
		}
	}

	public void RPCRespawn() {
		photonView.RPC ("Respawn", RpcTarget.All);
	}

	[PunRPC] public void Respawn() {
		this.gameObject.SetActive(true);
	    health = maxHealth;
		this.transform.position = GameObject.Find("PlayerSpawn").transform.position;
		explosionPossible = true;
		isAlive = true;
		if (photonView.IsMine == true) {
			Transform cam = Camera.main.transform;
			if (cam != null) {
			    cam.transform.SetParent(this.transform);
			    cam.GetComponent<SmoothCameraAdvanced>().enabled = true;
				cam.transform.position = new Vector3(0, 1, -3);
			}
		}
	}


    public void AntiNetworkTakeDamage(float amount, Vector3 point, GameObject attacker) {
		health -= amount;
		if (this.GetComponent<PlayerCube> () != null) {
			this.GetComponent<PlayerCube> ().timeScinceLastTimeTakingDamage = 0;
		}
		if (health <= 0f) {
			if (this.GetComponent<EnemyAI>() == null) {
			    EndWithoutView(point, attacker);
			} else {
				photonView.RPC("End", RpcTarget.All, point, attacker);
			}
		}
	}

	[PunRPC] public void PunTakeDamage(float amount, Vector3 point, GameObject attacker) {
		health -= amount;
		if (this.GetComponent<PlayerCube> () != null) {
			this.GetComponent<PlayerCube> ().timeScinceLastTimeTakingDamage = 0;
		}
		if (health <= 0f) {
			//Break (point);
			if (photonView != null) {
			    photonView.RPC("End", RpcTarget.All, point, attacker);
			}
		}
	}

    public void EndWithoutView(Vector3 point, GameObject attacker)
    {
        PrepareDeath();
        RunExplosionAnimation(point);
        if (this.GetComponent<EnemyAI>() != null)
        {
            photonView.RPC("SetAliveness", RpcTarget.All, false);
            isAlive = false;
            this.transform.GetComponent<EnemyAI>().Die();
        }
        else
        {
            if (this.GetComponent<PlayerCube>() != null)
            {
                this.gameObject.SetActive(false);
                if (attacker.GetComponent<PlayerCube>() != null)
                {
                    attacker.GetComponent<PlayerCube>().AddScore(ScoreGiven);
                }
                //(ShakeCamera(5f, 3f, 0.1f, 1.5f));
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }

	[PunRPC] public void SetAliveness(bool alive) {
		isAlive = alive;
	}

    [PunRPC]
    public void End(Vector3 point, GameObject attacker)
    {
        PrepareDeath();
        RunExplosionAnimation(point);
        if (this.GetComponent<EnemyAI>() != null)
        {
            photonView.RPC("SetAliveness", RpcTarget.All, false);
            isAlive = false;
            this.transform.GetComponent<EnemyAI>().Die();
        }
        else
        {
            if (photonView.IsMine == true)
            {
                isAlive = false;

                if (this.GetComponent<PlayerCube>() == null)
                {
                    PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
                    if (attacker.GetComponent<PlayerCube>() != null)
                    {
                        attacker.GetComponent<PlayerCube>().AddScore(ScoreGiven);
                    }
                } else
                {
                    //StartCoroutine(ShakeCamera(5f, 3f, 0.1f, 1.5f));
                }
            }
            //if (PhotonNetwork.isMasterClient) {
            //PhotonNetwork.Destroy(this.photonView);
            //}
            //Destroy(w);
        }
    }


	public void RunExplosionAnimation(Vector3 point) {
		if (explosionPossible == true) {
		if (layeredExplosion == true) {
			float heightLevel = this.transform.localScale.y;
			for (float x = 0.25f; x <= heightLevel; x += 0.25f) {
			    ExplosionAnimation(point);
			}
		}
		if (layeredExplosion == false) {
			    ExplosionAnimation(point);
		}
		explosionPossible = false;
		}
	}

	public void PrepareDeath() {
		if (this.GetComponent<PlayerCube>() != null) {
			if (photonView.IsMine == true) {
				   Transform cam = Camera.main.transform;
				   if (cam != null) {
					    cam.transform.SetParent(this.transform.parent);
					    cam.GetComponent<SmoothCameraAdvanced>().enabled = false;
				   }
			}
			if (this.transform.GetComponent<EnemyAI>() != null) {
                this.transform.GetComponent<EnemyAI>().Die();
			}
		}
	}



	public void ExplosionAnimation(Vector3 point) {
		GameObject brokenCube = (GameObject)GameObject.Instantiate (brokenVersion, new Vector3(this.transform.position.x, this.transform.position.y + 0.25f, this.transform.position.z), this.transform.rotation) as GameObject;
		brokenCube.transform.SetParent (partStorage);
		brokenModelSpawned = brokenCube;
		
            foreach(Transform colorChange in brokenCube.transform) {
				Renderer ccrend = colorChange.GetComponent<Renderer>();
				Material ccmat = new Material(ccrend.sharedMaterial);

                ccmat.color = this.GetComponent<Renderer>().sharedMaterial.color;
				ccrend.sharedMaterial = ccmat;
			}
		
				Rigidbody rigidThing = brokenCube.AddComponent<Rigidbody> ();
				rigidThing.AddForce ((point - rigidThing.transform.position).normalized * 10f, ForceMode.Force);
				//rigidThing.AddExplosionForce (100f, brokenCube.transform.position, 10f, 3.0f, ForceMode.Force);
				Transform[] allChildren = brokenCube.GetComponentsInChildren<Transform> ();
				foreach (Transform child in allChildren) {
					if (child.GetComponent<Rigidbody> () != null) {
						Rigidbody rigid = child.GetComponent<Rigidbody> ();
						Light lit = child.gameObject.AddComponent<Light> ();
						lit.range = 5.0f;
						//rigid.AddForceAtPosition(40f * rigidThing.transform.forward, point, ForceMode.Force);
						var dir = point - child.position;
						dir = dir.normalized;
						rigid.AddForce(-dir * 15f, ForceMode.Impulse);
						rigid.AddExplosionForce (30f, child.position, 10f, 2.0f, ForceMode.Force);
					}
				}
				RaycastHit[] hits = Physics.SphereCastAll (brokenCube.transform.position, 10f, brokenCube.transform.position, 20f);
				foreach (RaycastHit hit in hits) {
					//Debug.Log (hit.transform.name);
					Rigidbody rigidb = hit.transform.GetComponent<Rigidbody> ();
					if (rigidb != null) {
						//rigidb.AddForce (-hit.normal * 500f);
				}
		}
	}





    public void Break(Vector3 point)
    {
        //Destroy (gameObject);
        if (brokenModel == true)
        {

            if (this.gameObject.GetComponent<PlayerCube>() != null)
            {
                if (!photonView.IsMine == false)
                {
                    Transform cam = Camera.main.transform;
                    if (cam != null)
                    {
                        cam.transform.SetParent(this.transform.parent);
                    }
                }
            }

            float heightLevel = this.transform.localScale.y;

            for (float x = 0.25f; x <= heightLevel; x += 0.25f)
            {
                GameObject brokenCube = (GameObject)GameObject.Instantiate(brokenVersion, new Vector3(this.transform.position.x, this.transform.position.y + 0.25f, this.transform.position.z), this.transform.rotation) as GameObject;
                brokenCube.transform.SetParent(partStorage);
                brokenModelSpawned = brokenCube;
                Rigidbody rigidThing = brokenCube.AddComponent<Rigidbody>();
                rigidThing.AddForce((point - rigidThing.transform.position).normalized * 10f, ForceMode.Force);
                //rigidThing.AddExplosionForce (100f, brokenCube.transform.position, 10f, 3.0f, ForceMode.Force);
                Transform[] allChildren = brokenCube.GetComponentsInChildren<Transform>();
                foreach (Transform child in allChildren)
                {
                    if (child.GetComponent<Rigidbody>() != null)
                    {
                        Rigidbody rigid = child.GetComponent<Rigidbody>();
                        Light lit = child.gameObject.AddComponent<Light>();
                        lit.range = 5.0f;
                        //rigid.AddForceAtPosition(40f * rigidThing.transform.forward, point, ForceMode.Force);
                        var dir = point - child.position;
                        dir = dir.normalized;
                        rigid.AddForce(-dir * 15f, ForceMode.Impulse);
                        rigid.AddExplosionForce(30f, child.position, 10f, 2.0f, ForceMode.Force);
                    }
                }
                RaycastHit[] hits = Physics.SphereCastAll(brokenCube.transform.position, 10f, brokenCube.transform.position, 20f);
                foreach (RaycastHit hit in hits)
                {
                    //Debug.Log (hit.transform.name);
                    Rigidbody rigidb = hit.transform.GetComponent<Rigidbody>();
                    if (rigidb != null)
                    {
                        //rigidb.AddForce (-hit.normal * 500f);
                    }
                }
            }
        }



        if (this.transform != null && this.gameObject != null && this.transform.parent != null)
        {
            if (this.transform.GetComponent<EnemyAI>() != null)
            {
                this.transform.GetComponent<EnemyAI>().Die();
            }
            this.gameObject.SetActive(false);
        }
        this.enabled = false;
        if (this.GetComponent<PlayerCube>() != null)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void DestroyModel()
    {
        if (this.GetComponent<PlayerCube>() != null)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


	public void DisableEditor() {
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.IsWriting) {
			stream.SendNext(health);
			stream.SendNext(isAlive);

		}
		else 
		{
            this.health = (float)stream.ReceiveNext();
			this.isAlive = (bool)stream.ReceiveNext();
		}
	}
}
