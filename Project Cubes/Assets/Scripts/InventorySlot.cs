using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class InventorySlot : MonoBehaviour, IPointerClickHandler {

	public Image icon;

    public Text countText;
	
	public WeaponItem item;
    public int Count;

    public float startingY;

	public void AddItem(WeaponItem newItem) {
		item = newItem;

		icon.sprite = newItem.icon;
		icon.enabled = true;
	}

	public void Update() {
		if (item == null) {
			icon.enabled = false;
		} else {
			icon.enabled = true;
		}
	}

	public void RemoveFromInventory() {
		Inventory.instance.Remove(item);
	}

	public void ClearSlot() {
		item = null;

		icon.enabled = false;
		icon.sprite = null;
	}

	public void UseItem() {
		if (item != null) {
			item.Equip();
		}
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (eventData.button == PointerEventData.InputButton.Left) {
			UseItem();
		}
		if (eventData.button == PointerEventData.InputButton.Right) {
			RemoveFromInventory();
		}
	}
}
