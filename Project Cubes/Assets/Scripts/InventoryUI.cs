using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryUI : MonoBehaviour {
    public GameObject inventoryUI;
	public Transform itemsParent;
    public GameObject slot;

	Inventory inventory;

	void Start() {
		inventory = Inventory.instance;
		inventory.onItemChangedCallback += UpdateUI;
	}

	public void Update() {
		if (Input.GetKeyDown(KeyCode.I)) {
			inventoryUI.SetActive(!inventoryUI.activeSelf);
			UpdateUI();
		}


	}

	public void UpdateUI() {
		InventorySlot[] slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        float timesOfNine = Mathf.Floor(slots.Length / 3f);



        if (slots.Length - 1 < inventory.items.Count)
        {
            GameObject newSlot = (GameObject) Instantiate(slot, this.transform.position, this.transform.rotation) as GameObject;
            newSlot.SetActive(true);
            newSlot.transform.SetParent(itemsParent, false);

            float timesOfThree = 0f;
            timesOfThree = Mathf.Floor(slots.Length / 3f);

            Debug.Log(timesOfThree.ToString());


            float yChanger = -20f * timesOfThree;


            float x = (35f * slots.Length) - 35f;

            if (yChanger != 0f)
            {
                float xChanger = timesOfThree * 105f;
                x = (35f * slots.Length) - 35f - xChanger;
            }


             Vector3 pos = new Vector3(x, yChanger + 25f, 0f);

            if (slots.Length > 0)
            {
               //float yChanger = Mathf.Floor(slots.Length / 3) * -30;
                //pos.y += yChanger;
            }
            newSlot.GetComponent<RectTransform>().anchoredPosition3D = pos;
        }

		for(int i = 0; i < slots.Length; i++) {
			if (i < inventory.items.Count) {
				slots[i].AddItem(inventory.items[i]);
			} else {
				slots[i].ClearSlot();
			}
		}
	}
}
