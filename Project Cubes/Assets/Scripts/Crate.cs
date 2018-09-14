using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crate", menuName = "Items/Crate")]
public class Crate : ScriptableObject {
    public int basicChance;
	public int advancedChance;
	public int eliteChance;
	public int legendaryChance;
	public int ultimateChance;
	public int Cost;
}
