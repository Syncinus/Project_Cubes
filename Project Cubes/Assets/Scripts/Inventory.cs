using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour {

	public static Inventory instance;

	void Awake() {
		instance = this;
	}

	public delegate void OnItemChanged();
	public OnItemChanged onItemChangedCallback;

	public int space = 6;

	public List<WeaponItem> items = new List<WeaponItem>();

	public void Add(WeaponItem item) {
		if (items.Count >= space) {
			Debug.Log("No Space");
			return;
		}

		items.Add(item);

		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	public void ForceAdd(WeaponItem item) {
		StartCoroutine(forceAdd(item));
	}

	public IEnumerator forceAdd(WeaponItem item) {
         yield return new WaitUntil(() => items.Count < space);
		 Add(item);
	}

	public void Remove(WeaponItem item) {
		items.Remove(item);

		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	} 

}
