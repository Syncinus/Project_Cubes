using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour {
	public GameObject shopUI;
    public GameObject upgradeUI;
	public Transform itemsParent;
	public GameObject crateUI;

	Shop shop;

	void Start() {
        shop = Shop.instance;
		shop.onShopChangedCallback += UpdateShop;
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.O)) {
			shopUI.SetActive(!shopUI.activeSelf);
			UpdateShop();
		}
		if (Input.GetKeyDown(KeyCode.U)) {
			crateUI.SetActive(!crateUI.activeSelf);
		}
        if (Input.GetKeyDown(KeyCode.P))
        {
            upgradeUI.SetActive(!upgradeUI.activeSelf);
        }
	}

	public void UpdateShop() {
		WeaponButton[] slots = itemsParent.GetComponentsInChildren<WeaponButton>();

		for (int i = 0; i < slots.Length; i++) {
			if (i < shop.buyableItems.Count) {
				slots[i].AddNewWeapon(Shop.instance.buyableItems[i]);
			} else {
				slots[i].ClearSlot();
			}
		}
	}
}
