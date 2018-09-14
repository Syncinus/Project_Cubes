using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using EZCameraShake;

public class SkillSystem : Photon.MonoBehaviour {
	public bool superChargeModeReady = true;
	public float superchargeRecharge = 25f;
	public bool shockwaveReady = true;
	public float shockwaveRecharge = 10f;
	public float shockwaveDamage = 5000f;
    private GameObject cameraHolderForShaking;
	public ParticleSystem shockwaveParticles;

    public void Start()
    {
        cameraHolderForShaking = this.GetComponent<ShootShots>().cameraHolderForShaking;
    }

    public void FixedUpdate() {
        if (photonView.isMine != true)
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
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = false;
        Camera.main.transform.GetComponent<CameraShaker>().enabled = true;
        Vector3 camPosition = Camera.main.transform.localPosition;
        Quaternion camRotation = Camera.main.transform.localRotation;
        cameraHolderForShaking.transform.localPosition = camPosition;
        cameraHolderForShaking.transform.localRotation = camRotation;
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
        if (photonView.isMine != true)
        {
            return;
        }
        if (shockwaveReady == true) {
			shockwaveParticles.Play ();
            StartCoroutine(ShakeCamera(4f, 4f, 0.1f, 2.0f));
			RaycastHit[] hits = Physics.SphereCastAll (this.transform.position, 10f, this.transform.position, 10f);

			foreach (RaycastHit hit in hits) {
				if (hit.transform.GetComponent<PlayerCube> () == null) {
					DestroyableObject des = hit.transform.GetComponent<DestroyableObject> ();
					if (des != null) {
						des.TakeDamage (shockwaveDamage, -hit.point);
						Rigidbody rigidb = hit.transform.GetComponent<Rigidbody> ();
						if (rigidb != null) {
							rigidb.AddForce (-hit.normal * 500f);
						}
					}
				}
			}
			shockwaveReady = false;
			Invoke ("RechargeShockwave", shockwaveRecharge);
		}
	}

	public void BeamSuperchargeMode() {
        if (photonView.isMine != true)
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
