using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EZCameraShake;
using Photon.Pun;
using Photon.Realtime;

public class ShootShots : MonoBehaviourPunCallbacks {
    private float NextTimeToFire;
    private float NextTimeToPlaySound;
    private WeaponItem Weapon;
    private PlayerCube Cube;

    private void Awake()
    {
        Cube = this.GetComponent<PlayerCube>();
        Weapon = Cube.weapon;
    }

    public void Update()
    {
        if (Input.GetMouseButton(0) && Time.time > NextTimeToFire)
        {
            Shoot();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (Weapon.Particles.Type == "KvantSprayMV")
            {
                this.transform.Find("Particles").GetComponent<Kvant.SprayMV>().throttle = 0;
            } else
            {
                this.transform.Find("Particles").gameObject.SetActive(false);
            }
        }
    }

    [PunRPC] public void PlayParticles(Vector3 ParticlesColor, string ParticlesName, string ParticlesType, Vector3 ParticlesPosition)
    {
        GameObject ParticleObject = null;
        Color Coloring = new Color(ParticlesColor.x, ParticlesColor.y, ParticlesColor.z);

        if (ParticlesType == "Linerenderer")
        {
            //If particles are supposed to be a linerenderer
        }
        else
        {
            if (!this.transform.Find("Particles"))
            {
                ParticleObject = PhotonNetwork.Instantiate("Particles/" + ParticlesName, this.transform.position, this.transform.rotation, 0);
                ParticleObject.transform.SetParent(this.transform);
                ParticleObject.transform.localPosition = ParticlesPosition;
                ParticleObject.transform.name = "Particles";

                if (ParticlesType == "ParticleSystem")
                {
                    var Main = ParticleObject.GetComponent<ParticleSystem>().main;
                    Main.startColor = Coloring;
                }

                if (ParticlesType == "KvantSprayMV")
                {
                    //Change color... SOMEHOW???
                }

                if (ParticlesType == "KvantSwarmMV")
                {
                    //Unknown right now.
                }
            }
            else
            {
                ParticleObject = this.transform.Find("Particles").gameObject;
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
                        ParticleObject.GetComponent<Kvant.SwarmMV>().attractor = hit.transform;
                    } 
                }

            }
        }
    }


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

    public void Damage(RaycastHit Hit)
    {
        if (Hit.transform.GetComponent<Rigidbody>() != null)
        {
            Hit.transform.GetComponent<Rigidbody>().AddForce(-Hit.point * Weapon.Shooting.Force, ForceMode.Force);
        }

        if (Hit.transform.GetComponent<DestroyableObject>() != null)
        {
            Hit.transform.GetComponent<DestroyableObject>().TakeDamage(Weapon.Shooting.Damage, Hit.point, this.gameObject);
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
