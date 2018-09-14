using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;

public class AdvancedPathCalculation : MonoBehaviour {

    public BlockManager blockManager;
    public List<SingleNodeBlocker> obstacles;
    ABPath path;

    BlockManager.TraversalProvider traversalProvider;

	// Use this for initialization
	public void Start () {

        blockManager = GameObject.Find("MapGenerator").GetComponent<BlockManager>();
        obstacles = new List<SingleNodeBlocker>() {
             GameObject.Find("Friend").GetComponent<SingleNodeBlocker>()
        };
        traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.OnlySelector, obstacles);
       
        //base.Start();
	}
	
	// Update is called once per frame
	protected void Update () {
        //base.Update();

        path = ABPath.Construct(this.transform.position, this.GetComponent<EnemyAI>().target.position, null);

        path.traversalProvider = traversalProvider;

        AstarPath.StartPath(path);

        path.BlockUntilCalculated();

        path.Claim(this);


        if (path.error == true)
        {
            Debug.Log("No Path Was Found.");
        } else
        {
            Debug.Log("A Path Was Found With: " + path.vectorPath.Count + " Nodes");

            for (int i = 0; i < path.vectorPath.Count - 1; i++)
            {
                Debug.DrawLine(path.vectorPath[i], path.vectorPath[i + 1], Color.red);
            }

            this.GetComponent<Seeker>().StartPath(path.startPoint, path.endPoint, OnPathCompleteCallback);
        }
    }

    public void OnPathCompleteCallback(Path p)
    {
        Debug.Log("Finished Path!");
    }
}