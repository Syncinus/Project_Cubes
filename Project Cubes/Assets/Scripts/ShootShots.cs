using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EZCameraShake;
using Photon.Pun;
using Photon.Realtime;

public class ShootShots : MonoBehaviourPunCallbacks {

	public bool particlesOn = true;
	public float damage = 10f;
	public float range = 10f;
	public float impactForce = 50f;
	public float fireRate = 15f;
	public float beamRechargeTime = 10.0f;
	public bool beamReady = true;
	public float beamrange = 100f;
	public float beamdamage = 1000f;
	public float beamImpactForce = 50f;
	public bool beamHyperMode = false;
	private bool ableToShoot = true;
	private float lastTimeFired = 0f;

	private float timeOfLastFire;
	public ParticleSystem blast;
	public ParticleSystem beam;
    private ParticleSystem psystem;

    private AudioClip sound;
    private AudioSource source;


	public GameObject impactEffect;
    public GameObject cameraHolderForShaking;
    
	public GameObject shooter;
	public Color ColorOfParticles;
    [HideInInspector] public Vector3 realPosition;
    
	private float nextTimeToFire = 0f;
    private bool finishedGrowingLine = false;
    [HideInInspector] public bool ableToPlaySound = true;
    [HideInInspector] public bool ableToFire = true;
    private bool createdParticles;
    private WeaponItem weapon;
    private LineRenderer lineRenderer;
    private PlayerCube ccCube;

    private void Awake()
    {
        source = this.GetComponent<AudioSource>();
        lineRenderer = this.GetComponent<LineRenderer>();
        ccCube = this.GetComponent<PlayerCube>();
    }


    public void Start()
    {
        if (photonView.IsMine != true)
        {
            return;
        }

        psystem = blast;
        createdParticles = false;
        StartCoroutine(WaitForDefaultWeapon());
    }

    public void FixedUpdate()
    {
        if (photonView.IsMine != true)
        {
            return;
        }
        ParticleSystem[] systems = this.transform.GetComponentsInChildren<ParticleSystem>();
        /*
        foreach (ParticleSystem system in systems)
        {
            system.transform.position = this.transform.position + this.transform.forward;
            float increase = 0f;
            if (this.transform.Find("ArmorHolder") != null)
            {
                foreach (Transform child in this.transform.Find("ArmorHolder").transform)
                {
                    increase += 0.2f;
                }
            }
            system.transform.localPosition = new Vector3(0, 0, 1 + increase);
        }
        */
    }

    IEnumerator WaitForDefaultWeapon()
    {
        yield return new WaitUntil(() => this.GetComponent<PlayerCube>().weapon != null);
        weapon = this.GetComponent<PlayerCube>().weapon;
    }

    public void Update()
    {
        if (photonView.IsMine != true)
        {
            return;
        }

        PlayerCube cube = this.GetComponent<PlayerCube>();
        lastTimeFired += Time.deltaTime;


        if (cube.weapon != weapon && cube.weapon != null)
        {
            damage = cube.weapon.damage;
            fireRate = cube.weapon.firerate;
            range = cube.weapon.range;
            var main = blast.main;
            main.startColor = cube.weapon.particlesColor;
            ColorOfParticles = cube.weapon.particlesColor;
            var impactmain = impactEffect.GetComponent<ParticleSystem>().main;
            impactmain.startColor = cube.weapon.particlesColor;
            createdParticles = false;
            weapon = cube.weapon;
        }

        if (this.GetComponent<LineRenderer>().enabled == true)
        {
            photonView.RPC("SetLinePositions", RpcTarget.AllBuffered, this.transform.position, this.transform.position + (this.transform.forward * range));

            if (cube.weapon.type == WeaponType.Laser)
            {
                RaycastHit hit;
                if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, range, ~(1 << LayerMask.NameToLayer("Pieces"))))
                {
                    photonView.RPC("SetLinePositions", RpcTarget.AllBuffered, this.transform.position, hit.point);
                }
            }

            if (cube.weapon.weaponHasRecharge == true)
            {
                if (lastTimeFired >= timeOfLastFire + (cube.weapon.weaponRechargeTime / 2) && finishedGrowingLine == true)
                {


                    //StartCoroutine(DisableLineRenderer(cube.weapon.lineWidth));
                    photonView.RPC("disableLineRenderer", RpcTarget.AllBuffered, cube.weapon.lineWidth);

                    //StartCoroutine(disableLine());
                }
            }

            if (cube.weapon.type != WeaponType.Rail && cube.weapon.type != WeaponType.Laser)
            {
                photonView.RPC("disableLineRenderer", RpcTarget.AllBuffered, cube.weapon.lineWidth);
            }
        }


        if (ableToFire == true) {
            if (beamHyperMode == false)
            {

                if (cube.weapon.auto == true)
                {
                    if (Input.GetKey(KeyCode.Space) && Time.time >= nextTimeToFire && ableToShoot == true || Input.GetMouseButton(0) && Time.time >= nextTimeToFire && ableToShoot == true)
                    {
                        if (cube.weapon.weaponHasRecharge == false)
                        {
                            nextTimeToFire = Time.time + 1f / fireRate;
                            lastTimeFired = Time.time;
                            timeOfLastFire = Time.time;
                        }
                        else
                        {
                            nextTimeToFire = cube.weapon.weaponRechargeTime;
                            if (cube.weapon.type == WeaponType.Laser || cube.weapon.type == WeaponType.Rail)
                            {
                                nextTimeToFire += 0.05f;
                            }
                            ableToShoot = false;
                            //lastTimeFired = Time.time;
                            //timeOfLastFire = Time.time;
                        }

                        Shoot(false);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextTimeToFire && ableToShoot == true || Input.GetMouseButtonDown(0) && Time.time >= nextTimeToFire && ableToShoot == true)
                    {
                        if (cube.weapon.weaponHasRecharge == false)
                        {
                            nextTimeToFire = Time.time + 1f / fireRate;
                            if (cube.weapon.type == WeaponType.Laser || cube.weapon.type == WeaponType.Rail)
                            {
                                nextTimeToFire += 0.05f;
                            }
                            lastTimeFired = Time.time;
                            timeOfLastFire = Time.time;
                        }
                        else
                        {
                            nextTimeToFire = cube.weapon.weaponRechargeTime;
                            if (cube.weapon.type == WeaponType.Laser || cube.weapon.type == WeaponType.Rail)
                            {
                                nextTimeToFire += 0.05f;
                            }
                            ableToShoot = false;
                            //lastTimeFired = Time.time;
                            //timeOfLastFire = Time.time;
                        }

                        Shoot(false);
                    }
                }
            }




            if (Input.GetKey(KeyCode.LeftShift) && beamReady == true || Input.GetMouseButtonDown(1) && beamReady == true)
            {
                Shoot(true);
                beamReady = false;
                Invoke("ReEnableBeam", beamRechargeTime);
            }
        }

        if (beamHyperMode == true)
        {
            if (Input.GetKey(KeyCode.Space) && Time.time >= nextTimeToFire || Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot(true);
            }
        }
    }

	public void ReEnableShots() {
		ableToShoot = true;
	}

	public void ReEnableBeam() {
		beamReady = true;
	}

	[PunRPC] public void PlayParticles(bool usingbeam, Vector3 fakeColor) {
        if (usingbeam == true) {
			beam.Play();
		} else {
			Color color = new Color(fakeColor.x, fakeColor.y, fakeColor.z, 1);
            ParticleSystem.MainModule main;
            ParticleSystem system = this.GetComponentInChildren<ParticleSystem>();
            main = system.main;
			main.startColor = color;
            system.Play();
		}
	}

	IEnumerator disableLine() {
		yield return new WaitForSeconds(0.7f);
		this.GetComponent<LineRenderer>().enabled = false;
	}

    IEnumerator ShakeCamera()
    {
        PlayerCube cube = this.transform.GetComponent<PlayerCube>();
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = false;
        Camera.main.transform.GetComponent<CameraShaker>().enabled = true;
        Vector3 camPosition = Camera.main.transform.localPosition;
        Quaternion camRotation = Camera.main.transform.localRotation;
        cameraHolderForShaking.transform.localPosition = camPosition;
        cameraHolderForShaking.transform.localRotation = camRotation;
        Camera.main.transform.SetParent(cameraHolderForShaking.transform);
        CameraShaker.Instance.ShakeOnce(cube.weapon.shakeMagnitude, cube.weapon.shakeRoughness, 0.1f, 0.5f);
        yield return new WaitForSeconds(1f);
        Camera.main.transform.GetComponent<CameraShaker>().enabled = false;
        Camera.main.transform.SetParent(this.transform);
        Camera.main.transform.localPosition = camPosition;
        Camera.main.transform.localRotation = camRotation;
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = true;
    }

    [PunRPC] public void growLineRenderer(float maxWidth, float time)
    {
        lineRenderer.enabled = true;
        StartCoroutine(GrowLineRenderer(maxWidth, time));
    }

    [PunRPC] public void disableLineRenderer(float time)
    {
        StartCoroutine(DisableLineRenderer(time));
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

    public void Shoot(bool beamMode)
    {
        if (photonView.IsMine != true)
        {
            return;
        }

        StartCoroutine(shoot(beamMode));
    }

    public IEnumerator ReEnableSoundPlay(float delay)
    {
        //source.Stop();
        yield return new WaitForSeconds(delay);
        ableToPlaySound = true;
    }
    
    [PunRPC] public void SetUpLine(Vector3 color)
    {
        LineRenderer line = lineRenderer;
        PlayerCube cube = ccCube;
        line.startColor = new Color(color.x, color.y, color.z);
        line.endColor = new Color(color.x, color.y, color.z);
        //line.material = new Material(Shader.Find("Particles/Additive"));
        line.positionCount = 2;
    }

    [PunRPC] public void SetLinePositions(Vector3 startPos, Vector3 endPos)
    {
        LineRenderer line = lineRenderer;
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
    }



    public IEnumerator shoot(bool beamMode)
    {
        if (photonView.IsMine != true)
        {
            yield return null;
        }
        PlayerCube cube = this.GetComponent<PlayerCube>();
        LineRenderer line = this.GetComponent<LineRenderer>();
        WeaponType WType = cube.weapon.type;



        if (particlesOn == true)
        {
            if (cube.weapon.hasParticles == true)
            {
                Vector3 fakeColor = new Vector3(ColorOfParticles.r, ColorOfParticles.g, ColorOfParticles.b);
                if (beamMode == true)
                {
                    photonView.RPC("PlayParticles", RpcTarget.AllBuffered, true, fakeColor);
                }
                if (beamMode == false)
                {
                    if (cube.weapon.hasParticles == true)
                    {
                        if (createdParticles == false)
                        {
                            GameObject particles = PhotonNetwork.Instantiate("Particles/" + cube.weapon.particles.name, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z) + this.transform.forward, this.transform.rotation, 0);
                            particles.transform.SetParent(this.transform);
                            particles.transform.localPosition = this.transform.position + this.transform.forward;
                            psystem = particles.GetComponent<ParticleSystem>();
                            createdParticles = true;
                        }
                        photonView.RPC("PlayParticles", RpcTarget.AllBuffered, false, fakeColor);
                    }
                }
            }

            if (cube.weapon.sound != null)
            {
               //Debug.Log("Sound!");
               if (ableToPlaySound == true) {
                    if (cube.weapon.name == "Box Blaster")
                    {
                        source.volume = 2f;
                    } else
                    {
                        source.volume = 0.3f;
                    }
                    source.PlayOneShot(sound);
                    if (cube.weapon.soundRefreshTime > 0)
                    {
                        ableToPlaySound = false;
                        yield return StartCoroutine(ReEnableSoundPlay(cube.weapon.soundRefreshTime));
                    }
               }
            }



            float properRange = 0f;
            if (beamMode == false)
            {
                properRange = range;
            }
            if (beamMode == true)
            {
                properRange = beamrange;
            }

            //PostProcessProfile oldProfile = Camera.main.GetComponent<PostProcessVolume>().profile;

            //if (cube.weapon.profile != null)
            //{
            //    Camera.main.GetComponent<PostProcessVolume>().profile = cube.weapon.profile;
            //}   

            switch (WType)
            {

                case WeaponType.Mowing:
                    #region mowing
                    RaycastHit hit;

                    float radiusShooting = 0.5f;
                    Vector3 shootingPoint = this.transform.position;
                    shootingPoint.x += UnityEngine.Random.Range(-radiusShooting, radiusShooting);
                    shootingPoint.y += UnityEngine.Random.Range(-radiusShooting, radiusShooting);
                    //shootingPoint.z += UnityEngine.Random.Range(-radiusShooting, radiusShooting);
                    if (Physics.Raycast(shootingPoint, shooter.transform.forward, out hit, properRange, ~(1 << LayerMask.NameToLayer("Pieces"))))
                    {
                        if (hit.transform.name == "Piece (1)" || hit.transform.name == "Piece (2)" || hit.transform.name == "Piece (3)" || hit.transform.name == "Piece (4)")
                            Debug.Log(hit.transform.name);

                        float radius = 0.25f;
                        Vector3 originPoint;
                        originPoint = hit.point;
                        originPoint.x += UnityEngine.Random.Range(-radius, radius);
                        originPoint.z += UnityEngine.Random.Range(-radius, radius);
                        originPoint.y += UnityEngine.Random.Range(-radius, radius);


                        GameObject impact = Instantiate(impactEffect, originPoint, Quaternion.LookRotation(hit.normal));
                        var main = impact.GetComponent<ParticleSystem>().main;
                        main.startColor = cube.weapon.particlesColor;

                        impact.transform.Find("Light").GetComponent<Light>().color = cube.weapon.particlesColor;
                        impact.transform.SetParent(GameObject.Find("TempStorage").transform);


                        if (hit.transform.GetComponent<EnemyAI>())
                        {

                            EnemyAI ai = hit.transform.GetComponent<EnemyAI>();
                            if (ai.targetChangeable == true)
                            {
                                ai.target = shooter.transform;
                            }
                        }

                        DestroyableObject target = hit.transform.GetComponent<DestroyableObject>();
                        if (target != null)
                        {
                            if (beamMode == false)
                            {
                                target.TakeDamage(damage, hit.point);
                            }
                            if (beamMode == true)
                            {
                                target.TakeDamage(beamdamage, hit.point);
                            }
                        }

                        Rigidbody hitRigid = hit.transform.GetComponent<Rigidbody>();
                        Rigidbody rb = this.transform.GetComponent<Rigidbody>();
                        if (hitRigid != null)
                        {
                            if (rb != null)
                            {
                                if (hitRigid == rb)
                                {
                                    yield return null;
                                }
                            }

                            /*
                            if (hit.transform.GetComponent<EnemyAI> ()) {
                                hit.transform.gameObject.GetComponent<NavMeshAgent> ().enabled = false;
                            }
                            */
                            if (beamMode == false)
                            {
                                hitRigid.AddExplosionForce(impactForce, hit.point, 100f);
                                hitRigid.AddForce(-hit.point * impactForce);
                            }

                            if (beamMode == true)
                            {
                                hitRigid.AddForce(-hit.normal * beamImpactForce);
                            }

                            /*
                            if (hit.transform.GetComponent<EnemyAI> ()) {
                                if (hitRigid != null) {
                                    hitRigid.Sleep ();
                                }
                                hit.transform.gameObject.GetComponent<NavMeshAgent> ().enabled = true;
                                hitRigid.WakeUp ();
                            }
                            */
                        }

                    }

                    break;
                    #endregion
                case WeaponType.Rail:
                    #region rail
                    List<RaycastHit> hits;
                    yield return new WaitForSeconds(cube.weapon.shotDelay);

                    hits = (Physics.SphereCastAll(this.transform.position, cube.weapon.lineWidth, this.transform.forward, range)).ToList();

                    Color pColor = cube.weapon.particlesColor;
                    Vector3 fakeColor = new Vector3(pColor.r, pColor.g, pColor.b);
                    photonView.RPC("SetUpLine", RpcTarget.AllBuffered, fakeColor);
                    photonView.RPC("SetLinePositions", RpcTarget.AllBuffered, this.transform.position, this.transform.position + (this.transform.forward * range));
                    photonView.RPC("growLineRenderer", RpcTarget.AllBuffered, cube.weapon.lineWidth, cube.weapon.lineWidth);

                    if (cube.weapon.shakesCamera == true)
                    {
                        StartCoroutine(ShakeCamera());
                    }


                    //StartCoroutine(disableLine());
                    foreach (RaycastHit Hit in hits)
                    {

                        GameObject impact = Instantiate(impactEffect, Hit.point, Quaternion.LookRotation(Hit.point));
                        impact.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                        impact.transform.SetParent(GameObject.Find("TempStorage").transform);


                        if (Hit.transform.GetComponent<EnemyAI>())
                        {

                            EnemyAI ai = Hit.transform.GetComponent<EnemyAI>();
                            if (ai.targetChangeable == true)
                            {
                                ai.target = shooter.transform;
                            }
                        }

                        DestroyableObject target = Hit.transform.GetComponent<DestroyableObject>();
                        if (target != null)
                        {
                            if (beamMode == false)
                            {
                                target.TakeDamage(damage, Hit.point);
                            }
                            if (beamMode == true)
                            {
                                target.TakeDamage(beamdamage, Hit.point);
                            }
                        }

                        Rigidbody hitRigid = Hit.transform.GetComponent<Rigidbody>();
                        Rigidbody rb = this.transform.GetComponent<Rigidbody>();
                        if (hitRigid != null)
                        {
                            if (rb != null)
                            {
                                if (hitRigid == rb)
                                {
                                    yield return null;
                                }
                            }

                            /*
                            if (hit.transform.GetComponent<EnemyAI> ()) {
                                hit.transform.gameObject.GetComponent<NavMeshAgent> ().enabled = false;
                            }
                            */
                            if (beamMode == false)
                            {
                                hitRigid.AddExplosionForce(impactForce, Hit.point, 500f);
                                hitRigid.AddForce(-Hit.point * 10000f);
                            }

                            if (beamMode == true)
                            {
                                hitRigid.AddForce(-Hit.normal * beamImpactForce);
                            }

                            /*
                            if (hit.transform.GetComponent<EnemyAI> ()) {
                                if (hitRigid != null) {
                                    hitRigid.Sleep ();
                                }
                                hit.transform.gameObject.GetComponent<NavMeshAgent> ().enabled = true;
                                hitRigid.WakeUp ();
                            }
                            */
                        }
                    }
                       

                    break;
                     #endregion
                case WeaponType.Laser:
                    #region laser
                    //line.enabled = true;
                    yield return new WaitForSeconds(cube.weapon.shotDelay);
                    //line.SetPosition(0, this.transform.position);
                    //line.SetPosition(1, this.transform.position + (this.transform.forward * range));

                    Color pColorx = cube.weapon.particlesColor;
                    Vector3 fakeColorx = new Vector3(pColorx.r, pColorx.g, pColorx.b);
                    photonView.RPC("SetUpLine", RpcTarget.AllBuffered, fakeColorx);
                    photonView.RPC("SetLinePositions", RpcTarget.AllBuffered, this.transform.position, this.transform.position + (this.transform.forward * range));
                    photonView.RPC("growLineRenderer", RpcTarget.AllBuffered, cube.weapon.lineWidth, cube.weapon.lineWidth);

                    if (cube.weapon.shakesCamera == true)
                    {
                        StartCoroutine(ShakeCamera());
                    }
                    
                    if (Physics.SphereCast(shooter.transform.position, cube.weapon.lineWidth, shooter.transform.forward, out hit, properRange, ~(1 << LayerMask.NameToLayer("Pieces"))))
                    {
                        if (hit.transform.name == "Piece (1)" || hit.transform.name == "Piece (2)" || hit.transform.name == "Piece (3)" || hit.transform.name == "Piece (4)")
                            Debug.Log(hit.transform.name);

                        float radius = 0.5f;
                        Vector3 originPoint;
                        originPoint = hit.point;
                        originPoint.x += UnityEngine.Random.Range(-radius, radius);
                        originPoint.z += UnityEngine.Random.Range(-radius, radius);
                        originPoint.y += UnityEngine.Random.Range(-radius, radius);


                        if (hit.transform.GetComponent<EnemyAI>())
                        {

                            EnemyAI ai = hit.transform.GetComponent<EnemyAI>();
                            if (ai.targetChangeable == true)
                            {
                                ai.target = shooter.transform;
                            }
                        }

                        photonView.RPC("SetLinePositions", RpcTarget.AllBuffered, this.transform.position, hit.point);


                        DestroyableObject target = hit.transform.GetComponent<DestroyableObject>();
                        if (target != null)
                        {
                            if (beamMode == false)
                            {
                                target.TakeDamage(damage, hit.point);
                            }
                            if (beamMode == true)
                            {
                                target.TakeDamage(beamdamage, hit.point);
                            }
                        }

                        Rigidbody hitRigid = hit.transform.GetComponent<Rigidbody>();
                        Rigidbody rb = this.transform.GetComponent<Rigidbody>();

                        if (hitRigid != null)
                        {
                            if (rb != null)
                            {
                                if (hitRigid == rb)
                                {
                                    yield return null;
                                }
                            }

                            /*
                            if (hit.transform.GetComponent<EnemyAI> ()) {
                                hit.transform.gameObject.GetComponent<NavMeshAgent> ().enabled = false;
                            }
                            */
                            if (beamMode == false)
                            {
                                hitRigid.AddExplosionForce(impactForce, hit.point, 100f);
                                hitRigid.AddForce(-hit.point * impactForce);
                            }

                            if (beamMode == true)
                            {
                                hitRigid.AddForce(-hit.normal * beamImpactForce);
                            }

                            /*
                            if (hit.transform.GetComponent<EnemyAI> ()) {
                                if (hitRigid != null) {
                                    hitRigid.Sleep ();
                                }
                                hit.transform.gameObject.GetComponent<NavMeshAgent> ().enabled = true;
                                hitRigid.WakeUp ();
                            }
                            */
                        }

                    }


                    break;
                #endregion
                case WeaponType.Projectile:
                    #region projectile
                    yield return new WaitForSeconds(cube.weapon.shotDelay);

                    if (cube.weapon.shakesCamera == true)
                    {
                        StartCoroutine(ShakeCamera());
                    }

                    GameObject bullet = PhotonNetwork.Instantiate(cube.weapon.projectile.name, this.transform.position + this.transform.forward * 2, this.transform.rotation, 0);
                    //bullet.transform.SetParent(this.transform);
                    bullet.GetComponent<Bullet>().shooter = this.transform;
                    bullet.transform.SetParent(GameObject.Find("TempStorage").transform);
                    bullet.GetComponent<Rigidbody>().AddForce(-this.transform.position + this.transform.forward * 50);
                    bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 100;
                    Destroy(bullet, 2.0f);
                    break;
                #endregion
                case WeaponType.SeekingProjectile:
                    #region seekingprojectile
                    yield return new WaitForSeconds(cube.weapon.shotDelay);

                    if (cube.weapon.shakesCamera == true)
                    {
                        StartCoroutine(ShakeCamera());
                    }

                    GameObject projectile = PhotonNetwork.Instantiate(cube.weapon.projectile.name, this.transform.position + this.transform.forward * 1.4f, this.transform.rotation, 0);
                    projectile.GetComponent<Bullet>().shooter = this.transform;
                    Destroy(projectile.GetComponent<Bullet>());
                    projectile.AddComponent<Missile>();
                    //projectile.GetComponent<Missile>().enabled = true;
                    projectile.GetComponent<Missile>().ogName = cube.weapon.projectile.name;
                    projectile.GetComponent<Missile>().shooter = this.transform;
                    //projectile.GetComponent<Missile>().enabled = true;
                    projectile.transform.SetParent(GameObject.Find("TempStorage").transform);

                    line.enabled = true;
                    line.SetVertexCount(2);
                    line.material = new Material(Shader.Find("Particles/Additive"));
                    line.SetColors(Color.red, Color.red);
                    line.SetPosition(0, this.transform.position);
                    line.SetPosition(1, this.transform.position + (this.transform.forward * range));

                    if (Physics.Raycast(shooter.transform.position, shooter.transform.forward, out hit, properRange, ~(1 << LayerMask.NameToLayer("Pieces"))))
                    {
                        projectile.GetComponent<Missile>().target = hit.transform;
                        line.SetPosition(1, hit.point);
                    }

                    break;
                    #endregion
            }

            lastTimeFired = Time.time;
            timeOfLastFire = Time.time;
            if (cube.weapon.weaponHasRecharge == true)
            {
                nextTimeToFire = cube.weapon.weaponRechargeTime;
                Invoke("ReEnableShots", cube.weapon.weaponRechargeTime);
            }

  
                //-Camera.main.GetComponent<PostProcessVolume>().profile = oldProfile;

        }
    }

    public void OnBulletHit(GameObject Hit, Collision collision)
    {
        if (Hit.transform.GetComponent<EnemyAI>())
        {

            EnemyAI ai = Hit.transform.GetComponent<EnemyAI>();
            if (ai.targetChangeable == true)
            {
                ai.target = shooter.transform;
            }
        }

        DestroyableObject target = Hit.transform.GetComponent<DestroyableObject>();
        if (target != null)
        {
            ContactPoint point = collision.contacts[0];
            Vector3 hitpoint = point.point;
            target.TakeDamage(damage, hitpoint);
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

    IEnumerator Line (LineRenderer line, Transform[] points, float lineConnectWaitTime) {
        int seg;
		seg = points.Length;
		Vector3[] vP;
		vP = new Vector3[points.Length];
		for (int i = 0; i < points.Length; i++) {
			vP[i] = points[i].position;
		}


		for (int i = 0; i < seg; i++) {
			float t = i / (float) seg;
			line.SetVertexCount(seg);
			line.SetPosition(i, vP[i]);
			line.SetPosition(0, this.transform.position);
			yield return new WaitForSeconds(0.1f);
		}
	}
}
