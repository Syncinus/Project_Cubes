using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class OldEnemyAI : MonoBehaviour {

    public GameObject proxy;
	public GameObject model;
	public EnemyMode mode = EnemyMode.Assault;
	public enum State {Searching, Idle, Attacking};
	public enum EnemyMode {Assault, Avoid};
	public enum EnemyType {Blue, Green, Yellow, Orange, Black};
	public ParticleSystem blast;
	public EnemyType typeOfEnemy;
	public int ScoreGiven = 1;
	public float damage = 5f;
	public bool freezePointsEnabled = false;
	public List<float> freezePoints = new List<float>();
    public AudioClip clip;
    private AudioSource source;
	State curState;

	NavMeshAgent nav;
	public Transform target;
	private Transform oldTarget;
	[HideInInspector] public bool targetChangeable;
    
	public float collisionRadius = 1f;
	public float targetCollisionRadius = 1f;
	public float attackDistance = 10f;

	bool hasTarget;

	float nextTimeToFire;
	public float fireRate = 10f;

	public event System.Action OnDeath;
	public bool dead = false;

	private bool beamReady = true;
	private float beamRechargeTime = 10f;
	private Vector3 velocity = Vector3.zero;
	private Transform startTransform;
	private float nextRunAwayTime;

	public float healthBarLength;
	public float maxHealth;

    private float timeScinceLastTimeTakingDamage = 0f;
    private float healthValue;

	PlayerCube[] players;
	bool playerDistanceCheckReady = true;

	public void Die() {
		dead = true;
		if (OnDeath != null) {
			OnDeath ();
		}
		ScoreSystem.Score += ScoreGiven;
		GameObject.Destroy (this.gameObject);
	}

	public void Start() {
        source = this.gameObject.AddComponent<AudioSource>();
		if (GameObject.FindObjectOfType<PlayerCube>() != null) {
			curState = State.Searching;
			hasTarget = true;
			nav = proxy.GetComponent<NavMeshAgent> ();
			target = GameObject.FindObjectOfType<PlayerCube> ().transform;
			oldTarget = target;
			players = GameObject.FindObjectsOfType<PlayerCube>();
            //healthValue = model.GetComponent<DestroyableObject>().health;

            //if (typeOfEnemy == EnemyType.Black) {
            //    collisionRadius = 30f;
			//} else {
			//   collisionRadius = 1f;
			//}
			//targetCollisionRadius = 1f;
			//attackDistance = 10f;
			//maxHealth = model.GetComponent<DestroyableObject>().health;
			healthBarLength = Screen.width / 6;
			StartCoroutine (UpdatePath());
            StartCoroutine (Regenerate());
		}
	}

	public void DistanceRefresh() {
		playerDistanceCheckReady = true;
	}
	
	public void TargetChangeRefresh() {
		targetChangeable = true;
	}

	public void ReEnableNavMeshAgent() {
		nav.enabled = true;
	}

    IEnumerator Regenerate()
    {
        DestroyableObject desObj = model.GetComponent<DestroyableObject>();
        while (true)
        {
            //if (desObj.health < desObj.maxHealth && timeScinceLastTimeTakingDamage >= 5f)
            //{
            //    desObj.health += 750;
            //    yield return new WaitForSeconds(1f);
            //}
            //else
            //{
            //    yield return null;
            //}
        }
    }

    public void FixedUpdate() {
        timeScinceLastTimeTakingDamage += Time.deltaTime;
        //if(model.GetComponent<DestroyableObject>().health != healthValue)
        //{
        //    healthValue = model.GetComponent<DestroyableObject>().health;
        //    timeScinceLastTimeTakingDamage = 0.0f;
        //}
		if (freezePointsEnabled == true) {
            foreach (float freezePoint in freezePoints) {
				//if (model.GetComponent<DestroyableObject>().health >= freezePoint - 10 && model.GetComponent<DestroyableObject>().health <= freezePoint + 10) {
                //    nav.enabled = false;
				//	nav.speed -= 0.3f;
				//	nextTimeToFire = Time.time + 5f;
				//	Invoke("ReEnableNavMeshAgent", 5f);
					//freezePoints.Remove(freezePoint);
				//}
			}
		}

         
	    if (target != oldTarget) {
			oldTarget = target;
			targetChangeable = false;
			Invoke("TargetChangeRefresh", 5f);
		}
		
		

        
		if (playerDistanceCheckReady == true) {
            if (target != null)
            {
                float targetDistance = Vector3.Distance(this.transform.position, target.transform.position);
                foreach (PlayerCube cube in players)
                {
                    if (cube != null)
                    {
                        Transform playerTransform = cube.transform;
                        float curDistance = Vector3.Distance(this.transform.position, playerTransform.position);
                        if (curDistance < targetDistance)
                        {
                            target = cube.transform;
                            targetDistance = curDistance;
                        }
                    }
                }
            }
        playerDistanceCheckReady = false;
		Invoke("DistanceRefresh", 5f);
		}
		

        if (model != null) {
				model.transform.position = Vector3.SmoothDamp(model.transform.position, proxy.transform.position, ref velocity, 0.3f);
				model.transform.rotation = Quaternion.RotateTowards(model.transform.rotation, proxy.transform.rotation, 100f * Time.deltaTime);
		}
		if (hasTarget == true) {
			if (Time.time >= nextTimeToFire) {
				if (target != null) {
				float sqrDstToTarget = (target.position - proxy.transform.position).sqrMagnitude;
				if (sqrDstToTarget < Mathf.Pow(attackDistance + collisionRadius + targetCollisionRadius, 2)) {
				    int obstacles = LayerMask.GetMask("Obstacles");
					if (!Physics.Linecast(proxy.transform.position, target.position, obstacles)) {
						if (mode != EnemyMode.Avoid) {
						    nextTimeToFire = Time.time + 1f / fireRate;
						    StartCoroutine (Attack ());
						}
					}
				}
				}
			}
		}
	}

	IEnumerator Attack() {
		curState = State.Attacking;
		//Vector3 originalPosition = transform.position; 
		//Vector3 dirToTarget = (target.position - this.transform.position).normalized;
		//Vector3 attackPosition = target.position - dirToTarget * (collisionRadius);
		proxy.transform.LookAt (target.position);
		if (typeOfEnemy != EnemyType.Orange) {
			Shoot (false);
		}
		if (typeOfEnemy == EnemyType.Orange) {
			if (beamReady == true) {
				Shoot (true);
				beamReady = false;
				Invoke ("ReEnableBeam", beamRechargeTime);
			} else {
				Shoot (false);
			}
		}

		yield return null;
		curState = State.Searching;
	}

	public void ReEnableBeam() {
		beamReady = true;
	}

	//public void Update() {
	  //model.transform.position = Vector3.Lerp(model.transform.position, proxy.transform.position, Time.deltaTime * 2);
	  //model.transform.rotation = Quaternion.Lerp(model.transform.rotation, proxy.transform.rotation, Time.deltaTime * 5);
	//}

	public void Shoot(bool beam) {
		RaycastHit hit;

		if (beam == false) {
 		   if (blast != null) {
			blast.Play ();
		   }
		}

		if (beam == true) {
			if (blast != null) {
			    model.transform.GetChild (1).GetComponent<ParticleSystem> ().Play ();
			}
		}


		float properRange = 20f;
		if (beam == true) {
			properRange = 30f;
		}
        
		if (Physics.Raycast (model.transform.position, model.transform.forward, out hit, properRange)) {
			DestroyableObject desObj = hit.transform.GetComponent<DestroyableObject> ();
			Rigidbody hitRigid = hit.transform.GetComponent<Rigidbody> ();

            if (hit.transform.GetComponent<EnemyAI>() == null) {
				if (beam == false) {
					if (desObj != null) {
						//desObj.TakeDamage (damage, hit.point, this.gameObject);
					}
				}
				if (beam == true) {
					if (desObj != null) {
					//desObj.TakeDamage (1000f, hit.point, this.gameObject);
					}
					if (hitRigid != null) {
						hitRigid.AddForce(hit.point * 100);
					}
				}
				
			}
		} 	
	}

	IEnumerator UpdatePath() {
		float refreshRate = 0.01f;
	
		while (hasTarget) {
			if (curState == State.Searching) {
				if (mode == EnemyMode.Assault) {
				if (target != null) {
				 Vector3 dirToTarget = (target.position - proxy.transform.position).normalized;
				 Vector3 targetPosition = target.position - dirToTarget * (collisionRadius + targetCollisionRadius / 2);
				 //Vector3 targetPosition = target.position;
			        if (nav != null && nav.enabled == true) 
			            nav.SetDestination (targetPosition);
			        }
				} else if (mode == EnemyMode.Avoid) {
					if (Time.time > nextRunAwayTime || Vector3.Distance(proxy.transform.position, target.transform.position) < 10f) {
					startTransform = proxy.transform;

					proxy.transform.rotation = Quaternion.LookRotation(proxy.transform.position - target.transform.position);

					Vector3 runTo = proxy.transform.position + proxy.transform.forward * 3;

					NavMeshHit hit;

					NavMesh.SamplePosition(runTo, out hit, 5, 1 << NavMesh.GetNavMeshLayerFromName("Walkable"));

					proxy.transform.position = startTransform.position;
					proxy.transform.rotation = startTransform.rotation;

					nextRunAwayTime = Time.time * 3;

					    if (nav != null && nav.enabled == true) {
					        nav.SetDestination(hit.position);
					    }
					}
				}
			}
			yield return new WaitForSeconds (refreshRate);
		}
	}
}
