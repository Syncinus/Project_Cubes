using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;
using Chronos;

public class PlayerCube : MonoBehaviourPunCallbacks {
	public Rigidbody rigid;
	public float smooth = 1.0f;
	public float speed = 5.0f;
	public float turnSpeed = 0.05f;
	public List<float> rotationPoints = new List<float> { 0, 45, 90, 135, 180, 225, 270, 315 };
	public WeaponItem weapon;
    public bool ableToMove = true;

	private DestroyableObject desObj;
	public float timeScinceLastTimeTakingDamage = 0f;
	public bool moving = false;
	public bool rotation = false;

    public Color EnergyColor;
    private Color LastEnergyColor;
    private List<Transform> Energy = new List<Transform>();

    private List<RealityManipulator> manipulators = new List<RealityManipulator>();

	private bool setCamera = false;
    
    public Timeline time
    {
        get
        {
            return this.transform.GetComponent<Timeline>();
        }
    }

    public void Start() {
		if (photonView.IsMine != true) {
			return;
		}

        PhotonNetwork.RPC(photonView, "ChangeEnergyColor", RpcTarget.AllBuffered, false, new Vector3(EnergyColor.r, EnergyColor.g, EnergyColor.b), this.gameObject.GetPhotonView().ViewID);
        LastEnergyColor = EnergyColor;

        //jobsTransform.Add(this.transform);        
        
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().target = this.transform;
        Camera.main.transform.SetParent(this.transform);
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = true;

        //Camera.main.transform.GetComponent<AlternateCameraScript>().target = this.transform;

		this.transform.position = new Vector3(0f, 0.5f, 0f);
		rigid = this.GetComponent<Rigidbody> ();
		desObj = this.GetComponent<DestroyableObject> ();
        if (CubeSettings.weapon != null)
        {
            weapon = CubeSettings.weapon;
        }
        if (CubeSettings.manipulators.Count > 0)
        {
            manipulators = CubeSettings.manipulators;
        }
    }

    public void AddScore(float score)
    {
        ScoreSystem.Score += score;
        CubeSettings.Score += score;
    }

    public void Update()
    {
        if (photonView.IsMine != true)
        {
            return;
        }

        if (EnergyColor != LastEnergyColor)
        {
            PhotonNetwork.RPC(photonView, "ChangeEnergyColor", RpcTarget.AllBuffered, false, new Vector3(EnergyColor.r, EnergyColor.g, EnergyColor.b), this.gameObject.GetPhotonView().ViewID);
            LastEnergyColor = EnergyColor;
        }
	}

    [PunRPC] public void ChangeEnergyColor(Vector3 color, int viewID)
    {
        Color e = new Color(color.x, color.y, color.z);
        List<Transform> energyTransforms = new List<Transform>();
        GameObject thisObject = PhotonView.Find(viewID).gameObject;
        foreach (Transform child in thisObject.transform.Find("Model"))
        {
            if (child.name == "Energy")
            {
                energyTransforms.Add(child);
            }
        }

        foreach (Transform energy in energyTransforms)
        {
            energy.GetComponent<Renderer>().sharedMaterial.color = EnergyColor;
            energy.GetComponent<Light>().color = EnergyColor;
        }
    }

    public float figureDifference(float start, float end) {
		return end - start;
	}

    public enum MovementDirection { Forward, Backward, Left, Right, Up };


    public void Move(MovementDirection direction)
    {
        float dt = time.fixedDeltaTime;

        if (ableToMove == true)
        {
            if (direction == MovementDirection.Forward)
            {
                this.transform.Translate(Vector3.forward * speed * dt, Space.Self);
            }
            if (direction == MovementDirection.Backward)
            {
                this.transform.Translate(Vector3.back * speed * dt, Space.Self);
            }
        }

        if (direction == MovementDirection.Left)
        {
            this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x, (transform.rotation.eulerAngles.y + 75f * dt * Input.GetAxis("Horizontal")), transform.rotation.eulerAngles.z);
        }

        if (direction == MovementDirection.Right)
        {
            this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x, (transform.rotation.eulerAngles.y + 75f * dt * Input.GetAxis("Horizontal")), transform.rotation.eulerAngles.z);
        }

        if (direction == MovementDirection.Up)
        {
            time.rigidbody.AddForce(new Vector3(0, 500, 0), ForceMode.Acceleration);
        }
    }

    public void FixedUpdate() {
        if (photonView.IsMine != true) {
            return;
		}

        if (timeScinceLastTimeTakingDamage < 6f) {
			timeScinceLastTimeTakingDamage += Time.deltaTime;
		}

		moving = false;
		//transform.rotation = Quaternion.Euler (this.transform.rotation.eulerAngles.x, (transform.rotation.eulerAngles.y + 100f * Time.deltaTime * Input.GetAxis ("Horizontal")), transform.rotation.eulerAngles.z);
		if (Input.GetKey (KeyCode.W)) {
			Move(MovementDirection.Forward);
		}
		if (Input.GetKey (KeyCode.S)) {
			Move(MovementDirection.Backward);
		}
		if (Input.GetKey(KeyCode.A))
		{
			Move(MovementDirection.Left);
		}
		if (Input.GetKey(KeyCode.D))
		{
			Move(MovementDirection.Right);
		}
        if(Input.GetKeyDown(KeyCode.E))
        {
            Move(MovementDirection.Up);
        }

		if (moving == false) {
			if (Input.GetKeyDown (KeyCode.X)) {
				float currentRotation = this.transform.rotation.eulerAngles.y;
				float closest = rotationPoints.Aggregate ((x, y) => Mathf.Abs (x - currentRotation) < Mathf.Abs (y - currentRotation) ? x : y);
				float valueChange = figureDifference (currentRotation, closest);
				//Debug.Log (addOnValue.ToString ());
				StartCoroutine (Rotate (Vector3.up, 45 + valueChange, 0.6f));
			}

			if (Input.GetKeyDown (KeyCode.Z)) {
				float currentRotation = this.transform.rotation.eulerAngles.y;
				float closest = rotationPoints.Aggregate ((x, y) => Mathf.Abs (x - currentRotation) < Mathf.Abs (y - currentRotation) ? x : y);
				float valueChange = figureDifference (closest, currentRotation);

				StartCoroutine (Rotate (Vector3.down, 45 + valueChange, 0.6f));
			}
		}
	}




		

	IEnumerator Rotate(Vector3 axis, float angle, float duration) {
		Quaternion from = transform.rotation;
		Quaternion to = transform.rotation;
		to *= Quaternion.Euler (axis * angle);
		moving = true;
		float elapsed = 0.0f;
		while (elapsed < duration) {
			transform.rotation = Quaternion.Slerp (from, to, elapsed / duration);
			elapsed += Time.deltaTime;
			yield return null;
		}
		moving = false;
		transform.rotation = to;
	}

	/*
	public static Vector4 ToVector4 (this Quaternion quaternion) {
		return new Vector4 (quaternion.x, quaternion.y, quaternion.z, quaternion.w);
	}

	public static Quaternion ToQuaternion (this Vector4 vector) {
		return new Quaternion (vector.x, vector.y, vector.z, vector.w);
	}

	public static Vector4 SmoothDamp (Vector4 current, Vector4 target, ref Vector4 currentVelocity, float smoothTime) {
		float x = Mathf.SmoothDamp (current.x, target.x, ref currentVelocity.x, smoothTime);
		float y = Mathf.SmoothDamp (current.y, target.y, ref currentVelocity.y, smoothTime);
		float z = Mathf.SmoothDamp (current.z, target.z, ref currentVelocity.z, smoothTime);
		float w = Mathf.SmoothDamp (current.w, target.w, ref currentVelocity.w, smoothTime);

		return new Vector4 (x, y, z, w);
	}

	public static Quaternion SmoothDamp (Quaternion current, Quaternion target, ref Vector4 currentVelocity, float smoothTime) {
		Vector4 smooth = SmoothDamp (ToVector4 (current), ToVector4 (target), ref currentVelocity, smoothTime);
		return ToQuaternion (smooth);
	}
	*/
}
