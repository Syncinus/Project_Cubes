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

	public WeaponItem currentGun;

	public delegate void OnWeaponChanged(WeaponItem newGun, WeaponItem oldGun);
	public event OnWeaponChanged onWeaponChanged;


	void Start() {
        Equip(CubeSettings.weapon);
	}

    public void Equip(WeaponItem newItem) {
		WeaponItem oldItem = null;

		if (onWeaponChanged != null)
		    onWeaponChanged.Invoke(newItem, oldItem);

			currentGun = newItem;
			Debug.Log(newItem.gunName + " Was Equipped!");

	}
}
