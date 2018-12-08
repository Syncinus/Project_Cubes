using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shop : MonoBehaviour {

	public static Shop instance;

	void Awake() {
		instance = this;
	}

	public delegate void OnShopChanged();
	public OnShopChanged onShopChangedCallback;

	public int shopSpace = 4;

	public List<WeaponItem> buyableItems = new List<WeaponItem>();
	private List<WeaponItem> itemsToLoad;
	private List<WeaponItem> boughtItems = new List<WeaponItem>();

	public void Start() {
        StartCoroutine(LoadInShopItems());
	}

	IEnumerator LoadInShopItems() {
		yield return new WaitUntil(() => onShopChangedCallback != null);
        WeaponItem[] loadItems = Resources.LoadAll<WeaponItem>("Weapons");
        itemsToLoad = loadItems.ToList();
        foreach (WeaponItem item in itemsToLoad) {
			//if (item.cost > 0) {
			//    buyableItems.Add(item);
			//    if (onShopChangedCallback != null) {
			//	    onShopChangedCallback.Invoke();
			//    }
			//}
		}
	}

	public void LoadInNewShopItem() {
        WeaponItem[] loadItems = Resources.LoadAll<WeaponItem>("Weapons");
        List<WeaponItem> possibleItems = new List<WeaponItem>();
		foreach (WeaponItem item in loadItems) {
			//if (item.cost > 0 && !boughtItems.Contains(item)) {
			//	possibleItems.Add(item);
			//}
		}

		WeaponItem newItem = possibleItems[Random.Range(0, possibleItems.Count - 1)];
		if (newItem != null) {
			Add(newItem);
			if (onShopChangedCallback != null) {
				onShopChangedCallback.Invoke();
			}
		}
	}

	public void Add (WeaponItem item) {
        if (buyableItems.Count >= shopSpace) {
			Debug.Log("No Room In The Shop.");
			return;
		}

		buyableItems.Add(item);

		if (onShopChangedCallback != null) {
			onShopChangedCallback.Invoke();
		}
	}

	public void Remove(WeaponItem item) {
		buyableItems.Remove(item);
		boughtItems.Add(item);

		if (onShopChangedCallback != null) {
			onShopChangedCallback.Invoke();
		}
	}

}
