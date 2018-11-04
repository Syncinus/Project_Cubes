using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Photon.Pun;
using Photon.Realtime;


//namespace Pathfinding {
public class EnemyAI : MonoBehaviourPunCallbacks
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

    public Color color;
    public WeaponItem weapon;

    public Mode enemyMode = Mode.Assault;
    public bool BeamPossible = false;
    public bool createdParticles = false;
    public float damage = 5.0f;
    public float fireRate = 10.0f;
    public float range = 10.0f;
    public float detectionRadius = 15f;
    public float stealthDetection = 10f;
    private float lookAtDistance = 10f;
    public float attackDistance = 7.5f;

    public int ScoreGiven;
    float nextTimeToFire;

    public bool wandering = true;

    public event System.Action OnDeath;
    public bool dead = false;

    [HideInInspector] public bool targetChangeable;
    [HideInInspector] public bool playerDistanceCheckReady;
    /*
    [HideInInspector] public WeaponType wType;
    */
    [HideInInspector] public LineRenderer lineRenderer;
    [HideInInspector] public Color particlesColor;

    PlayerCube[] players;
    Transform particlesTransform;
    bool beamReady = true;
    bool finishedGrowingLine = false;
    float lastTimeFired;
    float timeOfLastFire;
    bool ableToShoot = true;

    public float fieldOfView = 60f;
    public Vector3 personalLastSighting;

    private Vector3 previousSighting;


    public void Die()
    {
        dead = true;
        OnDeath?.Invoke();
        ScoreSystem.Score += ScoreGiven;
        GameObject.Destroy(this.gameObject);
    }

    private void Awake()
    {

        personalLastSighting = LastPlayerSighting.resetPosition;
        previousSighting = LastPlayerSighting.resetPosition;


        this.gameObject.AddComponent<InvalidDestroyer>();
        this.gameObject.AddComponent<FallDissolve>();
        ai = this.GetComponent<IAstarAI>();
        setter = this.GetComponent<AIDestinationSetter>();
        players = GameObject.FindObjectsOfType<PlayerCube>();
        if (this.GetComponent<LineRenderer>() != null)
        {
            lineRenderer = this.GetComponent<LineRenderer>();
        } else
        {
            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.enabled = false;
        lineRenderer.material = Resources.Load<Material>("Materials/LineMaterial");
        //this.GetComponent<Renderer>().material.color = color;
        OldTarget = target;
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
        if (particlesTransform != null)
        {
            if (particlesTransform.gameObject.activeSelf == true && lastTimeFired > timeOfLastFire + 1.0f)
            {
                particlesTransform.gameObject.SetActive(false);
            }
        }

        if (lineRenderer.enabled == true)
        {
            /*
            SetLinePositions(this.transform.position, this.transform.position + (this.transform.forward * weapon.range));
            */
            /*
            if (weapon.type == WeaponType.Laser)
            {
                RaycastHit hit;
                if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, weapon.range))
                {
                    SetLinePositions(this.transform.position, hit.point);
                }
            }

            if (weapon.weaponHasRecharge == true)
            {
                if (lastTimeFired >= timeOfLastFire + (weapon.weaponRechargeTime / 2) && finishedGrowingLine == true)
                {
                    disableLineRenderer(0.25f);
                }
            }
            */
        }

        lastTimeFired += Time.deltaTime;

        /*
        if (weapon != null)
        {
            damage = weapon.damage;
            fireRate = weapon.firerate;
            range = weapon.range;
            wType = weapon.type;
            particlesColor = weapon.particlesColor;
        }
        */


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
            /*
            if (Time.time >= nextTimeToFire && ableToShoot == true)
            {
                float sqrDstToTarget = (target.position - this.transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDistance + 1f + 1f, 2))
                {
                    int obstacles = LayerMask.GetMask("Obstacles");
                    if (!Physics.Linecast(this.transform.position, target.position, obstacles))
                    {
                        if (weapon.weaponHasRecharge == false)
                        {
                            nextTimeToFire = Time.time + 1f / weapon.firerate;
                            if (wType == WeaponType.Laser || wType == WeaponType.Rail)
                            {
                                nextTimeToFire += 0.05f;
                            }
                            lastTimeFired = Time.time;
                            timeOfLastFire = Time.time;
                        } else
                        {
                            nextTimeToFire = weapon.weaponRechargeTime;

                            if (wType == WeaponType.Laser || wType == WeaponType.Rail)
                            {
                                nextTimeToFire += 0.05f;
                            }

                            ableToShoot = false;
                        }

                        Attack();
                    }

                }

            }
            */
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

        /*
        if (target != null)
        {
            if (Vector3.Distance(target.position, this.transform.position) < lookAtDistance)
            {
                int obstacles = LayerMask.GetMask("Obstacles");
                //int pieces = LayerMask.GetMask("Pieces");
                if (!(Physics.Linecast(this.transform.position, target.position, obstacles)))
                {
                    var targetRotation = Quaternion.LookRotation(target.position - this.transform.position, Vector3.up);
                    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * 2.0f);
                }
            }
        }
        */

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

        if (target != OldTarget)
        {
            OldTarget = target;
            targetChangeable = false;
            Invoke("TargetChangeRefresh", 5f);
        }
    }

    public void ReEnableBeam()
    {
        beamReady = true;
    }

    public void Attack()
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
            //StartCoroutine(shoot(false));
        }


        //curState = State.Searching;
    }

    public void Shoot(bool beamMode)
    {
        StartCoroutine(shoot(beamMode));
    }

    public IEnumerator shoot(bool beamMode)
    {
        RaycastHit hit;



        if (weapon == null)
        {
            if (beamMode == false)
            {
                this.transform.Find("Blast").GetComponent<ParticleSystem>().Play();
            }
            else
            {
                this.transform.Find("Beam").GetComponent<ParticleSystem>().Play();
            }

            if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, range))
            {
                DestroyableObject desObj = hit.transform.GetComponent<DestroyableObject>();
                Rigidbody hitRigid = hit.transform.GetComponent<Rigidbody>();

                if (hit.transform.GetComponent<EnemyAI>() == null)
                {

                    if (desObj != null)
                    {
                        if (beamMode == false)
                        {
                            desObj.TakeDamage(damage, hit.point, this.gameObject);
                        }
                        else
                        {
                            desObj.TakeDamage(1000f, hit.point, this.gameObject);
                        }
                        if (hitRigid != null)
                        {
                            hitRigid.AddForce(-hit.point * 10);
                        }
                    }
                }
            }
        } else
        
          
        yield return null;

    }

    public void PlayParticles(bool usingbeam, Color color)
    {
        if (usingbeam == true)
        {
            this.transform.Find("Beam").GetComponent<ParticleSystem>().Play();
        } else
        {
            Kvant.Stream stream = particlesTransform.GetComponent<Kvant.Stream>();
            stream.gameObject.SetActive(true);
            stream.color = color;
        }
    }

    public void OnBulletHit(GameObject Hit, Collision collision)
    {
        if (Hit.GetComponent<EnemyAI>() == null)
        {

            DestroyableObject target = Hit.transform.GetComponent<DestroyableObject>();
            if (target != null)
            {
                ContactPoint point = collision.contacts[0];
                Vector3 hitpoint = point.point;
                target.TakeDamage(damage, hitpoint, this.gameObject);
            }

            Rigidbody hitRigid = Hit.transform.GetComponent<Rigidbody>();
            Rigidbody rb = this.transform.GetComponent<Rigidbody>();
            if (hitRigid != null)
            {
                if (rb != null)
                {
                    if (hitRigid == rb)
                    {
                        return;
                    }
                }

                hitRigid.AddForce(-collision.contacts[0].point * 100);

            }
        }
    }

    public void reEnableShots(float rechargeTime)
    {
        StartCoroutine(ReEnableShots(rechargeTime));
    }

    IEnumerator ReEnableShots(float rechargeTime)
    {
        yield return new WaitForSeconds(rechargeTime);
        ableToShoot = true;
    }

    public void SetUpLine(Color lineColor)
    {
        lineRenderer.startColor = new Color(lineColor.r, lineColor.g, lineColor.b);
        lineRenderer.endColor = new Color(lineColor.r, lineColor.g, lineColor.b);

        lineRenderer.positionCount = 2;
    }

    public void SetLinePositions(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    public void growLineRenderer(float maxWidth, float time)
    {
        StartCoroutine(GrowLineRenderer(maxWidth, time));
    }

    public void disableLineRenderer(float time)
    {
        StartCoroutine(DisableLineRenderer(time));
    }


    IEnumerator DisableLineRenderer(float time)
    {
        LineRenderer line = this.GetComponent<LineRenderer>();
        Vector2 startSize = new Vector2(line.startWidth, line.endWidth);
        Vector2 endSize = Vector2.zero;
        Vector2 currentSize;

        float timer = 0.0f;

        do
        {
            currentSize = Vector2.Lerp(startSize, endSize, timer / time);
            line.SetWidth(currentSize.x, currentSize.y);
            timer += Time.deltaTime;
            yield return null;
        } while (timer < time);

        finishedGrowingLine = false;
        line.enabled = false;
    }



    IEnumerator GrowLineRenderer(float maxWidth, float time)
    {
        LineRenderer line = lineRenderer;
        line.enabled = true;
        Vector2 startSize = Vector2.zero;
        Vector2 maxSize = new Vector2(maxWidth, maxWidth);
        Vector2 currentSize;
        line.startWidth = 0f;
        line.endWidth = 0f;
        float timer = 0.0f;

        do
        {
            currentSize = Vector2.Lerp(startSize, maxSize, timer / time);
            line.startWidth = currentSize.x;
            line.endWidth = currentSize.y;
            timer += Time.deltaTime;
            yield return null;
        } while (timer < time);

        finishedGrowingLine = true;
    }

    /*
    [PunRPC]
    public void PlayParticles(bool usingbeam, Vector3 fakeColor)
    {
        if (usingbeam == true)
        {
            beam.Play();
        }
        else
        {
            Color color = new Color(fakeColor.x, fakeColor.y, fakeColor.z, 1);
            ParticleSystem.MainModule main;
            ParticleSystem system = this.GetComponentInChildren<ParticleSystem>();
            main = system.main;
            main.startColor = color;
            system.Play();
        }
    }

    [PunRPC]
    public void growLineRenderer(float maxWidth, float time)
    {
        lineRenderer.enabled = true;
        StartCoroutine(GrowLineRenderer(maxWidth, time));
    }

    [PunRPC]
    public void disableLineRenderer(float time)
    {
        StartCoroutine(DisableLineRenderer(time));
    }
    */

    private float nextRunawayTime;
    private float nextStealthCheckTime;
    private float nextSearchTime;

    public Transform OldTarget { get; set; }

    private bool hasDetectedPlayer = false;

    private void RunDetection(Collider other)
    {
        if (other.gameObject == target.gameObject)
        {
            hasDetectedPlayer = false;

            Vector3 direction = other.transform.position - this.transform.position;
            float angle = (Vector3.Angle(direction, this.transform.forward));

            if (angle >= -(fieldOfView / 2) && angle <= (fieldOfView / 2))
            {
                RaycastHit hit;

                Debug.DrawRay(this.transform.position, direction, Color.red);
                if (Physics.Raycast(this.transform.position, direction, out hit, 20f, LayerMask.NameToLayer("Player")))
                {
                    if (hit.collider.transform == target)
                    {
                        hasDetectedPlayer = true;
                        LastPlayerSighting.position = target.transform.position;
                    }
                }

            }
        }
    }

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
                        if (hasDetectedPlayer == false)
                        {
                            if (LastPlayerSighting.position != previousSighting)
                            {
                                personalLastSighting = LastPlayerSighting.position;
                            }

                            Collider[] colliders = Physics.OverlapSphere(this.transform.position, detectionRadius);

                            foreach (Collider collider in colliders)
                            {
                                RunDetection(collider);
                            }

                            previousSighting = LastPlayerSighting.position;                      

                            if (Time.time > nextSearchTime && (ai.reachedEndOfPath || !ai.hasPath))
                            {
                                trueTarget = null;
                                RandomPath path = RandomPath.Construct(this.transform.position, 20);

                                path.spread = 5;
                                this.GetComponent<Seeker>().StartPath(path);
                            }
                        }
                        else
                        {
                            trueTarget = target;
                        }
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
