using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using StateSystems;

public class TerrorizerAI : BossAI {

    public int currentPhase = 1;

    public override void Start()
    {

        orderlyStates = false;
        states.Add(new TargetTeleport());
        states.Add(new MissileLaunch());
        states.Add(new CuttingEdge());

        currentDesObjHealth = this.GetComponent<DestroyableObject>().health;
        currentTeleportsRemaining = maximumTeleports;
            
        base.Start();
    }

    private float nextTeleportTime = 0f;
    private int maximumTeleports = 10;
    private int currentTeleportsRemaining = 10;
    private State<BossAI> lastState;

    private float nextLaunchTime = 0f;

    private float nextCuttingEdgeTime = 0f;

    private float nextMissileBlasting;
    private int remainingMissilesToLaunch = 6;

    private float currentDesObjHealth;

    private float PhaseTwoInitilization = 25000f;
    private bool StartedPhaseTwo = false;

    public void ReAddTeleports()
    {
        currentTeleportsRemaining = maximumTeleports;
    }


    private CanvasGroup CV;

    public void StartPhaseTwo()
    {
        currentPhase = 2;
        orderlyStates = true;
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z);
        this.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        this.transform.Find("Blast").transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        this.transform.Find("Blast").transform.localPosition = new Vector3(0f, 0f, 0.75f);
        states.Clear();

        states.Add(new EnemyBombardment());
        states.Add(new Missiles());
        states.Add(new TripletMultiply());
        states.Add(new BoxBlast(0));
        states.Add(new BoxBlast(1));
        states.Add(new BoxBlast(2));
        states.Add(new BoxBlast(3));
        states.Add(new BoxBlast(4));
        states.Add(new TripletMultiply());
        states.Add(new EnemyBombardment());
        states.Add(new BoxBombarder());
        states.Add(new WarMode());


        ExecuteNextPhaseTwoState();
    }


    public new void FixedUpdate()
    {
        if (this.GetComponent<DestroyableObject>().health <= PhaseTwoInitilization && StartedPhaseTwo == false && target != null)
        {
            StartPhaseTwo();
            StartedPhaseTwo = true;
        }

        

        if (currentPhase == 1)
        {
            ExecutePhaseOneState();
        }

        if (currentPhase == 2)
        {
            ExecutePhaseTwoState();
            if (target.GetComponent<DestroyableObject>().health <= 0)
            {
                states.Insert(0, lastState);
                StartCoroutine(ScreenFlashStateSet());
            }
        }

        if (currentPhase == 3)
        {
            if (target != null)
            {
                UpdateWarMode();
            }
        }

        base.FixedUpdate();
    }

    private float waitingTimer;
    private float timeOfLastMove;
    private bool readyToPreformMove = true;
    private System.Random rndValue = new System.Random();

    private float StoppingTime;


    public void UpdateWarMode()
    {
        waitingTimer += Time.deltaTime;
        if (waitingTimer >= timeOfLastMove + 5f)
        {
            readyToPreformMove = true;
        }


        if (Time.time > nextTeleportTime && currentTeleportsRemaining > 0)
        {
            if (currentTeleportsRemaining == maximumTeleports)
            {
                currentDesObjHealth = this.GetComponent<DestroyableObject>().health;
            }
            this.transform.Find("TeleportParticles").GetComponent<ParticleSystem>().Play();
            Vector2 newPosition = Random.insideUnitCircle * 4 + new Vector2(target.transform.position.x, target.transform.position.z);

            this.transform.position = new Vector3(newPosition.x, this.transform.position.y, newPosition.y);
            //StartCoroutine(BlastToPosition(new Vector3(newPosition.x, this.transform.position.y, newPosition.y)));
            this.transform.LookAt(target);

            nextTeleportTime = Time.time * 1.01f;
            currentTeleportsRemaining -= 1;


            DestroyableObject desObj = this.GetComponent<DestroyableObject>();

            if (desObj.health < currentDesObjHealth)
            {
                currentDesObjHealth = desObj.health;
                nextTeleportTime = Time.time * 1.01f;
            }
        }


        if (BehindLeft != null && BehindRight != null)
        {
            target.GetComponent<PlayerCube>().ableToMove = false;
            BehindLeft.transform.position = target.transform.position + (Vector3.back + Vector3.left) * 2.4f;
            BehindRight.transform.position = target.transform.position + (Vector3.back + Vector3.right) * 2.4f;
            this.transform.position = target.transform.position + (Vector3.forward) * 3.0f;
            BehindLeft.GetComponent<Rigidbody>().isKinematic = true;
            BehindRight.GetComponent<Rigidbody>().isKinematic = true;
            BehindLeft.GetComponent<EnemyAI>().ai.isStopped = true;
            BehindRight.GetComponent<EnemyAI>().ai.isStopped = true;
            ai.isStopped = true;
            BehindLeft.transform.LookAt(target);
            BehindRight.transform.LookAt(target);
            this.transform.LookAt(target);
        }
        else
        {
            target.GetComponent<PlayerCube>().ableToMove = true;
        }

        if (readyToPreformMove == true)
        {
            waitingTimer = Time.time;
            timeOfLastMove = Time.time;
            int rnd = rndValue.Next(0, 4);


            if (rnd == 0)
            {
                Debug.Log("Missiles!");

                for (int i = 0; i < 3; i++)
                {
                    Vector3 rightPosition = this.transform.position + this.transform.right * 1.5f;

                    GameObject projectile = PhotonNetwork.Instantiate("Bullet", rightPosition, this.transform.rotation, 0);
                    //GameObject projectile = Instantiate(Resources.Load<GameObject>("Bullet"), rightPosition, this.transform.rotation);
                    projectile.GetComponent<Bullet>().shooter = this.transform;
                    Destroy(projectile.GetComponent<Bullet>());
                    projectile.AddComponent<Missile>();
                    projectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    //projectile.GetComponent<Missile>().enabled = false;
                    projectile.GetComponent<Missile>().shooter = this.transform;
                    projectile.GetComponent<Missile>().target = target;
                    projectile.GetComponent<Missile>().ogName = "Bullet";
                    projectile.GetComponent<Missile>().splitMissile = true;
                    projectile.GetComponent<Missile>().amountToSplit = 6;
                    projectile.GetComponent<Missile>().isExplosive = true;
                    projectile.GetComponent<Missile>().driveSpeed = 12.5f;
                    //projectile.GetComponent<Missile>().enabled = true;

                    //projectile.GetComponent<Missile>().isHoming = true;
                    projectile.transform.SetParent(GameObject.Find("TempStorage").transform);

                }

                for (int i = 0; i < 3; i++)
                {
                    Vector3 leftPosition = this.transform.position + (-this.transform.right) * 1.5f;

                    GameObject projectile = PhotonNetwork.Instantiate("Bullet", leftPosition, this.transform.rotation, 0);
                    //GameObject projectile = Instantiate(Resources.Load<GameObject>("Bullet"), rightPosition, this.transform.rotation);
                    projectile.GetComponent<Bullet>().shooter = this.transform;
                    Destroy(projectile.GetComponent<Bullet>());
                    projectile.AddComponent<Missile>();
                    projectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    //projectile.GetComponent<Missile>().enabled = false;
                    projectile.GetComponent<Missile>().shooter = this.transform;
                    projectile.GetComponent<Missile>().target = target;
                    projectile.GetComponent<Missile>().ogName = "Bullet";
                    projectile.GetComponent<Missile>().splitMissile = true;
                    projectile.GetComponent<Missile>().amountToSplit = 6;
                    projectile.GetComponent<Missile>().driveSpeed = 15f;
                    //projectile.GetComponent<Missile>().enabled = true;

                    //projectile.GetComponent<Missile>().isHoming = true;
                    projectile.transform.SetParent(GameObject.Find("TempStorage").transform);
                }
            }

            if (rnd == 1)
            {
                Debug.Log("Target Teleport!");

                currentTeleportsRemaining = 10;
            }

            if (rnd == 2)
            {
                Debug.Log("Triplet Assault!");

                target.transform.position = new Vector3(0f, 0.5f, 0f);
                for (int i = 0; i < 2; i++)
                {
                    GameObject newEnemy = PhotonNetwork.Instantiate("Enemies/" + this.transform.name, this.transform.position, this.transform.rotation, 0);
                    newEnemy.transform.SetParent(GameObject.Find("EnemyStorage").transform);
                    if (i == 0)
                    {
                        BehindLeft = newEnemy;
                    }
                    else
                    {
                        BehindRight = newEnemy;
                    }

                    Destroy(newEnemy.GetComponent<TerrorizerAI>());
                    EnemyAI eAI = newEnemy.AddComponent<EnemyAI>();
                    newEnemy.GetComponent<DestroyableObject>().maxHealth = 1000f;
                    newEnemy.GetComponent<DestroyableObject>().health = 1000f;
                    eAI.damage = 0;
                    eAI.fireRate = fireRate;
                    eAI.range = range;
                    eAI.target = target;
                    eAI.ScoreGiven = 5;

                    Invoke("DestroyTwinCubes", 3.0f);
                }
            }

            if (rnd == 3)
            {
                Debug.Log("Cutting Edge!");

                this.transform.LookAt(target);
                Quaternion currentRot = this.transform.rotation;
                RaycastHit hit;

                if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, 20.0f, LayerMask.NameToLayer("Player")))
                {
                    DestroyableObject obj = hit.transform.GetComponent<DestroyableObject>();


                    if (obj != null)
                    {
                        Rigidbody hitRigid = hit.transform.GetComponent<Rigidbody>();

                        if (hit.transform.GetComponent<EnemyAI>() == null)
                        {
                            obj.TakeDamage(1000f, hit.point);

                            if (hitRigid != null)
                            {
                                hitRigid.AddForce(-hit.point * 500);
                            }

                        }
                    }
                }

                if (hit.point != null)
                {
                    Vector3 pos = (-hit.point + Vector3.forward * 2.3f);
                    Vector3 point = new Vector3(pos.x, this.transform.position.y, pos.z);
                    this.transform.position = point;
                }

            }

            readyToPreformMove = false;
        }
    }
    

    public void DestroyTwinCubes()
    {
        if (BehindLeft != null)
        {
            PhotonNetwork.Destroy(BehindLeft);
            BehindLeft = null;
        }
        if (BehindRight != null)
        {
            PhotonNetwork.Destroy(BehindRight);
            BehindRight = null;
        }
    }




    public void ExecutePhaseOneState()
    {
        if (functionial == true)
        {
            if (currentState.GetType() == typeof(TargetTeleport) && target != null && currentState != null)
            {
                if (Time.time > nextTeleportTime)
                {
                    if (currentTeleportsRemaining == maximumTeleports)
                    {
                        currentDesObjHealth = this.GetComponent<DestroyableObject>().health;
                    }
                    this.transform.Find("TeleportParticles").GetComponent<ParticleSystem>().Play();
                    Vector2 newPosition = Random.insideUnitCircle * 4 + new Vector2(target.transform.position.x, target.transform.position.z);

                    this.transform.position = new Vector3(newPosition.x, this.transform.position.y, newPosition.y);
                    //StartCoroutine(BlastToPosition(new Vector3(newPosition.x, this.transform.position.y, newPosition.y)));
                    this.transform.LookAt(target);

                    nextTeleportTime = Time.time * 1.1f;
                    currentTeleportsRemaining -= 1;


                    DestroyableObject desObj = this.GetComponent<DestroyableObject>();

                    if (desObj.health < currentDesObjHealth)
                    {
                        currentDesObjHealth = desObj.health;
                        nextTeleportTime = Time.time * 1.01f;
                    }

                    if (currentTeleportsRemaining <= 0)
                    {
                        Invoke("ReAddTeleports", 0.2f);
                        //StartCoroutine(ReAddTeleports());
                        SwapState(states.ElementAt(1));
                    }
                }
            }

            if (currentState.GetType() == typeof(MissileLaunch) && target != null && currentState != null)
            {
                if (Time.time > nextLaunchTime)
                {
                    ai.isStopped = true;

                    if (Time.time >= nextMissileBlasting && remainingMissilesToLaunch >= 0)
                    {

                        for (int i = 0; i < 1; i++)
                        {
                            Vector3 rightPosition = this.transform.position + this.transform.right * 1.5f;

                            GameObject projectile = PhotonNetwork.Instantiate("Bullet", rightPosition, this.transform.rotation, 0);
                            //GameObject projectile = Instantiate(Resources.Load<GameObject>("Bullet"), rightPosition, this.transform.rotation);
                            projectile.GetComponent<Bullet>().shooter = this.transform;
                            Destroy(projectile.GetComponent<Bullet>());
                            projectile.AddComponent<Missile>();
                            projectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                            //projectile.GetComponent<Missile>().enabled = false;
                            projectile.GetComponent<Missile>().shooter = this.transform;
                            projectile.GetComponent<Missile>().target = target;
                            projectile.GetComponent<Missile>().ogName = "Bullet";
                            projectile.GetComponent<Missile>().splitMissile = true;
                            projectile.GetComponent<Missile>().amountToSplit = 3;
                            projectile.GetComponent<Missile>().isExplosive = true;
                            //projectile.GetComponent<Missile>().enabled = true;

                            //projectile.GetComponent<Missile>().isHoming = true;
                            projectile.transform.SetParent(GameObject.Find("TempStorage").transform);
                            remainingMissilesToLaunch -= 1;
                        }


                        for (int i = 0; i < 1; i++)
                        {
                            Vector3 leftPosition = this.transform.position + (-this.transform.right) * 1.5f;

                            GameObject projectile = PhotonNetwork.Instantiate("Bullet", leftPosition, this.transform.rotation, 0);
                            //GameObject projectile = Instantiate(Resources.Load<GameObject>("Bullet"), rightPosition, this.transform.rotation);
                            projectile.GetComponent<Bullet>().shooter = this.transform;
                            Destroy(projectile.GetComponent<Bullet>());
                            projectile.AddComponent<Missile>();
                            projectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                            //projectile.GetComponent<Missile>().enabled = false;
                            projectile.GetComponent<Missile>().shooter = this.transform;
                            projectile.GetComponent<Missile>().target = target;
                            projectile.GetComponent<Missile>().ogName = "Bullet";
                            projectile.GetComponent<Missile>().splitMissile = true;
                            projectile.GetComponent<Missile>().amountToSplit = 3;
                            //projectile.GetComponent<Missile>().enabled = true;

                            //projectile.GetComponent<Missile>().isHoming = true;
                            projectile.transform.SetParent(GameObject.Find("TempStorage").transform);
                            remainingMissilesToLaunch -= 1;
                        }

                        nextMissileBlasting = Time.time * 1.13f;

                        if (remainingMissilesToLaunch <= 0)
                        {
                            Invoke("ReEnableMissiles", 0.4f);
                            SwapState(states.ElementAt(2));
                            nextLaunchTime = Time.time * 1.1f;
                        }
                    }


                    ai.isStopped = false;
                }
            }


            if (currentState.GetType() == typeof(CuttingEdge) && target != null && currentState != null)
            {
                if (Time.time > nextCuttingEdgeTime)
                {
                    //SweepCast();
                    this.transform.LookAt(target);
                    Quaternion currentRot = this.transform.rotation;
                    RaycastHit hit;

                    if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, 20.0f, LayerMask.NameToLayer("Player")))
                    {
                        DestroyableObject obj = hit.transform.GetComponent<DestroyableObject>();


                        if (obj != null)
                        {
                            Rigidbody hitRigid = hit.transform.GetComponent<Rigidbody>();

                            if (hit.transform.GetComponent<EnemyAI>() == null)
                            {
                                obj.TakeDamage(1000f, hit.point);

                                if (hitRigid != null)
                                {
                                    hitRigid.AddForce(-hit.point * 500);
                                }

                            }
                        }
                    }

                    if (hit.point != null)
                    {
                        Vector3 pos = this.transform.position + (-hit.point + this.transform.forward * 2.3f);
                        Vector3 point = new Vector3(pos.x, this.transform.position.y, pos.z);
                        StartCoroutine(BlastToPosition(point));
                        this.transform.position = point;
                    }

                    //StartCoroutine(InvokeSwapState(3.0f, states.ElementAt(0)));
                    SwapState(states.ElementAt(0));
                    //Invoke("SwapState", 5.0f, states.ElementAt(0));
                }

                nextCuttingEdgeTime = Time.time * 1.2f;
            }
        }
    }

    public IEnumerator InvokeSwapState(float delay, State<BossAI> newState)
    {
        yield return new WaitForSeconds(delay);
        SwapState(newState);
    }

    private bool preformedAction = false;

    private GameObject BehindLeft;
    private GameObject BehindRight;

    [HideInInspector] public List<GameObject> objectStorage = new List<GameObject>();
    
    private float currentStateTimer = 0f;
    private float timeOfLastStateStart = 0f;
    private float lengthOfState = 0f;

    private bool doingBlackout = false;

    private int direction;

    public void ExecuteNextPhaseTwoState()
    {
        if (states.Count > 0)
        {
            State<BossAI> nextState = states.ElementAt(0);
            SwapState(nextState);
            lastState = states.ElementAt(0);
            states.RemoveAt(0);
        } else
        {
            Debug.Log("No More States! Resetting!");
        }
    }

    public void ExecutePhaseTwoState()
    {
        if (doingBlackout == false)
        {
            if (BehindLeft != null && BehindRight != null)
            {
                target.GetComponent<PlayerCube>().ableToMove = false;
                BehindLeft.transform.position = target.transform.position + (Vector3.back + Vector3.left) * 2.4f;
                BehindRight.transform.position = target.transform.position + (Vector3.back + Vector3.right) * 2.4f;
                this.transform.position = target.transform.position + (Vector3.forward) * 3.0f;
                BehindLeft.GetComponent<Rigidbody>().isKinematic = true;
                BehindRight.GetComponent<Rigidbody>().isKinematic = true;
                BehindLeft.GetComponent<EnemyAI>().ai.isStopped = true;
                BehindRight.GetComponent<EnemyAI>().ai.isStopped = true;
                ai.isStopped = true;
                BehindLeft.transform.LookAt(target);
                BehindRight.transform.LookAt(target);
                this.transform.LookAt(target);
            }

            if (currentState != null && currentState.GetType() == typeof(TripletMultiply) && target != null)
            {
                if (preformedAction == false)
                {
                    target.transform.position = new Vector3(0f, 0.5f, 0f);
                    lengthOfState = 5f;
                    for (int i = 0; i < 2; i++)
                    {
                        GameObject newEnemy = PhotonNetwork.Instantiate("Enemies/" + this.transform.name, this.transform.position, this.transform.rotation, 0);
                        newEnemy.transform.SetParent(GameObject.Find("EnemyStorage").transform);
                        if (i == 0)
                        {
                            BehindLeft = newEnemy;
                        }
                        else
                        {
                            BehindRight = newEnemy;
                        }

                        Destroy(newEnemy.GetComponent<TerrorizerAI>());
                        EnemyAI eAI = newEnemy.AddComponent<EnemyAI>();
                        newEnemy.GetComponent<DestroyableObject>().maxHealth = 1000f;
                        newEnemy.GetComponent<DestroyableObject>().health = 1000f;
                        eAI.damage = 0;
                        eAI.fireRate = fireRate;
                        eAI.range = range;
                        eAI.target = target;
                        eAI.ScoreGiven = 5;
                    }
                    preformedAction = true;
                }
            }

            if (currentState != null && currentState.GetType() == typeof(BoxBlast) && target != null)
            {
                if (preformedAction == false)
                {
                    ai.isStopped = true;
                    this.transform.position = new Vector3(10f, 0.5f, 10f);

                    lengthOfState = 2f;


                    target.transform.position = new Vector3(1f, 0.6f, -1f);
                    target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    StartCoroutine(SummonBox());
                    preformedAction = true;
                }
            }

            if (currentState != null && currentState.GetType() == typeof(EnemyBombardment) && target != null)
            {
                if (preformedAction == false)
                {
                    this.GetComponent<SpawnSystem>().enabled = true;
                    this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

                    lengthOfState = 20f;
                    preformedAction = true;
                }
            }

            if (currentState != null && currentState.GetType() == typeof(Missiles) && target != null)
            {
                if (preformedAction == false)
                {

                    ai.isStopped = true;
                    lengthOfState = 3f;


                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 rightPosition = this.transform.position + this.transform.right * 1.5f;

                        GameObject projectile = PhotonNetwork.Instantiate("Bullet", rightPosition, this.transform.rotation, 0);
                        //GameObject projectile = Instantiate(Resources.Load<GameObject>("Bullet"), rightPosition, this.transform.rotation);
                        projectile.GetComponent<Bullet>().shooter = this.transform;
                        Destroy(projectile.GetComponent<Bullet>());
                        projectile.AddComponent<Missile>();
                        projectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        //projectile.GetComponent<Missile>().enabled = false;
                        projectile.GetComponent<Missile>().shooter = this.transform;
                        projectile.GetComponent<Missile>().target = target;
                        projectile.GetComponent<Missile>().ogName = "Bullet";
                        projectile.GetComponent<Missile>().splitMissile = true;
                        projectile.GetComponent<Missile>().amountToSplit = 6;
                        projectile.GetComponent<Missile>().isExplosive = true;
                        projectile.GetComponent<Missile>().driveSpeed = 12.5f;
                        //projectile.GetComponent<Missile>().enabled = true;

                        //projectile.GetComponent<Missile>().isHoming = true;
                        projectile.transform.SetParent(GameObject.Find("TempStorage").transform);

                    }

                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 leftPosition = this.transform.position + (-this.transform.right) * 1.5f;

                        GameObject projectile = PhotonNetwork.Instantiate("Bullet", leftPosition, this.transform.rotation, 0);
                        //GameObject projectile = Instantiate(Resources.Load<GameObject>("Bullet"), rightPosition, this.transform.rotation);
                        projectile.GetComponent<Bullet>().shooter = this.transform;
                        Destroy(projectile.GetComponent<Bullet>());
                        projectile.AddComponent<Missile>();
                        projectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        //projectile.GetComponent<Missile>().enabled = false;
                        projectile.GetComponent<Missile>().shooter = this.transform;
                        projectile.GetComponent<Missile>().target = target;
                        projectile.GetComponent<Missile>().ogName = "Bullet";
                        projectile.GetComponent<Missile>().splitMissile = true;
                        projectile.GetComponent<Missile>().amountToSplit = 6;
                        projectile.GetComponent<Missile>().driveSpeed = 15f;
                        //projectile.GetComponent<Missile>().enabled = true;

                        //projectile.GetComponent<Missile>().isHoming = true;
                        projectile.transform.SetParent(GameObject.Find("TempStorage").transform);
                    }
                    ai.isStopped = false;
                    preformedAction = true;
                }

            }

            //if (currentState != null && currentState.GetType() == typeof(BoxBombarder) && target != null)
            //{
            //    if (preformedAction == false)
            //    {
            //        lengthOfState = 20f;
            //        ai.isStopped = true;
            //        StartCoroutine(BoxBombardment(10));
            //        preformedAction = true;
            //    }
            //}

            if (currentState != null && currentState.GetType() == typeof(WarMode) && target != null)
            {
                currentPhase = 3;
            }

            currentStateTimer += Time.deltaTime;
            if (currentStateTimer >= timeOfLastStateStart + lengthOfState)
            {
                StartCoroutine(ScreenFlashStateSet());
            }
        }
    }

    IEnumerator BoxBombardment(int timesOfBox)
    {
        for (int i = 0; i < timesOfBox; i++)
        {
            yield return StartCoroutine(SummonBox());
            yield return new WaitForSeconds(2f);
        }
        Debug.Log("DONE THE BOX BOMBARDING");
    }

    IEnumerator SummonBox()
    {
        ai.isStopped = true;
        float appearDelay = 0.75f;
        float tileFlashSpeed = 5f;
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = false;
        Transform cam = Camera.main.transform;

        BoxBlast bb = currentState as BoxBlast;

        Quaternion originalRotation = cam.rotation;

        Vector3 originalPosition = cam.position;

        if (currentState.GetType() == typeof(BoxBlast))
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, 90f, 90f * bb.direction);
        } else
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, 90f, 90f * 0);
        }
        Camera.main.transform.position = new Vector3(-5f, 1.5f, -1f);

        GameMapGenerator map = GameObject.Find("MapGenerator").GetComponent<GameMapGenerator>();
        Transform blastTile = map.GetTileFromPosition(target.position);

        Material tileMat = blastTile.GetComponent<Renderer>().material;
        Color initialColor = tileMat.color;
        Color flashColor = Color.yellow;

        float spawnTimer = 0f;

        while (spawnTimer < appearDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        tileMat.color = initialColor;

        GameObject SpawnedObject = this.gameObject;
        int random = Random.Range(1, 10);

        if (random <= 5) {
            float positiveOrNegative = Random.Range(-1f, 1f) * 15;

            SpawnedObject = PhotonNetwork.Instantiate("Box", new Vector3(blastTile.position.x, blastTile.position.y + 0.5f, blastTile.position.z + positiveOrNegative), Quaternion.Euler(0, 0, 0), 0);
            SpawnedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX;

        }
        if (random > 5)
        {
            float positiveOrNegative = Random.Range(-1f, 1f) * 15;

            SpawnedObject = PhotonNetwork.Instantiate("Box", new Vector3(blastTile.position.x + positiveOrNegative, blastTile.position.y + 0.5f, blastTile.position.z), Quaternion.Euler(0, 0, 0), 0);
            SpawnedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        }

        // = PhotonNetwork.Instantiate("Box", new Vector3(blastTile.position.x, blastTile.position.y + 0.5f, blastTile.position.z + 15f), Quaternion.Euler(0, 0, 0), 0);
        objectStorage.Add(SpawnedObject);
        Bullet bullet = SpawnedObject.GetComponent<Bullet>();
        bullet.transform.LookAt(target);

        bullet.shooter = this.transform;
        //bullet.goesForward = false;
        bullet.isExplosive = true;
        //bullet.breaks = false;
        bullet.mineMode = true;
        bullet.damage = 1000f;

        SpawnedObject.GetComponent<Rigidbody>().velocity = (target.transform.position - SpawnedObject.transform.position).normalized * 15f;
        SpawnedObject.GetComponent<Rigidbody>().AddForce(SpawnedObject.transform.forward * 7.5f, ForceMode.VelocityChange);


        //Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = true;
        //cam.rotation = originalRotation;
        //cam.position = originalPosition;
        ai.isStopped = false;

        StartCoroutine(FixCamera(originalRotation, originalPosition, cam));
    }

    public IEnumerator FixCamera(Quaternion originalRotation, Vector3 originalPosition, Transform cam)
    {
        yield return new WaitForSeconds(0.3f);
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = true;
        cam.rotation = originalRotation;
        cam.position = originalPosition;
    }

    public IEnumerator ScreenFlashStateSet()
    {
        doingBlackout = true;
        this.GetComponent<SpawnSystem>().enabled = false;
        if (BehindLeft != null)
        {
            PhotonNetwork.Destroy(BehindLeft);
            BehindLeft = null;
        }

        if (BehindRight != null)
        {
            PhotonNetwork.Destroy(BehindRight);
            BehindRight = null;
        }
        foreach (GameObject obj in objectStorage.ToArray())
        {
            if (obj != null)
            {
                objectStorage.Remove(obj);
                PhotonNetwork.Destroy(obj);
            }
        }

        objectStorage.Clear();

        foreach (Transform obj in GameObject.Find("TempStorage").transform)
        {
            if (obj.GetComponent<PhotonView>() != null)
            {
                PhotonNetwork.Destroy(obj.gameObject);
            } else
            {
                Destroy(obj.gameObject);
            }
        }
        
        GameObject canvas = GameObject.Find("Canvas");
        Transform screenCover = canvas.transform.Find("ScreenCover");
        screenCover.gameObject.SetActive(true);
        Vector3 originalPosition = this.transform.position;
        Quaternion originalRotation = this.transform.rotation;
        this.transform.position = new Vector3(0f, 0.5f, 10f);
        target.GetComponent<PlayerCube>().ableToMove = true;
        ai.isStopped = true;
        this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        target.GetComponent<Rigidbody>().velocity = Vector3.zero;
        yield return new WaitForSeconds(0.8f);
        //RandomizeStateWithoutDelay(states.IndexOf(currentState));+-
        ExecuteNextPhaseTwoState();
        this.transform.position = originalPosition;
        this.transform.rotation = originalRotation; 
        screenCover.gameObject.SetActive(false);
        preformedAction = false;
        ai.isStopped = false;
        doingBlackout = false;
        timeOfLastStateStart = Time.time;
        currentStateTimer = Time.time;
    }



    public void OnBulletHit(GameObject Hit, Collision collision, float DamageAmount)
    {
        DestroyableObject hitDesObj = Hit.transform.GetComponent<DestroyableObject>();
        if (hitDesObj != null)
        {
            ContactPoint point = collision.contacts[0];
            Vector3 hitpoint = point.point;
            hitDesObj.TakeDamage(DamageAmount, hitpoint);
        }

        Rigidbody hitRigid = Hit.transform.GetComponent<Rigidbody>();
        Rigidbody rb = this.transform.GetComponent<Rigidbody>();
        if (hitRigid != null)
        {
            if (rb != null)
            {
                return;
            }

            hitRigid.AddForce(-collision.contacts[0].point * 100);
        }
    }


    public void ReEnableMissiles()
    {
        remainingMissilesToLaunch = 6;
    }

    Vector3 velocity = Vector3.zero;

    
    public IEnumerator BlastToPosition(Vector3 destination)
    {
        ai.isStopped = true;
        this.GetComponent<Rigidbody>().isKinematic = true;
        this.GetComponent<Rigidbody>().useGravity = false;


        float t = 0;

        while (t <= 1.0f)
        {
            t += Time.deltaTime;
            this.transform.position = Vector3.Lerp(this.transform.position, destination, t);
            yield return new WaitForFixedUpdate();
        }

        this.GetComponent<Rigidbody>().isKinematic = false;
        this.GetComponent<Rigidbody>().useGravity = true;
        this.transform.position = new Vector3(destination.x, 0.5f, destination.z);
    }


    //Phase 1 States
    [SerializeField] public sealed class TargetTeleport : BaseState { }
    [SerializeField] public sealed class MissileLaunch : BaseState { }
    [SerializeField] public sealed class CuttingEdge : BaseState { }

    //Phase 2 States
    [SerializeField] public sealed class TripletMultiply : BaseState { }
    [SerializeField] public sealed class BoxBlast : BaseState {
        public int direction = 0;

        public BoxBlast(int _direction)
        {
            direction = _direction;
        }
    }

    [SerializeField] public sealed class Missiles : BaseState { }
    [SerializeField] public sealed class EnemyBombardment : BaseState { }
    [SerializeField] public sealed class BoxBombarder : BaseState { }

    //Final Phase
    [SerializeField] public sealed class WarMode : BaseState { }


}


