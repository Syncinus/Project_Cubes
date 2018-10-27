using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TimeManager : MonoBehaviourPunCallbacks {

    public float TimeLimit = 600f;
    public float CurrentTime = 0f;
    public Text TimeText;

    public void Start()
    {
        this.gameObject.AddComponent<PhotonView>();
    }

    public void FixedUpdate()
    {
        CurrentTime += Time.deltaTime;
        TimeText.text = "Current Time: " + Mathf.FloorToInt(CurrentTime).ToString() + " Seconds";
        if (CurrentTime >= TimeLimit)
        {
            PhotonNetwork.RPC(this.photonView, "EndGame", RpcTarget.All, false);
        }
    }

    [PunRPC] public void EndGame()
    {

    }
}
