using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Chronos;

//This controls reality
public class SkillSystem : MonoBehaviourPunCallbacks {

    public RealityManipulator test;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PhotonNetwork.RPC(this.photonView, "Effect", RpcTarget.AllBuffered, false, this.gameObject.GetPhotonView().ViewID);
        }
    }

    [PunRPC] public void Effect(int viewID)
    {
        GameObject player = PhotonView.Find(viewID).gameObject;
        SkillSystem skills = player.GetComponent<SkillSystem>();
        RealityManipulator manipulator = skills.test;

        Vector3 position = Vector3.zero;

        switch (manipulator.emmision)
        {
            case ManipulatorEmmision.Position:
                position = player.transform.position;
                break;
            case ManipulatorEmmision.Projected:
                RaycastHit Hit;
                LayerMask ignore = 1 << LayerMask.NameToLayer("Bullets") | 1 << LayerMask.NameToLayer("Pieces");
                if (Physics.Raycast(player.transform.position, player.transform.forward, out Hit, 30f, ignore))
                {
                    position = Hit.transform.position;
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
            obj.layer = LayerMask.NameToLayer("Manipulators");
            obj.transform.name = "Reality Manipulator Field";
            obj.transform.SetParent(GameObject.Find("TempStorage").transform);

            Renderer renderer = obj.GetComponent<Renderer>();
            Material mat = renderer.material;

            float emmision = Mathf.PingPong(Time.time, 1.0f);
            Color baseColor = manipulator.color;

            Color finalColor = baseColor * Mathf.LinearToGammaSpace(emmision);

            mat.SetColor("_EmmisionColor", finalColor);

            obj.transform.localScale = new Vector3(manipulator.radius, manipulator.radius, manipulator.radius);
            Collider c = obj.transform.GetComponent<Collider>();
            c.isTrigger = true;
            AreaClock3D field = obj.AddComponent<AreaClock3D>();
            field.localTimeScale = manipulator.speed;
            field.innerBlend = manipulator.blend;
            field.mode = manipulator.mode;
        }
    }

    public IEnumerator effect(int viewID)
    {
        GameObject player = PhotonView.Find(viewID).gameObject;
        SkillSystem skills = player.GetComponent<SkillSystem>();
        RealityManipulator manipulator = skills.test;

        Vector3 position = Vector3.zero;

        switch (manipulator.emmision)
        {
            case ManipulatorEmmision.Position:
                position = player.transform.position;
                break;
            case ManipulatorEmmision.Projected:
                RaycastHit Hit;
                LayerMask ignore = 1 << LayerMask.NameToLayer("Bullets") | 1 << LayerMask.NameToLayer("Pieces");
                if (Physics.Raycast(player.transform.position, player.transform.forward, out Hit, 30f, ignore))
                {
                    position = Hit.transform.position;
                }
                break;
        }

        yield return player.transform.GetComponent<Timeline>().WaitForSeconds(manipulator.delay);

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
            obj.layer = LayerMask.NameToLayer("Manipulators");
            obj.transform.name = "Reality Manipulator Field";
            obj.transform.SetParent(GameObject.Find("TempStorage").transform);

            Renderer renderer = obj.GetComponent<Renderer>();
            Material mat = renderer.material;

            float emmision = Mathf.PingPong(Time.time, 1.0f);
            Color baseColor = manipulator.color;

            Color finalColor = baseColor * Mathf.LinearToGammaSpace(emmision);

            mat.SetColor("_EmmisionColor", finalColor);

            //obj.transform.localScale = new Vector3(manipulator.radius, manipulator.radius, manipulator.radius);
            Expand(obj.transform, new Vector3(manipulator.radius, manipulator.radius, manipulator.radius), 1.0f);
            Collider c = obj.transform.GetComponent<Collider>();
            c.isTrigger = true;
            AreaClock3D field = obj.AddComponent<AreaClock3D>();
            field.localTimeScale = manipulator.speed;
            field.innerBlend = manipulator.blend;
            field.mode = manipulator.mode;
        }

        yield return player.transform.GetComponent<Timeline>().WaitForSeconds(manipulator.delay);
    }

    public void Expand(Transform trans, Vector3 size, float time)
    {
        StartCoroutine(expand(trans, size, time));
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
