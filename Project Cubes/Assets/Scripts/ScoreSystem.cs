using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSystem : MonoBehaviour {
    public static float Score;
	public float ScoreShown;
	public Text scoreText;

	public void FixedUpdate() {
		scoreText.text = "Score: " + Score.ToString ();
		ScoreShown = Score;
	}
}
