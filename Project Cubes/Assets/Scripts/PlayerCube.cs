using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using EZCameraShake;


public class PlayerCube : MonoBehaviourPunCallbacks {

	public Rigidbody rigid;
	public float smooth = 1.0f;
	public float speed = 5.0f;
	public float turnSpeed = 0.05f;
	public List<float> rotationPoints = new List<float> { 0, 45, 90, 135, 180, 225, 270, 315 };
	public WeaponItem weapon;
    public bool ableToMove = true;

	private DestroyableObject desObj;
	private Vector3 velocity;
	public float timeScinceLastTimeTakingDamage = 0f;
	public bool moving = false;
	public bool rotation = false;
	public GameObject proxyCube;
    public Vector3 realPosition;

	private bool setCamera = false;
	private Text healthText;

    private EquipmentManager gunManager;

    bool first = true;


    public void Start() {
		if (photonView.IsMine != true) {
			return;
		}

        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().target = this.transform;
        Camera.main.transform.SetParent(this.transform);
        Camera.main.transform.GetComponent<SmoothCameraAdvanced>().enabled = true;
       // Camera.main.transform.GetComponent<AlternateCameraScript>().target = this.transform;

		this.transform.position = new Vector3(0f, 0.5f, 0f);
		rigid = this.GetComponent<Rigidbody> ();
		desObj = this.GetComponent<DestroyableObject> ();
		GameObject canvas = GameObject.Find("Canvas");
		healthText = canvas.transform.Find("Health").GetComponent<Text>();
		StartCoroutine (Regenerate ());
        gunManager = EquipmentManager.instance;
        weapon = gunManager.currentGun;
    }

    public void Update()
    {
        if (photonView.IsMine != true)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            this.GetComponent<DestroyableObject>().TakeDamage(30000, this.transform.position);
        }
        if (weapon != gunManager.currentGun)
        {
            weapon = gunManager.currentGun;
        }
	}


	public float figureDifference(float start, float end) {
		return end - start;
	}

	IEnumerator Regenerate() {
		while (true) {
			if (desObj.health < desObj.maxHealth && timeScinceLastTimeTakingDamage >= 5f) {
				desObj.health += 750;
				yield return new WaitForSeconds (1f);
			} else {
				yield return null;
			}
		}
	}

    public void Move(string way)
    {
        if (ableToMove == true)
        {
            if (way == "Forward")
            {
                this.transform.Translate(Vector3.forward * speed * Time.deltaTime / Time.timeScale, Space.Self);
            }
            if (way == "Backward")
            {
                this.transform.Translate(Vector3.back * speed * Time.deltaTime / Time.timeScale, Space.Self);
            }
        }
        if (way == "Left")
        {
            this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x, (transform.rotation.eulerAngles.y + 75f * Time.fixedDeltaTime / Time.timeScale * Input.GetAxis("Horizontal")), transform.rotation.eulerAngles.z);
        }
        if (way == "Right")
        {
            this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x, (transform.rotation.eulerAngles.y + 75f * Time.fixedDeltaTime / Time.timeScale * Input.GetAxis("Horizontal")), transform.rotation.eulerAngles.z);
        }
        if (way == "Up")
        {
            this.GetComponent<Rigidbody>().AddForce(new Vector3(0, 5, 0), ForceMode.VelocityChange);
            //this.GetComponent<Rigidbody>().velocity += new Vector3(0, 10, 0);
        }

    }

	public void FixedUpdate() {

        /*
        Plane playerPlane = new Plane(Vector3.up, transform.position);

        // Generate a ray from the cursor position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Determine the point where the cursor ray intersects the plane.
        // This will be the point that the object must look towards to be looking at the mouse.
        // Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
        //   then find the point along that ray that meets that distance.  This will be the point
        //   to look at.
        float hitdist = 0.0f;
        // If the ray is parallel to the plane, Raycast will return false.
        if (playerPlane.Raycast(ray, out hitdist))
        {
            // Get the point along the ray that hits the calculated distance.
            Vector3 targetPoint = ray.GetPoint(hitdist);

            // Determine the target rotation.  This is the rotation if the transform looks at the target point.
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);

            // Smoothly rotate towards the target point.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2.0f * Time.deltaTime);
        }
        */

        //foreach (Transform armor in this.transform.Find("ArmorHolder").transform)
        //{
        //armor.transform.localPosition = new Vector3 (0f, 0f, 0f);
        //}

        //if (Camera.main.GetComponent<CameraShaker>().enabled != true)
        //{
        //    this.transform.Find("ShakePosition").position = Camera.main.transform.position;
        //}

        if (photonView.IsMine != true) {
            return;
		}

		healthText.text = "Health: " + this.GetComponent<DestroyableObject>().health;


		if (desObj.health > desObj.maxHealth) {
			desObj.health = desObj.maxHealth;
		}
		if (timeScinceLastTimeTakingDamage < 6f) {
			timeScinceLastTimeTakingDamage += Time.deltaTime;
		}

		moving = false;
		//transform.rotation = Quaternion.Euler (this.transform.rotation.eulerAngles.x, (transform.rotation.eulerAngles.y + 100f * Time.deltaTime * Input.GetAxis ("Horizontal")), transform.rotation.eulerAngles.z);
		if (Input.GetKey (KeyCode.W)) {
			Move("Forward");
		}
		if (Input.GetKey (KeyCode.S)) {
			Move("Backward");
		}
		if (Input.GetKey(KeyCode.A))
		{
			Move("Left");
		}
		if (Input.GetKey(KeyCode.D))
		{
			Move("Right");
		}
        if (Input.GetKeyDown(KeyCode.E))
        {
            Move("Up");
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
