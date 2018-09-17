using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class EnemySpawner : MonoBehaviourPunCallbacks {
    public static EnemySpawner instance;

    void Awake() {
        instance = this;
    }

    public enum EnemyType { Blue, Green, Yellow, Orange, Black };

    public Wave[] waves;

    /* 
	public EnemyAI enemy;
	public EnemyAI greenEnemy;
	public EnemyAI yellowEnemy;
	public EnemyAI orangeEnemy; 
	public EnemyAI blackEnemy;


	public GameObject enemyp;
	public GameObject greenEnemyp;
	public GameObject yellowEnemyp;
	public GameObject orangeEnemyp;
	public GameObject blackEnemyp;
	*/


    GameMapGenerator map;

    PlayerCube player;
    Transform playerT;
    PlayerCube[] players;

    Wave currentWave;
    int currentWaveNumber;

    public List<WaveEnemies> enemyTypesToSpawn = new List<WaveEnemies>();
    public List<GameObject> enemyObjects = new List<GameObject>();

    int remainingLivingEnemies;
    int remainingEnemiesToSpawn;
    float nextSpawnTime;
    bool gameStarted = false;

    public GameObject spawnParticles;
    public GameObject enemy;
    private bool foundPlayer = false;

    public void Start() {
        if (PhotonNetwork.IsMasterClient != true)
        {
            return;
        }
        if (FindObjectOfType<PlayerCube>() != null)
        {
            map = FindObjectOfType<GameMapGenerator>();
            player = FindObjectOfType<PlayerCube>();
            playerT = player.transform;

            //enemyp = enemy.gameObject;
            //greenEnemyp = greenEnemy.gameObject;
            //yellowEnemyp = yellowEnemy.gameObject;
            //orangeEnemyp = orangeEnemy.gameObject;
            //blackEnemyp = blackEnemy.gameObject;

            //if (gameStarted == false)
            //{
            photonView.RPC("NextWave", RpcTarget.AllBuffered, "STARTING ENEMY WAVES!");
            //    gameStarted = true;
            //}
        }
    }

    public void FixedUpdate() {
        if (PhotonNetwork.IsMasterClient != true)
        {
            return;
        }
        if (foundPlayer == false)
        {
            var plr = FindObjectOfType<PlayerCube>();
            if (plr != null)
            {
                map = FindObjectOfType<GameMapGenerator>();
                player = FindObjectOfType<PlayerCube>();
                playerT = player.transform;

                photonView.RPC("NextWave", RpcTarget.AllBuffered, "STARTING ENEMY WAVES!");
                foundPlayer = true;
            }
        }
        if (remainingEnemiesToSpawn > 0 && Time.time > nextSpawnTime) {
            remainingEnemiesToSpawn--;
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

            if (enemyObjects.Count > 0)
            {
                GameObject enemyToSpawn = enemyObjects.First();
                int enemyObjectIndex = enemyObjects.IndexOf(enemyToSpawn);
                StartCoroutine(SpawnEnemy(enemyToSpawn));
                enemyObjects.RemoveAt(enemyObjectIndex);
            }
            
        }
    }

	/* 
	public IEnumerator SpawnEnemyAtPosition(GameObject theEnemy, Vector3 position) {
		float SpawnDelay = 1f;
		float tileFlashSpeed = 5f;

		Transform spawnTile = map.GetTileFromPosition (position);

		Material tileMat = spawnTile.GetComponent<Renderer>().material;
		Color initialColor = tileMat.color;
		Color flashColor = Color.red;
		float spawnTimer = 0f;

		while (spawnTimer < SpawnDelay) {
			tileMat.color = Color.Lerp (initialColor, flashColor, Mathf.PingPong (spawnTimer * tileFlashSpeed, 1));

			spawnTimer += Time.deltaTime;
			yield return null;
		}

		tileMat.color = initialColor;
		GameObject spawnedEnemy = null;
		if (type == EnemyType.Blue) {
		    spawnedEnemy = PhotonNetwork.Instantiate (enemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemyType.Green) {
		    spawnedEnemy = PhotonNetwork.Instantiate (greenEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemyType.Yellow) {
			spawnedEnemy = PhotonNetwork.Instantiate (yellowEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemyType.Orange) {
			spawnedEnemy = PhotonNetwork.Instantiate (orangeEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemyType.Black) {
			spawnedEnemy = PhotonNetwork.Instantiate (blackEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}

		if (spawnedEnemy != null) {
			spawnedEnemy.transform.SetParent (GameObject.Find ("EnemyStorage").transform);
			GameObject particles = Instantiate (spawnParticles, spawnTile.position + Vector3.up, Quaternion.identity);
			particles.transform.SetParent (GameObject.Find ("TempStorage").transform);
		}
	}
	*/



	IEnumerator SpawnEnemy(GameObject theEnemy) {
		float SpawnDelay = 1f;
		float tileFlashSpeed = 5f;

		Transform spawnTile = map.GetRandomOpenTile ();

		Material tileMat = spawnTile.transform.Find("Octagon").GetComponent<Renderer> ().material;

        Color initialColor = tileMat.color;
		Color flashColor = Color.red;
		//if (type == EnemyType.Black) {
         //   flashColor = Color.green;
		//} 
		float spawnTimer = 0f;

		while (spawnTimer < SpawnDelay) {
			tileMat.color = Color.Lerp (initialColor, flashColor, Mathf.PingPong (spawnTimer * tileFlashSpeed, 1));

			spawnTimer += Time.deltaTime;
			yield return null;
		}

		tileMat.color = initialColor;
        GameObject spawnedEnemy = PhotonNetwork.Instantiate("Enemies/" + theEnemy.name, new Vector3(spawnTile.position.x, spawnTile.position.y + 0.5f, spawnTile.position.z), Quaternion.identity, 0);
		/* 

		if (type == EnemyType.Blue) {
		    spawnedEnemy = PhotonNetwork.Instantiate (enemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemyType.Green) {
		    spawnedEnemy = PhotonNetwork.Instantiate (greenEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemyType.Yellow) {
			spawnedEnemy = PhotonNetwork.Instantiate (yellowEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemyType.Orange) {
			spawnedEnemy = PhotonNetwork.Instantiate (orangeEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemyType.Black) {
			spawnedEnemy = PhotonNetwork.Instantiate (blackEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		*/

		if (spawnedEnemy != null) {
			spawnedEnemy.transform.SetParent (GameObject.Find ("EnemyStorage").transform);
			GameObject particles = Instantiate (spawnParticles, spawnTile.position + Vector3.up, Quaternion.identity);
            particles.AddComponent<UnrenderDespawn>();
			particles.transform.SetParent (GameObject.Find ("TempStorage").transform);
            spawnedEnemy.GetComponent<EnemyAI>().OnDeath += OnEnemyDeath;
		}
	}

	public void OnEnemyDeath() {
		remainingLivingEnemies--;

		if (remainingLivingEnemies <= 0) {
			Invoke ("PreformNextWave", 5f);
		}
	}

    public void PreformNextWave()
    {
        photonView.RPC("NextWave", RpcTarget.AllBuffered, "STARTING NEXT WAVE");  
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentWave);
            stream.SendNext(currentWaveNumber);
            stream.SendNext(remainingEnemiesToSpawn);
            stream.SendNext(remainingLivingEnemies);
            stream.SendNext(enemyTypesToSpawn);
            stream.SendNext(enemyObjects);
            //stream.SendNext(gameStarted);
        }
        else
        {
            this.currentWave = (Wave)stream.ReceiveNext();
            this.currentWaveNumber = (int)stream.ReceiveNext();
            this.remainingEnemiesToSpawn = (int)stream.ReceiveNext();
            this.remainingLivingEnemies = (int)stream.ReceiveNext();
            this.enemyTypesToSpawn = (List<WaveEnemies>)stream.ReceiveNext();
            this.enemyObjects = (List<GameObject>)stream.ReceiveNext();
            //this.gameStarted = (bool)stream.ReceiveNext();
        }
    }
    

	[PunRPC] public void NextWave(string debugStatment) {
        enemyTypesToSpawn.Clear();
		enemyObjects.Clear();
		currentWaveNumber++;
		Debug.Log ("Starting Wave: " + currentWaveNumber);  
        Debug.Log(debugStatment);
		foreach (Transform temp in GameObject.Find("TempStorage").transform) {
			Destroy (temp.gameObject);
		}
		if (currentWaveNumber - 1 < waves.Length) {
			currentWave = waves [currentWaveNumber - 1];

            foreach (WaveEnemies wEnemies in currentWave.enemies) {
				enemyTypesToSpawn.Add(wEnemies);
				for (int i = 0; i < wEnemies.enemyCount; i++) {
				    enemyObjects.Add(wEnemies.enemyPrefab);
				}
				Debug.Log(wEnemies.enemyCount.ToString());
				remainingEnemiesToSpawn += wEnemies.enemyCount;
			}

			remainingLivingEnemies = remainingEnemiesToSpawn;

			/* 
			remainingEnemiesToSpawn = currentWave.enemyCount + currentWave.greenEnemyCount + currentWave.yellowEnemyCount + currentWave.orangeEnemyCount + currentWave.blackEnemyCount;

			remainingBlueEnemiesToSpawn = currentWave.enemyCount;
			remainingGreenEnemiesToSpawn = currentWave.greenEnemyCount;
			remainingYellowEnemiesToSpawn = currentWave.yellowEnemyCount;
			remainingOrangeEnemiesToSpawn = currentWave.orangeEnemyCount;
			remainingBlackEnemiesToSpawn = currentWave.blackEnemyCount;

			remainingLivingEnemies = remainingEnemiesToSpawn;

			enemyTypesToSpawn = new List<EnemyType> ();

			if (remainingBlueEnemiesToSpawn != 0) {
				for (int i = 0; i < remainingBlueEnemiesToSpawn; i++) {
					enemyTypesToSpawn.Add (EnemyType.Blue);
				}
			}

			if (remainingGreenEnemiesToSpawn != 0) {
				for (int i = 0; i < remainingGreenEnemiesToSpawn; i++) {
					enemyTypesToSpawn.Add (EnemyType.Green);
				}
			}

			if (remainingYellowEnemiesToSpawn != 0) {
				for (int i = 0; i < remainingYellowEnemiesToSpawn; i++) {
					enemyTypesToSpawn.Add (EnemyType.Yellow);
				}
			}

			if (remainingOrangeEnemiesToSpawn != 0) {
				for (int i = 0; i < remainingOrangeEnemiesToSpawn; i++) {
					enemyTypesToSpawn.Add (EnemyType.Orange);
				}
			}

			if (remainingBlackEnemiesToSpawn != 0) {
				for (int i = 0; i < remainingBlackEnemiesToSpawn; i++) {
					enemyTypesToSpawn.Add (EnemyType.Black);
				}
			}
			*/

           
			//WaveEnemies[] enemyTypeSpawnArr = Utility.ShuffleArray (enemyTypesToSpawn.ToArray (), UnityEngine.Random.Range (1, 10));

			//enemyTypesToSpawn = enemyTypeSpawnArr.ToList ();
		}
	}

	[System.Serializable]
	public class Wave {
		public WaveEnemies[] enemies;
		public float timeBetweenSpawns;
	}

}

[System.Serializable] public struct WaveEnemies {
	public GameObject enemyPrefab;
	public int enemyCount;
}
