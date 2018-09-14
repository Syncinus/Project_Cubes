using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StateSystems;

public class BossAI : EnemyAI {
    public bool switchState = false;
    public State<BossAI> currentState;
    public BossStateSystem<BossAI> stateSystem { get; set; }

    [HideInInspector] public List<State<BossAI>> states = new List<State<BossAI>>();


    [HideInInspector] public bool orderlyStates = false;
    [HideInInspector] public bool functionial = true;

    public virtual void Start()
    {
        stateSystem = new BossStateSystem<BossAI>(this);

        if (states.Any())
        {
            if (orderlyStates == true)
            {
                SwapState(states.ElementAt(0));
            } else
            {
                SwapState(states.ElementAt(0));
            }
        }
    }

    private void Update()
    {
        stateSystem.Update();
    }

    public void SwapState(State<BossAI> newState)
    {
        Debug.Log("Swapping State!");
        stateSystem.ChangeState(newState);
        currentState = newState;
    }

    public IEnumerator RandomizeState(int usedStateIndex)
    {
        ai.isStopped = true;
        functionial = false;
        Debug.Log("Randomizing");
        yield return new WaitForSeconds(0.4f);

        List<State<BossAI>> possibleStates = states.ToList();
        //BaseState 

        possibleStates.RemoveAt(usedStateIndex);
        int RandomStateNumber = Random.Range(0, possibleStates.Count);
        Debug.Log(RandomStateNumber.ToString());
        SwapState(possibleStates.ElementAt(RandomStateNumber));

        functionial = true;
        ai.isStopped = false;
    }

    public void RandomizeStateWithoutDelay(int usedStateIndex)
    {
        ai.isStopped = true;
        functionial = false;
        Debug.Log("Randomizing");

        List<State<BossAI>> possibleStates = states;
        //BaseState 

        possibleStates.RemoveAt(usedStateIndex);
        if (possibleStates.Count >= 2)
        {
            int RandomStateNumber = Random.Range(0, possibleStates.Count);
            Debug.Log(RandomStateNumber.ToString());
            SwapState(possibleStates.ElementAt(RandomStateNumber));
        } else
        {
            SwapState(possibleStates.ElementAt(0));
        }

        functionial = true;
        ai.isStopped = false;
    }
}

public class BaseState : State<BossAI>
{
    private string stateName;
    private static BaseState _instance;

    public BaseState()
    {
        if (_instance != null)
        {
            return;
        }

        _instance = this;
        stateName = this.GetType().Name;
    }

    public static BaseState Instance
    {
        get
        {
            if (_instance == null)
            {
                new BaseState();
            }

            return _instance;
        }
    }

    public override void EnterState(BossAI _owner)
    {
        Debug.Log("Entering State: " + stateName);
    }

    public override void ExitState(BossAI _owner)
    {
        Debug.Log("Exiting State: " + stateName);
    }

    public override void UpdateState(BossAI _owner)
    {
        //Do Nothing
    }
}

