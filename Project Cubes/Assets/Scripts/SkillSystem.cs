using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using System;
using System.Linq;
using UnityEngine;
using EZCameraShake;
using Photon.Pun;
using Photon.Realtime;


public class SkillSystem : MonoBehaviourPunCallbacks {
	public bool superChargeModeReady = true;
	public float superchargeRecharge = 25f;
	public bool shockwaveReady = true;
	public float shockwaveRecharge = 10f;
	public float shockwaveDamage = 5000f;
    private GameObject cameraHolderForShaking;
	public ParticleSystem shockwaveParticles;

    Kino.PostProcessing.Isoline line;

    public void Start()
    {
        cameraHolderForShaking = this.GetComponent<ShootShots>().cameraHolderForShaking;
    }

    public void FixedUpdate() {
        if (photonView.IsMine != true)
        {
            return;
        }
		if (Input.GetKeyDown (KeyCode.F1)) {
			BeamSuperchargeMode ();
		}
		if (Input.GetKeyDown (KeyCode.F2)) {
			Shockwave ();
		}
	}

    public IEnumerator ShakeCamera(float magnitude, float roughness, float startFadeIn, float endFadeOut)
    {
        Vector3 camPosition = Camera.main.transform.localPosition;
        Quaternion camRotation = Camera.main.transform.localRotation;
        cameraHolderForShaking.transform.localPosition = camPosition;
        cameraHolderForShaking.transform.localRotation = camRotation;
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = false;
        Camera.main.transform.GetComponent<CameraShaker>().enabled = true;
        Camera.main.transform.SetParent(cameraHolderForShaking.transform);
        CameraShaker.Instance.ShakeOnce(magnitude, roughness, startFadeIn, endFadeOut);
        yield return new WaitForSeconds(endFadeOut + 0.1f);
        Camera.main.transform.GetComponent<CameraShaker>().enabled = false;
        Camera.main.transform.SetParent(this.transform);
        Camera.main.transform.localPosition = camPosition;
        Camera.main.transform.localRotation = camRotation;
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = true;
    }



	public void Shockwave() {
        if (photonView.IsMine != true)
        {
            return;
        }
        StartCoroutine(shockwave());
	}

    IEnumerator shockwave()
    {
        if (shockwaveReady == true)
        {
            //shockwaveParticles.Play ();

            PostProcessVolume volume = Camera.main.GetComponent<PostProcessVolume>();
            PostProcessProfile profile = volume.profile;
            profile.TryGetSettings(out line);

            line.enabled.value = true;
            line.active = true;

            yield return new WaitForSeconds(2.9f);

            line.enabled.value = false;
            line.active = false;

            yield return new WaitForSeconds(0.3f);

            //shakeCamera(4f, 4f, 0.1f, 2.0f);
            RaycastHit[] hits = Physics.SphereCastAll(this.transform.position, 100f, Vector3.zero, 100f);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.GetComponent<PlayerCube>() == null)
                {
                    DestroyableObject des = hit.transform.GetComponent<DestroyableObject>();
                    if (des != null)
                    {
                        des.TakeDamage(shockwaveDamage, -hit.point);
                        Rigidbody rigidb = hit.transform.GetComponent<Rigidbody>();
                        if (rigidb != null)
                        {
                            rigidb.AddForce(-hit.normal * 500f);
                        }
                    }
                }
            }
            shockwaveReady = false;
            Invoke("RechargeShockwave", shockwaveRecharge);
        }
    }

    public void shakeCamera(float magnitude, float roughness, float startFadeIn, float endFadeOut)
    {
        StartCoroutine(ShakeCamera(magnitude, roughness, startFadeIn, endFadeOut));
    }

	public void BeamSuperchargeMode() {
        if (photonView.IsMine != true)
        {
            return;
        }
        if (this.GetComponent<ShootShots> () != null && superChargeModeReady == true) {
			this.GetComponent<ShootShots> ().beamHyperMode = true;
			superChargeModeReady = false;
			Invoke ("disableBeamSupercharge", 7.5f);
		}
	}

	public void RechargeShockwave() {
		shockwaveReady = true;
	}

	public void disableBeamSupercharge() {
		this.GetComponent<ShootShots> ().beamHyperMode = false;
		Invoke ("RechargeBeamSupercharge", superchargeRecharge);
	}

	public void RechargeBeamSupercharge() {
		superChargeModeReady = true;
	}

}
