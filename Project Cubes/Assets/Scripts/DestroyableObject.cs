using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using EZCameraShake;
using Photon.Pun;
using Photon.Realtime;
using Chronos;

public class DestroyableObject : MonoBehaviourPunCallbacks {

    private Transform partStorage;
    public bool isAlive = true;
    public ExplosionEffect Effect;
    public float Pieces;

    public PhotonView photonView;

    public void Start()
    {
        partStorage = GameObject.Find("TempStorage").transform;
        photonView = PhotonView.Get(this);
    }

    public void Respawn()
    {
        PhotonNetwork.RPC(photonView, "Respawn", RpcTarget.AllBuffered, false, this.gameObject.GetPhotonView());
    }

    [PunRPC] public void respawn(int viewID)
    {
        Transform thisObject = PhotonView.Find(viewID).transform;
        DestroyableObject desObj = thisObject.GetComponent<DestroyableObject>();
        desObj.isAlive = true;
        thisObject.gameObject.SetActive(true);
        thisObject.position = new Vector3(0f, 0.5f, 0f);
    }

    public void GetHit(Vector3 point, GameObject killer)
    {
        if (photonView != null)
        {
            PhotonNetwork.RPC(photonView, "PunBreak", RpcTarget.AllBuffered, false, point, killer.GetPhotonView().ViewID);
        } else
        {
            Break(point);
        }
    }

    public void Break(Vector3 point)
    {
        Explosion(point);
        Die(null);
    }

    [PunRPC] public void PunBreak(Vector3 point, int killerID)
    {
        GameObject killer = PhotonView.Find(killerID).gameObject;
        Explosion(point);
        Die(killer);
    }

    public void Die(GameObject killer)
    {
        if (photonView != null)
        {
            if (photonView.IsMine == true)
            {
                if (this.GetComponent<PlayerCube>() != null)
                {
                    Transform cam = Camera.main.transform;
                    if (cam != null)
                    {
                        cam.transform.SetParent(this.transform.parent);
                        cam.GetComponent<SmoothCameraAdvanced>().enabled = false;
                    }
                }

                isAlive = false;
            }
        } else
        {
            if (this.GetComponent<PlayerCube>() != null)
            {
                Transform cam = Camera.main.transform;
                if (cam != null)
                {
                    cam.transform.SetParent(this.transform.parent);
                    cam.GetComponent<SmoothCameraAdvanced>().enabled = false;
                }
                Invoke("Respawn", 5f);
            }
            isAlive = false;
        }

        if (killer != null)
        {
            if (killer.transform.GetComponent<PlayerCube>())
            {
                killer.transform.GetComponent<PlayerCube>().score += 1f;
            }
        }

        this.gameObject.SetActive(false);
    }


    public void Explosion(Vector3 point)
    {
        if (Effect == ExplosionEffect.Cubeblast)
            ExplosionCubes(point);
        else if (Effect == ExplosionEffect.Polygons)
            ExplosionPolygons(point);
    }

    private GameObject[] fragments;

    public void ExplosionPolygons(Vector3 point)
    {
        
    }

    public GameObject[] GetFragments()
    {
        return fragments;
    }

    public void ExplosionCubes(Vector3 point)
    {
        List<Vector3> LayerPositions = new List<Vector3>();
        /*

        float SeperationZ = PieceSize.z;
        float SeperationX = PieceSize.x;

        List<Vector3> RowPositions = new List<Vector3>();

        for(int i = 0; i < Pieces; i++)
        {
            RowPositions.Add(new Vector3(0, 0, SeperationZ * i));
        }


        for(int i = 0; i < Pieces; i++)
        {
            for (int j = 0; j < RowPositions.Count; j++)
            {
                Vector3 vector = RowPositions[j];
                vector.x = SeperationX * i;
                LayerPositions.Add(vector);
            }
        }
        */

        Vector3 Size = this.transform.localScale;
        Vector3 PieceSize = Size / Pieces;

        for (int i = 0; i < Pieces; i++)
        {
            for (int j = 0; j < Pieces; j++)
            {
                for (int k = 0; k < Pieces; k++)
                {
                    Vector3 PiecePosition = new Vector3((PieceSize.x * j) - PieceSize.x, (PieceSize.y * i) - PieceSize.y, (PieceSize.z * k) - PieceSize.z);
                    LayerPositions.Add(PiecePosition);
                }
            }
        }

        foreach (Vector3 position in LayerPositions)
        {
            GameObject Piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Piece.AddComponent<Rigidbody>();
            Rigidbody Rigid = Piece.transform.GetComponent<Rigidbody>();
            Piece.transform.localScale = PieceSize;
            Piece.transform.rotation = Quaternion.identity;
            if (this.GetComponent<Rigidbody>() != null)
            {
                Rigid.mass = this.GetComponent<Rigidbody>().mass / LayerPositions.Count;
            }
            else
            {
                Rigid.mass = 1000 / LayerPositions.Count;
            }
            Rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Rigid.useGravity = true;

            Timeline t = Piece.AddComponent<Timeline>();
            t.mode = TimelineMode.Global;
            t.globalClockKey = "Root";

            Piece.transform.SetParent(partStorage);
            Piece.transform.position = this.transform.position + position;
            Piece.transform.name = "Piece";

            Piece.GetComponent<Renderer>().sharedMaterial = this.GetComponent<Renderer>().sharedMaterial;
            Piece.GetComponent<Renderer>().sharedMaterial.color = this.GetComponent<Renderer>().sharedMaterial.color;

            Rigid.AddExplosionForce(Rigid.mass * 10f, point, 2f, 0.001f, ForceMode.Force);
            //Rigid.AddForce(-point * (Rigid.mass * 10f), ForceMode.Impulse);
        }     
    }

    public void DisableEditor() {
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.IsWriting) {
			stream.SendNext(isAlive);

		}
		else 
		{
			this.isAlive = (bool)stream.ReceiveNext();
		}
	}
}

public enum ExplosionEffect { Cubeblast, Polygons }
