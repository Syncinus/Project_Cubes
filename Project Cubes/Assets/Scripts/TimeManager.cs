using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviourPunCallbacks {

    public float TimeLimit = 600f;
    public float CurrentTime = 0f;
    public Text TimeText;
    public Text ControlText;

    bool gameEnded = false;

    public void Start()
    {
        this.gameObject.AddComponent<PhotonView>();
    }

    public void FixedUpdate()
    {
        CurrentTime += Time.deltaTime;
        TimeText.text = "Current Time: " + Mathf.FloorToInt(CurrentTime).ToString() + " Seconds";
        if (CurrentTime >= TimeLimit && gameEnded == false)
        {
            gameEnded = true;
            List<int> ids = new List<int>();
            var players = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "PlayerCube(Clone)");
            foreach (GameObject player in players)
            {
                ids.Add(player.GetPhotonView().ViewID);
            }
            PhotonNetwork.RPC(this.photonView, "EndGame", RpcTarget.All, false, ids.ToArray());
        }
    }

    [PunRPC] public void EndGame(int[] ids)
    {
        List<float> scores = new List<float>();
        List<GameObject> players = new List<GameObject>();

        foreach (int id in ids)
        {
            GameObject obj = PhotonView.Find(id).gameObject;
            players.Add(obj);
            PlayerCube p = obj.GetComponent<PlayerCube>();
            scores.Add(p.score);
        }

        var max = Mathf.Max(scores.ToArray());

        foreach (GameObject p in players)
        {
            if (p.GetComponent<PlayerCube>().score == max)
            {
                ControlText.text = "The Winner Is: " + p.transform.name;
            }
        }

        StartCoroutine(FinishGame());

    }

    public IEnumerator FinishGame()
    {
        yield return new WaitForSeconds(3.0f);
        ControlText.text = "Ending Game";
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        SceneManager.LoadScene("Menu");
    }
}

