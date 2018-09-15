using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class InventoryUI : MonoBehaviourPunCallbacks {
    public GameObject inventoryUI;
	public Transform itemsParent;
    public GameObject slot;

	Inventory inventory;

	void Start() {
		inventory = Inventory.instance;
		inventory.onItemChangedCallback += UpdateUI;
	}

	public void Update() {
        if (GameObject.Find("PlayerCube(Clone)") != null)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                inventoryUI.SetActive(!inventoryUI.activeSelf);

                if (inventoryUI.activeSelf == true)
                {
                    GameObject.Find("PlayerCube(Clone)").transform.GetComponent<ShootShots>().ableToFire = false;
                }
                else
                {
                    GameObject.Find("PlayerCube(Clone)").transform.GetComponent<ShootShots>().ableToFire = true;
                }
                UpdateUI();
            }
        }
	}

	public void UpdateUI() {
		InventorySlot[] slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        float timesOfNine = Mathf.Floor(slots.Length / 9f);
        float timesOfThreeLarge = Mathf.Floor(slots.Length / 3f);

        //itemsParent.localScale = new Vector3(1.5f, (timesOfThreeLargeScale / 0.5f) + 2.5f, 0f); 
        RectTransform rTransform = itemsParent.GetComponent<RectTransform>();

        rTransform.sizeDelta = new Vector2(rTransform.sizeDelta.x, -(-50f * timesOfNine) + -120f);
        rTransform.anchoredPosition = new Vector2(rTransform.anchoredPosition.x, -(-60f * timesOfNine));
        rTransform.localPosition = new Vector2(rTransform.localPosition.x, -(60f * timesOfNine) + -100f);

        foreach (InventorySlot slot in slots)
        {
            Vector3 slotNewPosition = slot.transform.GetComponent<RectTransform>().localPosition;
            slotNewPosition.y = slot.startingY + 25f * timesOfNine;
            slot.GetComponent<RectTransform>().localPosition = slotNewPosition;
        }


        

        if (slots.Length - 1 < inventory.items.Count)
        {
            GameObject newSlot = (GameObject) Instantiate(slot, this.transform.position, this.transform.rotation) as GameObject;
            newSlot.SetActive(true);
            newSlot.transform.SetParent(itemsParent, true);
            newSlot.transform.localScale = slot.transform.localScale;

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
            newSlot.GetComponent<InventorySlot>().startingY = newSlot.GetComponent<RectTransform>().anchoredPosition3D.y;
        }

        for (int i = 0; i < slots.Length; i++) {
			if (i < inventory.items.Count) {
				slots[i].AddItem(inventory.items[i]);
			} else {
				slots[i].ClearSlot();
			}
		}
	}
}
