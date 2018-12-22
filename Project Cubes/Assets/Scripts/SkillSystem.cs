using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Chronos;

//This controls time
public class SkillSystem : MonoBehaviourPunCallbacks {
    public RealityManipulator Manipulator;

    public void Awake()
    {
        if (CubeSettings.manipulator != null)
        {
            Manipulator = CubeSettings.manipulator;
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            PhotonNetwork.RPC(this.photonView, "Effect", RpcTarget.AllBuffered, false, this.gameObject.GetPhotonView().ViewID);
        }
    }

    [PunRPC] public void Effect(int viewID)
    {
        GameObject player = PhotonView.Find(viewID).gameObject;
        SkillSystem skills = player.GetComponent<SkillSystem>();
        RealityManipulator manipulator = skills.Manipulator;
        Transform target = null;
        bool projectToPosition = false;

        Vector3 position = Vector3.zero;

        switch (manipulator.follow)
        {
            case FollowMode.Stay:
                target = null;
                break;
            case FollowMode.FollowShooter:
                target = this.transform;
                break;
            case FollowMode.FollowEmmisionHit:
                //This is dealt with in the emmision switch
                break;
        }

        switch (manipulator.emmision)
        {
            case ManipulatorEmmision.Position:
                position = player.transform.position;
                break;
            case ManipulatorEmmision.Projected:
                projectToPosition = true;
                RaycastHit Hit;
                LayerMask ignore = ~(1 << LayerMask.NameToLayer("Bullets") | 1 << LayerMask.NameToLayer("Pieces"));
                position = this.transform.position + (this.transform.forward * 30f);
                if (Physics.Raycast(player.transform.position, player.transform.forward, out Hit, 30f, ignore))
                {
                    position = Hit.transform.position;
                    if (manipulator.follow == FollowMode.FollowEmmisionHit)
                    {
                        target = Hit.transform;
                    }
                }         
                break;
        }

        GameObject obj = null;

        switch (manipulator.area)
        {
            case AreaType.Sphere:
                obj = PhotonNetwork.Instantiate("Primatives/Sphere", position, Quaternion.Euler(Vector3.zero));
                break;
            case AreaType.Cube:
                obj = PhotonNetwork.Instantiate("Primatives/Cube", position, Quaternion.Euler(Vector3.zero));
                break;
        }

        if (obj != null)
        {
            if (projectToPosition == true)
            {
                MoveToPosition(obj.transform, position, 0.5f);
            }

            if (target != null)
            {
                SmoothFollow smooth = obj.gameObject.AddComponent<SmoothFollow>();
                smooth.target = target;
            }

            obj.layer = LayerMask.NameToLayer("Manipulators");
            obj.transform.name = "Reality Manipulator Field";
            obj.transform.SetParent(GameObject.Find("TempStorage").transform);

            Renderer renderer = obj.GetComponent<Renderer>();
            Material mat = renderer.material;

            float emmision = Mathf.PingPong(Time.time, 1.0f);
            Color baseColor = manipulator.color;

            Color finalColor = baseColor * Mathf.LinearToGammaSpace(emmision);

            mat.SetColor("_EmmisionColor", finalColor);

            Expand(obj.transform, new Vector3(manipulator.radius, manipulator.radius, manipulator.radius), 0.75f);
            Collider c = obj.transform.GetComponent<Collider>();
            c.isTrigger = true;
            AreaClock3D field = obj.AddComponent<AreaClock3D>();
            field.localTimeScale = manipulator.speed;
            field.innerBlend = manipulator.blend;
            field.mode = manipulator.mode;
        }
    }

    public void Expand(Transform trans, Vector3 size, float time)
    {
        StartCoroutine(expand(trans, size, time));
    }

    public void MoveToPosition(Transform trans, Vector3 position, float time)
    {
        StartCoroutine(moveToPosition(trans, position, time));
    }

    public IEnumerator moveToPosition(Transform trans, Vector3 position, float time)
    {
        Vector3 startPosition = trans.position;
        Vector3 endPosition = position;
        Vector3 currentPosition = startPosition;

        float timer = 0f;

        do
        {
            currentPosition = Vector3.Lerp(startPosition, endPosition, timer / time);
            trans.position = currentPosition;
            timer += Time.deltaTime;
            yield return null;
        } while (timer < time);
    }

    public IEnumerator expand(Transform trans, Vector3 size, float time)
    {
        Vector3 startSize = trans.localScale;
        Vector3 endSize = size;
        Vector3 currentSize = startSize;

        float timer = 0f;

        do
        {
            currentSize = Vector3.Lerp(startSize, endSize, timer / time);
            trans.localScale = currentSize;
            timer += Time.deltaTime;
            yield return null;
        } while (timer < time);
    }

    /*
    public void Effect(RealityManipulator m)
    {
        GameObject obj = PhotonNetwork.Instantiate("Primatives/" + m.prefab, this.transform.position, this.transform.rotation);
        obj.layer = LayerMask.NameToLayer("Manipulators");
        obj.transform.name = "Reality Manipulation Field";

        Renderer renderer = obj.GetComponent<Renderer>();
        Material mat = renderer.material;

        float emission = Mathf.PingPong(Time.time, 1.0f);
        Color baseColor = m.color;

        Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);

        mat.SetColor("_EmissionColor", finalColor);

        obj.transform.localScale = new Vector3(m.radius, m.radius, m.radius);
        Collider c = obj.transform.GetComponent<Collider>();
        c.isTrigger = true;
        AreaClock3D field = obj.AddComponent<AreaClock3D>();
        field.localTimeScale = m.speed;
        field.innerBlend = m.blend;
        field.mode = m.mode;
    }
    */
}
