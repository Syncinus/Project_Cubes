using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


//namespace Pathfinding {
public class EnemyAI : MonoBehaviour
{

    #region Depracted
    [HideInInspector] [System.Obsolete] public EnemyType typeOfEnemy;
    [HideInInspector] [System.Obsolete] public enum EnemyType { Blue, Green, Yellow, Orange, Black };
    #endregion

    public IAstarAI ai;
    AIDestinationSetter setter;
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform trueTarget;
    State curState;
    public enum State { Searching, Idle, Attacking };
    public enum Mode  { Assault, Avoid }

    public WeaponItem weapon;

    public Mode enemyMode = Mode.Assault;
    public bool BeamPossible = false;
    public float damage = 5.0f;
    public float fireRate = 10.0f;
    public float range = 10.0f;
    public float attackDistance = 7.5f;
    public int ScoreGiven;
    float nextTimeToFire;

    public event System.Action OnDeath;
    public bool dead = false;

    [HideInInspector] public bool targetChangeable;
    [HideInInspector] public bool playerDistanceCheckReady;
    


    PlayerCube[] players;
    Transform oldTarget;
    bool beamReady = true;


    public void Die()
    {
        dead = true;
        OnDeath?.Invoke();
        ScoreSystem.Score += ScoreGiven;
        GameObject.Destroy(this.gameObject);
    }

    private void Awake()
    {
        ai = this.GetComponent<IAstarAI>();
        setter = this.GetComponent<AIDestinationSetter>();
        players = GameObject.FindObjectsOfType<PlayerCube>();
        oldTarget = target;
        StartCoroutine(UpdatePath());
    }

    public void DistanceRefresh()
    {
        playerDistanceCheckReady = true;
    }

    public void ReEnablePathfinder()
    {
        ai.isStopped = false;
    }

    public void TargetChangeRefresh()
    {
        targetChangeable = true;
    }

    public void FixedUpdate()
    {

        if (weapon != null)
        {
            damage = weapon.damage;
            fireRate = weapon.firerate;
            range = weapon.range;
        }


        //if (target != null)
        //{
        //  this.transform.LookAt(target);
        //}
        if (setter.target == null)
        {
            if (GameObject.Find("PlayerCube(Clone)"))
            {
                target = GameObject.Find("PlayerCube(Clone)").transform;
                //this.GetComponent<AdvancedPathCalculation>().enabled = true;
            }
        }



        if (target != null && enemyMode != Mode.Avoid)
        {
            if (Time.time >= nextTimeToFire)
            {
                float sqrDstToTarget = (target.position - this.transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDistance + 1f + 1f, 2))
                {
                    int obstacles = LayerMask.GetMask("Obstacles");
                    if (!Physics.Linecast(this.transform.position, target.position, obstacles))
                    {
                        nextTimeToFire = Time.time + 1f / fireRate;
                        StartCoroutine(Attack());
                    }

                }

            }
        }


        if (target != null) {
       
            float SquareDstToTarget = (target.position - this.transform.position).sqrMagnitude;

            if (SquareDstToTarget < Mathf.Pow(3.0f + 0.85f + 0.85f, 2))
            {
                int obstacles = LayerMask.GetMask("Obstacles");
                //int pieces = LayerMask.GetMask("Pieces");
                if (!(Physics.Linecast(this.transform.position, target.position, obstacles)))
                {

                    var targetRotation = Quaternion.LookRotation(target.position - this.transform.position, Vector3.up);
                    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * 2.0f);
                    if (enemyMode != Mode.Avoid)
                    {
                        curState = State.Attacking;
                    }
                }
            }
            else
            {
                curState = State.Searching;
            }
        }

        if (playerDistanceCheckReady == true)
        {
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

        if (target != oldTarget)
        {
            oldTarget = target;
            targetChangeable = false;
            Invoke("TargetChangeRefresh", 5f);
        }
    }

    public void ReEnableBeam()
    {
        beamReady = true;
    }

    public IEnumerator Attack()
    {
        //curState = State.Attacking;

        if (BeamPossible == true && beamReady == true)
        {
            Shoot(true);
            beamReady = false;
            Invoke("ReEnableBeam", 10.0f);
        } else
        {
            Shoot(false);
        }

        yield return null;

        //curState = State.Searching;
    }

    public void Shoot(bool beam)
    {
        RaycastHit hit;


        if (beam == false)
        {
            this.transform.Find("Blast").GetComponent<ParticleSystem>().Play();
        } else
        {
            this.transform.Find("Beam").GetComponent<ParticleSystem>().Play();
        }

        if (weapon == null)
        {
            if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, range))
            {
                DestroyableObject desObj = hit.transform.GetComponent<DestroyableObject>();
                Rigidbody hitRigid = hit.transform.GetComponent<Rigidbody>();

                if (hit.transform.GetComponent<EnemyAI>() == null)
                {

                    if (desObj != null)
                    {
                        if (beam == false)
                        {
                            desObj.TakeDamage(damage, hit.point);
                        }
                        else
                        {
                            desObj.TakeDamage(1000f, hit.point);
                        }
                        if (hitRigid != null)
                        {
                            hitRigid.AddForce(-hit.point * 10);
                        }
                    }
                }
            }
        } else
        {
        }
    }

    private float nextRunawayTime;

    IEnumerator UpdatePath()
    {
        while (true)
        {
 
            if (curState == State.Searching)
            {
                ai.isStopped = false;
                //this.GetComponent<RichAI>().enabled = false;
                if (target != null)
                {
                    if (enemyMode == Mode.Assault)
                    {
                        trueTarget = target;
                    }
                    else if (enemyMode == Mode.Avoid)
                    {
                        {

                            if (Time.time > nextRunawayTime && Vector3.Distance(this.transform.position, target.transform.position) < 20f)
                            {
                                //Debug.Log("STAY AWAY FROM ME!");

                                Vector3 targetPosition = target.position;

                                int stopGScore = 1000;

                                FleePath path = FleePath.Construct(this.transform.position, targetPosition, stopGScore);
                                path.spread = 4000;
                                path.aimStrength = 1;

                                
                                Seeker seeker = this.GetComponent<Seeker>();

                                seeker.StartPath(path, GetEscapePath);

                                nextRunawayTime = Time.time * 1.01f;
                            }
                        }
                    }
                }
            }

            if (curState == State.Attacking)
            {
                ai.isStopped = true;
                //var targetRotation = Quaternion.LookRotation(target.position - this.transform.position, Vector3.up);
                //this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * 2.0f);
                this.GetComponent<EnemyAI>().enabled = true;
            }
            yield return new WaitForEndOfFrame(); 
        }
    }

    public void PhotonInstantiate(string objectName, Vector3 position, Quaternion rotation)
    {
        PhotonNetwork.Instantiate(objectName, position, rotation, 0);
    }



    void GetEscapePath(Path p)
    {
        if (p.error)
        {
            Debug.Log("Aw Crap!!! The Path Failed! This Means That The Pathfinder Deserves A Beatdown!!!!!!!!!!!");
            return;
        }
        FleePath fPath = p as FleePath;

        
        List<GraphNode> nodeList = fPath.path;
        List<Vector3> pointList = PathUtilities.GetPointsOnNodes(nodeList, 25, 10);
        Vector3 farthestPoint = target.transform.position;
        for (int i = 0; i < pointList.Count; i++)
        {
            if (Vector3.Distance(pointList[i], target.transform.position) > Vector3.Distance(farthestPoint, target.transform.position))
                farthestPoint = pointList[i];
        }
        


        Debug.DrawLine(this.transform.position, farthestPoint);

        ai.destination = farthestPoint;
    }

}
//}
