using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class CubeEditingManager : MonoBehaviour {

    public Transform buttonHolder;
    CubeEditingButton[] buttons;

    private void Start()
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

    public void Equip(WeaponItem item)
    {
        CubeSettings.weapon = item;
        CubeSettings.weapon.Emmision = item.Emmision;
        CubeSettings.weapon.Shooting = item.Shooting;
        CubeSettings.weapon.Sound = item.Sound;
        CubeSettings.weapon.Particles = item.Particles;
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("ProjectCubesMain");
    }
}
