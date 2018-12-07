using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class CubeEditingManager : MonoBehaviour {

    public Transform buttonHolder;
    CubeEditingButton[] buttons;

    public Text info;

    private void Start()
    {
        UpdateInfo();
        buttons = buttonHolder.GetComponentsInChildren<CubeEditingButton>();
        foreach (CubeEditingButton button in buttons)
        {
            if (CubeSettings.Score > button.RequiredScore)
            {
                button.Unlock();
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

        if (CubeSettings.manipulators.Count > 0) {
            m = "";
            foreach (RealityManipulator manipulator in CubeSettings.manipulators)
            {
                m = m + manipulator.ToString() + " ";
            }
        }
        info.text = "WEAPON: " + w + " " + "MANIPULATORS: " + m;
    }

    public void Equip(WeaponItem item)
    {
        CubeSettings.weapon = item;
        CubeSettings.weapon.Points = item.Points;
        UpdateInfo();
    }

    public void Equip(RealityManipulator manipulator)
    {
        if (CubeSettings.manipulators.Count < 5)
        {
            CubeSettings.manipulators.Add(manipulator);
            UpdateInfo();
        }
    }

    public void UnequipManipulator(RealityManipulator manipulator)
    {
        if (CubeSettings.manipulators.Contains(manipulator))
        {
            CubeSettings.manipulators.Remove(manipulator);
            UpdateInfo();
        }
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
