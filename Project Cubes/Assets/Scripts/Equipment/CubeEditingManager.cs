using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class CubeEditingManager : MonoBehaviour {

    public Transform[] buttonHolders;
    CubeEditingButton[] buttons;

    public Text info;

    private void Start()
    {
        UpdateInfo();
        foreach (Transform buttonHolder in buttonHolders)
        {
            buttons = buttonHolder.GetComponentsInChildren<CubeEditingButton>();
            foreach (CubeEditingButton button in buttons)
            {
                if (CubeSettings.Score > button.RequiredScore)
                {
                    button.Unlock();
                }
            }
        }
    }

    public void UpdateInfo()
    {
        string w = "NULL";
        string m = "NULL";
        if (CubeSettings.weapon != null)
        {   
            w = CubeSettings.weapon.name;
        }
        if (CubeSettings.manipulator != null)
        {
            m = CubeSettings.manipulator.name;
        }
        info.text = "WEAPON: " + w + " " + "MANIPULATOR: " + m;
    }

    public void Equip(WeaponItem item)
    {
        CubeSettings.weapon = item;
        CubeSettings.weapon.Points = item.Points;
        UpdateInfo();
    }

    public void Equip(RealityManipulator manipulator)
    {
        CubeSettings.manipulator = manipulator;
        UpdateInfo();

    }

    public void UnequipManipulator(RealityManipulator manipulator)
    {
        if (CubeSettings.manipulator == manipulator)
        {
            CubeSettings.manipulator = null;
            UpdateInfo();
        }
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
