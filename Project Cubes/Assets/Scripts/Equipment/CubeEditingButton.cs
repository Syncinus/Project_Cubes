using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CubeEditingButton : MonoBehaviour {
    public bool Unlocked = false;
    public float RequiredScore;
    private Button button;

    private void Awake()
    {
        button = this.GetComponent<Button>();
        if (Unlocked == false)
        {
            button.enabled = false;
        }
    }

    public void Unlock()
    {
        Unlocked = true;
        button.enabled = true;
    }
}
