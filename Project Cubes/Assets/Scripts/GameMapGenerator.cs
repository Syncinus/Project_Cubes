using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapGenerator : MonoBehaviour {

	public static GameMapGenerator instance;

	void Awake() {
		instance = this;
	}

	public Map[] maps;
	public int mapIndex;

    //public List<Coord> obstacleTileCoords = new List<Coord>();
    public LayerMask unwalkableMask;
	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public Transform navmeshFloor;
	public Transform navmeshMaskPrefab;
	//public int mapSizeX;
	//public int mapSizeY;
	//public Coord mapSizeCoord;
	//public Vector2 mapSize;
	public Vector2 maxMapSize;

	//public float minObstacleHeight;
	//public float maxObstacleHeight;
	//public Color foregroundColor;
	//public Color backgroundColor;

	[Range(0,1)]
	public float outlinePercent;
	//[Range(0,1)]
	//public float obstaclePercent;

	public float tileSize;

	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;
	Queue<Coord> shuffledOpenTileCoords;
	Transform[,] tileMap;
    Node[,] grid;

	Map currentMap;
    Transform player;

    float nodeDiameter;
    int gridSizeX;
    int gridSizeY;


	//public int seed = 10;
	//Coord mapCentre;

	public void Start() {
		Generate ();
	}

    public void FixedUpdate()
    {
        if (player == null)
        {
            if (GameObject.Find("PlayerCube(Clone)") != null)
            {
                player = GameObject.Find("PlayerCube(Clone)").transform;
            }
        }
    }

    Coord[] RandomizeArray(Coord[] arr) {
		Coord[] theArray = arr;
		for (int i = theArray.Length - 1; i > 0; i--) {
			int r = UnityEngine.Random.Range (0, i);
			Coord tmp = theArray [i];
			theArray [i] = theArray [r];
			theArray [r] = tmp;
		}
		return theArray;
	}

	public void Generate() {
		currentMap = maps [mapIndex];

        //float tileRealSize = 1 * (1 - outlinePercent) * tileSize;
        //nodeDiameter = tileSize * 1;
        //gridSizeX = Mathf.RoundToInt(currentMap.mapSize.x / nodeDiameter);
        //gridSizeY = Mathf.RoundToInt(currentMap.mapSize.y / nodeDiameter);

        grid = new Node[currentMap.mapSize.x, currentMap.mapSize.y];
		tileMap = new Transform [currentMap.mapSize.x, currentMap.mapSize.y];
        //Vector3 worldBottomLeft = this.transform.position - Vector3.right * currentMap.mapSize.x / 2 - Vector3.forward * currentMap.mapSize.y / 2;
		System.Random prng = new System.Random (currentMap.seed);
		GetComponent<BoxCollider> ().size = new Vector3 (currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y * tileSize);
        //Vector3 worldBottomLeft = this.transform.position - Vector3.right * currentMap.mapSize.x / 2 - Vector3.forward * currentMap.mapSize.y / 2;
		//GetComponent<Rigidbody> ()?.AddForce (Vector3.zero);
		
		allTileCoords = new List<Coord> ();
		for (int x = 0; x < currentMap.mapSize.x; x++) {
			for (int y = 0; y < currentMap.mapSize.y; y++) {
                //Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + tileSize) + Vector3.forward * (y * nodeDiameter + tileSize);
                //worldPoint.z;
                //bool walkable = !(Physics.CheckSphere(worldPoint, tileSize, unwalkableMask));
                //grid[x, y] = new Node(walkable, worldPoint);
			   allTileCoords.Add(new Coord(x, y));
			}
		}

		shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

		string holderName = "Generated Map";
		if (this.transform.Find (holderName)) {
			DestroyImmediate (this.transform.Find (holderName).gameObject);
		}

		Transform mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = this.transform;

		for (int x = 0; x < currentMap.mapSize.x; x++) {
			for (int y = 0; y < currentMap.mapSize.y; y++) {
				Vector3 tilePosition = CoordToPosition (x, y);
				Transform newTile = Instantiate (tilePrefab, tilePosition, Quaternion.Euler (Vector3.right * 90)) as Transform;
				newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				newTile.SetParent (mapHolder);
				tileMap [x, y] = newTile;
                bool walkable = !(Physics.CheckSphere(tilePosition, tileSize, unwalkableMask));
                grid[x, y] = new Node(walkable, tilePosition, x, y);
			}
		}
		 	
		bool[,] obstacleMap = new bool[(int) currentMap.mapSize.x, (int) currentMap.mapSize.y];

		int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
		int currentObstacleCount = 0;
		List<Coord> allOpenCoords = new List<Coord> (allTileCoords);

		for (int i = 0; i < obstacleCount; i++) {
			Coord randomCoord = GetRandomCoord ();
			obstacleMap [randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;

			if (randomCoord != currentMap.mapCentre && MapIsFullyAccessable (obstacleMap, currentObstacleCount)) {
				float obstacleHeight = Mathf.Lerp (currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble ());
				Vector3 obstaclePosition = CoordToPosition (randomCoord.x, randomCoord.y);
                Transform obstacleTile = GetTileFromPosition(obstaclePosition).Find("Octagon");

				Transform newObstacle = Instantiate (obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2f, Quaternion.identity) as Transform;
				newObstacle.SetParent (mapHolder);
				newObstacle.localScale = new Vector3 ((1.01f - outlinePercent) * tileSize, obstacleHeight, (1.02f - outlinePercent) * tileSize);


				Renderer obstacleRenderer = newObstacle.transform.Find("Octagon").GetComponent<Renderer> ();
                Renderer tileRenderer = obstacleTile.GetComponent<Renderer>();

				Material obstacleMaterial = new Material (obstacleRenderer.sharedMaterial);
				float colorPercent = randomCoord.y / (float) currentMap.mapSize.y;
 
				obstacleMaterial.color = Color.Lerp (currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                
				obstacleRenderer.sharedMaterial = obstacleMaterial;
                tileRenderer.sharedMaterial = obstacleMaterial;

                /*
                foreach (Transform line in newObstacle)
                {
                    Renderer lineRenderer = line.GetComponent<Renderer>();
                    Material lineMaterial = new Material(lineRenderer.sharedMaterial);
                    float lineColorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                    lineMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, lineColorPercent);

                    lineRenderer.sharedMaterial = lineMaterial;
                }
                */

                allOpenCoords.Remove (randomCoord);
                grid[randomCoord.x, randomCoord.y].walkable = false;

			} else {
				obstacleMap [randomCoord.x, randomCoord.y] = true;
				currentObstacleCount--;
			}
		}

		shuffledOpenTileCoords = new Queue<Coord> (Utility.ShuffleArray (allOpenCoords.ToArray (), currentMap.seed));

		Transform maskLeft = Instantiate (navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + currentMap.mapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
		maskLeft.SetParent (mapHolder);
		maskLeft.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2, 1, currentMap.mapSize.y) * tileSize;

		Transform maskRight = Instantiate (navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + currentMap.mapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
		maskLeft.SetParent (mapHolder);
		maskLeft.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2, 1, currentMap.mapSize.y) * tileSize;

		Transform maskTop = Instantiate (navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.x + currentMap.mapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
		maskLeft.SetParent (mapHolder);
		maskLeft.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2) / tileSize;

		Transform maskBottom = Instantiate (navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.x + currentMap.mapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
		maskLeft.SetParent (mapHolder);
		maskLeft.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2) / tileSize;

        //navmeshFloor.localScale = new Vector3 (maxMapSize.x, maxMapSize.y) * tileSize;
        this.GetComponent<AstarPath>().enabled = true;
        //AstarPath.active.Scan();
	}

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < currentMap.mapSize.x && checkY >= 0 && checkY < currentMap.mapSize.y)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }



	public Transform GetTileFromPosition(Vector3 position) {
		int x = Mathf.RoundToInt (position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
		int y = Mathf.RoundToInt (position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
		x = Mathf.Clamp (x, 0, tileMap.GetLength (0) - 1); 
		y = Mathf.Clamp (y, 0, tileMap.GetLength (0) - 1); 
		return tileMap [x, y];
	}

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + currentMap.mapSize.x / 2) / currentMap.mapSize.x;
        float percentY = (worldPosition.z + currentMap.mapSize.y / 2) / currentMap.mapSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int CMX = Mathf.RoundToInt(currentMap.mapSize.x / 2);
        int CMY = Mathf.RoundToInt(currentMap.mapSize.y / 2);

        int x = Mathf.RoundToInt((CMX) * percentX) + 12;
        int y = Mathf.RoundToInt((CMY) * percentY) + 12;

        return grid[x, y];
    }

    public List<Node> path;

    /*
    private void OnDrawGizmos()
    {
        if (Application.isPlaying == true)
        {
            Gizmos.DrawWireCube(this.transform.position, new Vector3(currentMap.mapSize.x, 1, currentMap.mapSize.y));

            if (grid != null)
            {
                foreach (Node n in grid)
                {
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    if (path != null)
                    {
                        if (path.Contains(n))
                        {
                            Gizmos.color = Color.black;
                        }
                    }

                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (tileSize - outlinePercent));
                }
            }
        }
    }
    */


    public Transform GetRandomOpenTile() {
		Coord randomCoord = shuffledOpenTileCoords.Dequeue();
		shuffledOpenTileCoords.Enqueue (randomCoord);
		return tileMap [randomCoord.x, randomCoord.y];
	}

	bool MapIsFullyAccessable(bool[,] obstacleMap, int currentObstacleCount) {
		bool[,] mapFlags = new bool[obstacleMap.GetLength (0), obstacleMap.GetLength (1)];
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (currentMap.mapCentre);
		mapFlags [currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

		int accessibleTileCount = 1;

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue ();

			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					int neighborX = tile.x + x;
					int neighborY = tile.y + y;
					if (x == 0 || y == 0) {
						if (neighborX >= 0 && neighborX < obstacleMap.GetLength (0) && neighborY >= 0 && neighborY < obstacleMap.GetLength (1)) {
							if (!mapFlags [neighborX, neighborY] && !obstacleMap [neighborX, neighborY]) {
								mapFlags [neighborX, neighborY] = true;
								queue.Enqueue (new Coord (neighborX, neighborY));
								accessibleTileCount++;
							}
						}
					}
				}
			}
		}

		int targetAccesibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
		return targetAccesibleTileCount == accessibleTileCount;
	}

	public Vector3 CoordToPosition(int x, int y) {
		return new Vector3 (-currentMap.mapSize.x / 2 + 0.5f + x, 0, -currentMap.mapSize.y / 2 + 0.5f + y) * tileSize;
	}

	public Coord GetRandomCoord() {
		Coord randomCoord = shuffledTileCoords.Dequeue ();
		shuffledTileCoords.Enqueue (randomCoord);
		return randomCoord;
	}

	[System.Serializable]
	public struct Coord {
		public int x;
		public int y;

		public Coord (int _x, int _y) {
			x = _x;
			y = _y;
		}

		public static bool operator == (Coord c1, Coord c2) {
			return c1.x == c2.x && c1.y == c2.y;
		}

		public static bool operator != (Coord c1, Coord c2) {
			return !(c1 == c2);
		}

	}

	[System.Serializable]
	public class Map {
		public Coord mapSize;
		[Range(0,1)]
		public float obstaclePercent;
		public int seed;
		public float minObstacleHeight;
		public float maxObstacleHeight;
		public Color foregroundColor;
		public Color backgroundColor;

		public Coord mapCentre {
			get {
				return new Coord (mapSize.x / 2, mapSize.y / 2);
			}
		}
	}
}
