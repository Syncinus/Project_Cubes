using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotonManager : Photon.MonoBehaviour {

    [SerializeField] private Text connect;
    [SerializeField] private GameObject player;
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private GameObject lobbyCamera;

	// Use this for initialization
	private void Start () {
		PhotonNetwork.ConnectUsingSettings("Project: Cubes");
	}

	public virtual void OnJoinedLobby() {
		Debug.Log("Joined Lobby");

		PhotonNetwork.JoinOrCreateRoom("New", null, null);
	}

	public virtual void OnJoinedRoom() {
		PhotonNetwork.Instantiate(player.name, spawnPoint.position, spawnPoint.rotation, 0);
        AstarPath.active.Scan();
	}

	// Update is called once per frame
	private void Update () {
		connect.text = PhotonNetwork.connectionStateDetailed.ToString();
	}
}
