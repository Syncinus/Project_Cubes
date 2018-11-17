using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EZCameraShake;
using Photon.Pun;
using Photon.Realtime;

public class ShootShots : MonoBehaviourPunCallbacks {
    private List<float> NextTimesToFire = new List<float>();
    private float NextTimeToPlaySound;
    private WeaponItem Weapon;
    private PlayerCube Cube;

    private void Awake()
    {
        Cube = this.GetComponent<PlayerCube>();
        Weapon = Cube.weapon;
        foreach (FiringPoint Point in Weapon.Points)
        {
            NextTimesToFire.Add(0);
        }
    }

    public void Update()
    {
        Outline();

        if (Input.GetMouseButton(0))
        {
            foreach(FiringPoint Point in Weapon.Points)
            {
                int Index = Weapon.Points.IndexOf(Point);
                if (Time.time > NextTimesToFire.ElementAt(Index))
                {
                    Shoot(Point, Index);
                }
            }
        }

        foreach (FiringPoint Point in Weapon.Points)
        {
            int Index = Weapon.Points.IndexOf(Point);
            if (this.transform.Find("Line: " + Index.ToString()) != null)
            {
                GameObject LineObject = this.transform.Find("Line: " + Index.ToString()).gameObject;
                Vector3 SecondPoint = this.transform.position + (this.transform.forward * Point.Shooting.Range);
                RaycastHit Hit;
                if (Physics.Raycast(this.transform.position, this.transform.forward, out Hit, Point.Shooting.Range))
                {
                    SecondPoint = Hit.point;
                }
                SetLinePositions(LineObject.GetComponent<LineRenderer>(), this.transform.position, SecondPoint);

                if (LineObject.GetComponent<LineRenderer>().startWidth >= Point.Particles.Size - 0.1f)
                {
                    StopCoroutine("GrowLineRenderer");
                    StartCoroutine(ShrinkLineRenderer(LineObject.GetComponent<LineRenderer>(), 0.25f));
                }
            }
        }

        
        if (Input.GetMouseButtonUp(0))
        {
            foreach (FiringPoint Point in Weapon.Points)
            {
                int Index = Weapon.Points.IndexOf(Point);
                Transform Particles = this.transform.Find("Particles" + Index);
                if (Particles != null)
                {
                    if (Particles.GetComponent<Kvant.SprayMV>() != null)
                    {
                        Particles.GetComponent<Kvant.SprayMV>().throttle = 0;
                    }
                    else if (Particles.GetComponent<Kvant.SwarmMV>() != null)
                    {
                        Particles.GetComponent<Kvant.SwarmMV>().throttle = 0;
                    }
                    else
                    {
                        Particles.gameObject.SetActive(false);
                    }
                }
            }

            /*
            if (Weapon.Particles.Type == "KvantSprayMV")
            {
                this.transform.Find("Particles").GetComponent<Kvant.SprayMV>().throttle = 0;
            } else
            {
                this.transform.Find("Particles").gameObject.SetActive(false);
            }
            */
        }
        
    }

    public IEnumerator GrowLineRenderer(LineRenderer Line, float maxWidth, float time, Color color)
    {
        Vector2 CurrentWidth = Vector2.zero;
        Vector2 StartWidth = Vector2.zero;
        Vector2 EndWidth = new Vector2(maxWidth, maxWidth);

        float timer = 0f;
        Line.enabled = true;
        Line.startColor = color;
        Line.endColor = color;

        do
        {
            CurrentWidth = Vector2.Lerp(StartWidth, EndWidth, timer / time);
            SetLineWidth(Line, CurrentWidth.x, CurrentWidth.y);
            timer += Time.deltaTime;
            yield return null;
        } while (timer < time);
    }



    public IEnumerator ShrinkLineRenderer(LineRenderer Line, float time)
    {
        Vector2 CurrentWidth = new Vector2(Line.startWidth, Line.endWidth);
        Vector2 StartWidth = CurrentWidth;
        Vector2 EndWidth = Vector2.zero;

        float timer = 0f;

        do
        {
            CurrentWidth = Vector2.Lerp(StartWidth, EndWidth, timer / time);
            SetLineWidth(Line, CurrentWidth.x, CurrentWidth.y);
            timer += Time.deltaTime;
            yield return null;
        } while (timer < time);

        Line.enabled = false;
    }

    public IEnumerator DrawLineRenderer(LineRenderer Line, Vector3 startPos, Vector3 endPos, float time)
    {
        Vector3 CurrentPos = startPos;

        float timer = 0f;

        do
        {
            CurrentPos = Vector3.Lerp(startPos, endPos, timer / time);
            SetLinePositions(Line, startPos, CurrentPos);
            timer += Time.deltaTime;
            yield return null;
        } while (timer < time);

        SetLinePositions(Line, startPos, endPos);
    }

    public void SetLinePositions(LineRenderer Line, Vector3 StartPosition, Vector3 EndPosition)
    {
        Line.SetPosition(0, StartPosition);
        Line.SetPosition(1, EndPosition);
    }

    public void SetLineWidth(LineRenderer Line, float StartWidth, float EndWidth)
    {
        Line.startWidth = StartWidth;
        Line.endWidth = EndWidth;
    }


    [PunRPC] public void PlayParticles(Vector3 ParticlesColor, string ParticlesName, string ParticlesType, Vector3 ParticlesPosition, Quaternion ParticlesRotation, int ParticlesIndex, float Size)
    {
        GameObject ParticleObject = null;
        Color Coloring = new Color(ParticlesColor.x, ParticlesColor.y, ParticlesColor.z);

        if (ParticlesType == "LineRenderer") {
            GameObject Line = null;
            LineRenderer l = null;
            if (!this.transform.Find("Line: " + ParticlesIndex.ToString()))
            {
                Line = new GameObject();
                Line.transform.name = "Line: " + ParticlesIndex.ToString();
                Line.transform.SetParent(this.transform);
                Line.transform.localPosition = Vector3.zero;
                Line.transform.localRotation = Quaternion.Euler(Vector3.zero);
                l = Line.AddComponent<LineRenderer>();
                l.material = Resources.Load<Material>("Materials/Particle/LineMaterial");
            } else
            {
                Line = this.transform.Find("Line: " + ParticlesIndex.ToString()).gameObject;
                l = Line.GetComponent<LineRenderer>();
            }
            SetLinePositions(l, this.transform.position, this.transform.position + (this.transform.forward * Weapon.Points[ParticlesIndex].Shooting.Range));
            StopCoroutine("ShrinkLineRenderer");
            StartCoroutine(GrowLineRenderer(l, Size, 0.25f, Coloring));
        }
        else
        {
            if (!this.transform.Find("Particles" + ParticlesIndex.ToString()))
            {
                ParticleObject = PhotonNetwork.Instantiate("Particles/" + ParticlesName, this.transform.position, ParticlesRotation, 0);
                ParticleObject.transform.SetParent(this.transform);
                ParticleObject.transform.localPosition = ParticlesPosition;
                ParticleObject.transform.localScale = new Vector3(Size, Size, Size);
                ParticleObject.transform.name = "Particles" + ParticlesIndex.ToString();

                if (ParticlesType == "ParticleSystem")
                {
                    var Main = ParticleObject.GetComponent<ParticleSystem>().main;
                    Main.startColor = Coloring;
                }

                if (ParticlesType == "KvantSprayMV")
                {

                }

                if (ParticlesType == "KvantSwarmMV")
                {

                }
            }
            else
            {
                ParticleObject = this.transform.Find("Particles" + ParticlesIndex.ToString()).gameObject;
                ParticleObject.SetActive(true);

                if (ParticlesType == "ParticleSystem")
                {
                    ParticleObject.GetComponent<ParticleSystem>().Play();
                }
                if (ParticlesType == "KvantSprayMV")
                {
                    ParticleObject.GetComponent<Kvant.SprayMV>().throttle = 1;
                }
                if (ParticlesType == "KvantSwarmMV")
                {
                    ParticleObject.GetComponent<Kvant.SwarmMV>().throttle = 1;
                    RaycastHit hit;
                    if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, 100))
                    {
                        float dist = Vector3.Distance(this.transform.position + this.transform.forward, hit.transform.position);
                        float z = dist * 0.125f;
                        Vector3 vector = new Vector3(0f, 0f, z);
                        ParticleObject.GetComponent<Kvant.SwarmMV>().flow = vector;
                    } 
                }
            }
        }
    }

    private GameObject OutlinedObject = null;

    public void Outline()
    {
        foreach (FiringPoint Point in Weapon.Points)
        {
            RaycastHit hit;
            var dir = Quaternion.Euler(Point.Emmision.RotationOffset) * this.transform.forward;
            Ray ray = new Ray(this.transform.position, this.transform.forward);
            var Layers = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Pieces");
            Layers = ~Layers;
            if (Physics.Raycast(ray, out hit, Point.Shooting.Range, Layers))
            {
                if (hit.transform != this.transform)
                {
                    Debug.Log("Outline Object: " + hit.transform.name);
                    if (!hit.transform.gameObject.GetComponent<cakeslice.Outline>())
                    {
                        hit.transform.gameObject.AddComponent<cakeslice.Outline>();
                    }
                    if (hit.transform.gameObject != OutlinedObject)
                    {
                        if (OutlinedObject != null)
                        {
                            cakeslice.Outline o = OutlinedObject.GetComponent<cakeslice.Outline>();
                            Destroy(o);
                        }
                        OutlinedObject = hit.transform.gameObject;
                    }
                }
            } else
            {
                if (OutlinedObject != null)
                {
                    cakeslice.Outline o = OutlinedObject.GetComponent<cakeslice.Outline>();
                    Destroy(o);
                    OutlinedObject = null;
                }
            }
        }
    }

    public void Shoot(FiringPoint Point, int Index)
    {
        EmmisionMode Emmision = Point.Emmision;
        ShotMode Shooting = Point.Shooting;
        ParticleMode Particles = Point.Particles;
        SoundMode Sound = Point.Sound;

        Vector3 ShootPosition = this.transform.position + this.transform.TransformDirection(Vector3.forward);
        Vector3 ParticlesPosition = Emmision.EmmisionOffset + Particles.Position;
        Quaternion ShootRotation = this.transform.rotation * Quaternion.Euler(Emmision.RotationOffset);

        if (Particles != null)
        {
            Vector3 ParticleColor = new Vector3(Particles.Coloring.r, Particles.Coloring.g, Particles.Coloring.b);
            PhotonNetwork.RPC(this.photonView, "PlayParticles", RpcTarget.AllBuffered, false, ParticleColor, 
                Particles.Prefab, Particles.Type, ParticlesPosition, ShootRotation, Index, Particles.Size);
        }


        if (Sound != null && Time.time > NextTimeToPlaySound)
        {
            AudioSource Source = this.GetComponent<AudioSource>();
            Source.clip = Sound.Clip;
            Source.volume = Sound.Volume;
            Source.Play();
            NextTimeToPlaySound = Time.time + Sound.Refresh;
        }

        for (int i = 0; i < Shooting.ShotCount; i++)
        {
            EmmisionType EmitType = Emmision.Type;
            RaycastHit Hit;
            var dir = Quaternion.Euler(Emmision.RotationOffset) * this.transform.forward;
            Debug.DrawRay(ShootPosition, dir, Color.cyan);

            switch (EmitType)
            {
                case EmmisionType.Line:
                    if (Physics.Raycast(ShootPosition, dir, out Hit, Shooting.Range, ~(1 << LayerMask.NameToLayer("Pieces"))))
                    {
                        Damage(Hit, Shooting, Index);
                    }
                    break;
                case EmmisionType.Tube:
                    if (Physics.SphereCast(ShootPosition, Emmision.EmmisionSize, dir, out Hit, Shooting.Range, ~(1 << LayerMask.NameToLayer("Pieces"))))
                    {
                        Damage(Hit, Shooting, Index);
                    }
                    break;
            }
        }
        NextTimesToFire[Index] = Time.time + (1 / Shooting.FireRate) + Shooting.Recharge;
        if (NextTimesToFire[Index] == Mathf.Infinity || NextTimesToFire[Index] == Mathf.NegativeInfinity)
        {
            NextTimesToFire[Index] = 0;
        }
    }


    /*
    public void Shoot()
    {
        EmmisionMode Emmision = Weapon.Emmision;
        ShotMode Shooting = Weapon.Shooting;
        ParticleMode Particles = Weapon.Particles;
        SoundMode Sound = Weapon.Sound;

        if (Particles != null)
        {
            Vector3 ParticleColor = new Vector3(Particles.Coloring.r, Particles.Coloring.g, Particles.Coloring.b);
            PhotonNetwork.RPC(this.photonView, "PlayParticles", RpcTarget.AllBuffered, false, ParticleColor, Particles.Prefab, Particles.Type, Particles.Position);
        }

        if (Sound != null && Time.time > NextTimeToPlaySound)
        {
            AudioSource Source = this.GetComponent<AudioSource>();
            Source.clip = Sound.Clip;
            Source.volume = Sound.Volume;
            Source.Play();
            NextTimeToPlaySound = Time.time + Sound.Refresh;
        }

        for (int i = 0; i < Shooting.ShotCount; i++)
        {
            EmmisionType EmitType = Emmision.Type;
            RaycastHit Hit;

            switch (EmitType)
            {
                case EmmisionType.Line:
                    if (Physics.Raycast(this.transform.position, this.transform.forward, out Hit, Shooting.Range, ~(1 << LayerMask.NameToLayer("Pieces"))))
                    {
                        Damage(Hit);
                    }
                    break;
                case EmmisionType.Tube:
                    if (Physics.SphereCast(this.transform.position, Emmision.EmmisionSize, this.transform.forward, out Hit, Shooting.Range, ~(1 << LayerMask.NameToLayer("Pieces"))))
                    {
                        Damage(Hit);
                    }
                    break;
            }        
        }

        NextTimeToFire = Time.time + (1 / Shooting.FireRate) + Shooting.Recharge;
    }
    */


    public void Damage(RaycastHit Hit, ShotMode Shooting, int Index)
    {
        if (Hit.transform.GetComponent<Rigidbody>() != null)
        {
            Hit.transform.GetComponent<Rigidbody>().AddForce(-Hit.point * Shooting.Force, ForceMode.Force);
        }

        if (Hit.transform.GetComponent<DestroyableObject>() != null)
        {
            Hit.transform.GetComponent<DestroyableObject>().TakeDamage(Shooting.Damage, Hit.point, this.gameObject);
        }

        if (this.GetComponent<LineRenderer>() != null)
        {
            LineRenderer Line = this.transform.Find("Line: " + Index.ToString()).GetComponent<LineRenderer>();
            SetLinePositions(Line, this.transform.position, Hit.point);
        }
    }
}

/* Gun Types
[System.Serializable]
public class EmmisionMode
{
    public EmmisionType Type;
    public Vector3 Direction;
    public float EmmisionSize;
    public float ShotOffset;
}

[System.Serializable]
public class ShotMode
{
    public float Damage;
    public float Range;
    public float FireRate;
    public float Recharge;
    public float ShotCount;
    public float Penetration;
    public float Recoil;
}

[System.Serializable]
public class ParticleMode
{
    public GameObject Prefab;
    public Color Coloring;
}

[System.Serializable]
public class SoundMode
{
    public AudioClip Clip;
    public float Refresh;
    public float Volume;
}
*/
