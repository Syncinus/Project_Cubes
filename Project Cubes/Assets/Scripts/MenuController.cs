using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuController : MonoBehaviour {
    public void Load(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
