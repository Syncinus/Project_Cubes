using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour {

	public static EquipmentManager instance {
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<EquipmentManager>();
			}
			return _instance;
		}
	}
	static EquipmentManager _instance;

	void Awake() {
		_instance = this;
	}

	public WeaponItem defaultGun;

	public WeaponItem currentGun;

	public delegate void OnWeaponChanged(WeaponItem newGun, WeaponItem oldGun);
	public event OnWeaponChanged onWeaponChanged;

	Inventory inventory;

	void Start() {
		inventory = Inventory.instance;
		currentGun = defaultGun;
	}

    public void Equip(WeaponItem newItem) {
		WeaponItem oldItem = null;

		if (currentGun != null) {
			oldItem = currentGun;
			UnEquip(oldItem);
			//inventory.Add(oldItem);
		}


		if (onWeaponChanged != null)
		    onWeaponChanged.Invoke(newItem, oldItem);

			currentGun = newItem;
			Debug.Log(newItem.gunName + " Was Equipped!");

	}

	public void UnEquip(WeaponItem item) {
		WeaponItem oldGun = item;
		inventory.ForceAdd(oldGun);

		currentGun = null;

		if (onWeaponChanged != null) {
			onWeaponChanged.Invoke(null, oldGun);
		}
	}


}
