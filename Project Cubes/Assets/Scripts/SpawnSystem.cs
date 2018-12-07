using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnSystem : MonoBehaviourPunCallbacks {

	GameMapGenerator map;
	EnemySpawner eSpawner;
	
	private bool ableToSpawn = true;
	public float spawnRefreshTime = 0.5f;
	public int spawnTrys = 1;
    public bool isTerrorizer;
    public bool isTerrorizerChild;
    [HideInInspector] public Transform terrorizer;

	public List<EnemyToSpawn> enemiesToSpawn = new List<EnemyToSpawn>();

	


	public void Start() {
		map = GameMapGenerator.instance;
		eSpawner = EnemySpawner.instance;
	}

	
	IEnumerator SpawnEnemy(GameObject enemyPrefab, Vector3 position) {
        float spawnDelay = 0.15f;
		float tileFlashSpeed = 1f;

		Transform spawnTile = map.GetTileFromPosition(position);

		Material tileMat = spawnTile.GetComponent<Renderer>().material;
		Color initialColor = tileMat.color;
		Color flashColor = Color.green;
		float spawnTimer = 0f;

		while (spawnTimer < spawnDelay) {
			tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

			spawnTimer += Time.deltaTime;
			yield return null;
		}

		tileMat.color = initialColor;
        GameObject spawnedEnemy = PhotonNetwork.Instantiate ("Enemies/" + enemyPrefab.name, spawnTile.position, Quaternion.identity, 0);
		/* 
		if (type == EnemySpawner.EnemyType.Blue) {
		    spawnedEnemy = PhotonNetwork.Instantiate (eSpawner.enemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemySpawner.EnemyType.Green) {
		    spawnedEnemy = PhotonNetwork.Instantiate (eSpawner.greenEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemySpawner.EnemyType.Yellow) {
			spawnedEnemy = PhotonNetwork.Instantiate (eSpawner.yellowEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemySpawner.EnemyType.Orange) {
			spawnedEnemy = PhotonNetwork.Instantiate (eSpawner.orangeEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		if (type == EnemySpawner.EnemyType.Black) {
			spawnedEnemy = PhotonNetwork.Instantiate (eSpawner.blackEnemyp.name, spawnTile.position, Quaternion.identity, 0);
		}
		*/

		if (spawnedEnemy != null) {
			spawnedEnemy.transform.SetParent(GameObject.Find("EnemyStorage").transform);
			GameObject particles = Instantiate(eSpawner.spawnParticles, spawnTile.position + Vector3.up, Quaternion.identity);
			particles.transform.SetParent(GameObject.Find("TempStorage").transform);
            if (isTerrorizer == true || isTerrorizerChild == true)
            {
                if (isTerrorizer == true)
                {
                   // this.transform.GetComponent<TerrorizerAI>().objectStorage.Add(spawnedEnemy);
                }  else
                {
                   // terrorizer.GetComponent<TerrorizerAI>().objectStorage.Add(spawnedEnemy);
                    this.GetComponent<EnemyAI>().enemyMode = EnemyAI.Mode.Assault;
                }
                spawnedEnemy.transform.SetParent(GameObject.Find("TempStorage").transform);
                SpawnSystem sSystem = spawnedEnemy.GetComponent<SpawnSystem>();
                if (sSystem != null)
                {
                    sSystem.isTerrorizerChild = true;
                    sSystem.terrorizer = this.transform;
                    this.GetComponent<EnemyAI>().enemyMode = EnemyAI.Mode.Assault;
                }
            }
        }
	}


    public void FixedUpdate()
    {
        if (GameObject.Find("PlayerCube(Clone)") != null)
        {
            if (ableToSpawn == true)
            {
                //eSpawner.StartCoroutine(eSpawner.SpawnEnemyAtPosition(EnemySpawner.EnemyType.Blue, this.transform.position));
                for (int i = 0; i < spawnTrys; i++)
                {
                    foreach (EnemyToSpawn enemy in enemiesToSpawn)
                    {
                        if (Random.Range(0f, 100f) <= enemy.chance)
                        {
                            System.Random rnd = new System.Random();
                            int rndContainer = rnd.Next(1, 10);
                            if (rndContainer <= 5)
                            {
                                //Right
                               StartCoroutine(SpawnEnemy(enemy.prefab, (this.transform.position + (this.transform.right * 2))));
                            } else if (rndContainer > 5)
                            {
                                //Left
                                StartCoroutine(SpawnEnemy(enemy.prefab, (this.transform.position + ((this.transform.right * -1) * 2))));
                            }
                            break;
                        }
                    }
                }


                ableToSpawn = false;
                Invoke("ReEnableSpawning", spawnRefreshTime);
            }
        }
    }

	public void ReEnableSpawning() {
		ableToSpawn = true;
	}

}

[System.Serializable] public struct EnemyToSpawn {
	public GameObject prefab;
	public float chance;
}
