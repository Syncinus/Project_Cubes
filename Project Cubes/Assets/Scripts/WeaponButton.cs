using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class WeaponButton : MonoBehaviourPunCallbacks {
    public Text name;
    public Text cost;
    public Text description;
    public WeaponItem wItem;    

    void Start() {
        if (wItem == null) {
            ClearSlot();
        }
        if (wItem != null) {
            SetButton();
        }
    }

    public void AddNewWeapon(WeaponItem newWeapon) {
        wItem = newWeapon;

        SetButton();
    }

    public void ClearSlot() {
        wItem = null;


        name.enabled = false;
        cost.enabled = false;
        description.enabled = false;
    }


    public void RemoveFromShop() {
        if (wItem != null) {
            Shop.instance.Remove(wItem);
        }
    }

    void SetButton() {
        //string costString = wItem.cost.ToString();
        name.enabled = true;
        cost.enabled = true;
        description.enabled = true;
        //name.text = wItem.gunName;
        //cost.text = "#" + wItem.cost;
        //description.text = wItem.description;
    }

    public void OnClick() {
        if (wItem != null) {
            //if (ScoreSystem.Score >= wItem.cost && Inventory.instance.items.Count < Inventory.instance.space) {
            //   ScoreSystem.Score -= wItem.cost;
            //    Inventory.instance.Add(wItem);
            //    RemoveFromShop();
            //}
        }
    }


}
