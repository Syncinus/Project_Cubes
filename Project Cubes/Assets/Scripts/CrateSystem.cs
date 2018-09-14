using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CrateSystem : MonoBehaviour {

	public int cratesOwned = 0;
	public Crate CrateToUse;
	public Text cratesOwnedText;


	public void PurchaseCrate() {
        if (ScoreSystem.Score >= CrateToUse.Cost) {
			ScoreSystem.Score -= CrateToUse.Cost;
			cratesOwned += 1;
		}
	}

	public void FixedUpdate() {
		cratesOwnedText.text = "Crates Owned: " + cratesOwned.ToString();
	}

	void GenerateItem(int basic, int advanced, int elite, int legendary, int ultimate) {
        List<WeaponItem> AllItems = (Resources.LoadAll<WeaponItem>("Weapons")).ToList();
		int rnd = Random.Range(1, 100);
		Debug.Log(rnd.ToString());

		if (rnd >= 0 && rnd <= basic) {
			List<WeaponItem> Items = new List<WeaponItem>();
			foreach (WeaponItem item in AllItems) {
				if (item.rarity == WeaponRarity.Basic) {
					Items.Add(item);
				}
			}
			WeaponItem itemToUse = Items.ElementAt(Random.Range(0, Items.Count));
			Inventory.instance.Add(itemToUse);
 		}

		if (rnd >= basic + 1 && rnd <= advanced) {
			List<WeaponItem> Items = new List<WeaponItem>();
			foreach (WeaponItem item in AllItems) {
				if (item.rarity == WeaponRarity.Advanced) {
					Items.Add(item);
				}
			}
			WeaponItem itemToUse = Items.ElementAt(Random.Range(0, Items.Count));
			Inventory.instance.Add(itemToUse);
 		}

		 
		if (rnd >= advanced + 1 && rnd <= elite) {
			List<WeaponItem> Items = new List<WeaponItem>();
			foreach (WeaponItem item in AllItems) {
				if (item.rarity == WeaponRarity.Elite) {
					Items.Add(item);
				}
			}
			WeaponItem itemToUse = Items.ElementAt(Random.Range(0, Items.Count));
			Inventory.instance.Add(itemToUse);
 		}

		 
		if (rnd >= elite + 1 && rnd <= legendary) {
			List<WeaponItem> Items = new List<WeaponItem>();
			foreach (WeaponItem item in AllItems) {
				if (item.rarity == WeaponRarity.Legendary) {
					Items.Add(item);
				}
			}
			WeaponItem itemToUse = Items.ElementAt(Random.Range(0, Items.Count));
			Inventory.instance.Add(itemToUse);
 		}

		if (rnd >= legendary + 1 && rnd <= ultimate) {
			List<WeaponItem> Items = new List<WeaponItem>();
			foreach (WeaponItem item in AllItems) {
				if (item.rarity == WeaponRarity.Ultimate) {
					Items.Add(item);
				}
			}
			WeaponItem itemToUse = Items.ElementAt(Random.Range(0, Items.Count));
			Inventory.instance.Add(itemToUse);
 		}

		Debug.Log("Opened Crate!");
	}

	public void OpenCrate() {
		if (cratesOwned > 0) {
			cratesOwned -= 1;
		    GenerateItem(CrateToUse.basicChance, CrateToUse.advancedChance, CrateToUse.eliteChance, CrateToUse.legendaryChance, CrateToUse.ultimateChance);
		}
	}
}
