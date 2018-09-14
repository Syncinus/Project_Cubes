using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasterEggs : Photon.MonoBehaviour
{
    private KeyCode[] boxBlasterSequence = new KeyCode[] { KeyCode.Keypad1  , KeyCode.Keypad7, KeyCode.Keypad9, KeyCode.Keypad3, KeyCode.Keypad2, KeyCode.Keypad4, KeyCode.Keypad8, KeyCode.Keypad6, KeyCode.Keypad5 };
    private KeyCode[] scoreBlaster = new KeyCode[] { KeyCode.S, KeyCode.C, KeyCode.O, KeyCode.R, KeyCode.E };

    private int bbsequenceIndex = 0;
    private int scoreBlasterIndex = 0;

    [SerializeField] private WeaponItem boxBlaster;
        
    private void Update()
    {
        if (Input.GetKeyDown(boxBlasterSequence[bbsequenceIndex]))
        {
            if (++bbsequenceIndex == boxBlasterSequence.Length)
            {
                bbsequenceIndex = 0;
                PlayerCube[] cubes = FindObjectsOfType<PlayerCube>();
                foreach (PlayerCube cube in cubes)
                {
                    if (cube.GetComponent<PhotonView>().isMine == true)
                    {
                        Inventory.instance.ForceAdd(boxBlaster);
                        Debug.Log("CONGRATULATIONS! YOU GOT THE BOX BLASTER!");
                    }
                }
            }
        }
        else if (Input.anyKeyDown) bbsequenceIndex = 0;


        if (Input.GetKeyDown(scoreBlaster[scoreBlasterIndex]))
        {
            if (++scoreBlasterIndex == scoreBlaster.Length)
            {
                scoreBlasterIndex = 0;
                PlayerCube[] cubes = FindObjectsOfType<PlayerCube>();
                foreach (PlayerCube cube in cubes)
                {
                    if (cube.GetComponent<PhotonView>().isMine == true)
                    {
                        Debug.Log("SCORE BLASTER!!!");
                        StartCoroutine(ScoreBlaster());
                    }
                }
            }
        }
        else if (Input.anyKeyDown) scoreBlasterIndex = 0;
    }

    IEnumerator ScoreBlaster()
    {
        float timer = 0.0f;
        float score = 0f;
        float scoreToGoTo = 100000f;

        while (timer < 3.0f)
        {
            ScoreSystem.Score = Mathf.Lerp(score, scoreToGoTo, timer);
            timer += Time.deltaTime;
            yield return null;
        }

        ScoreSystem.Score = 100000f;
    }
}
