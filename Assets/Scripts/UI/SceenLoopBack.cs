using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceenLoopBack : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Invoke("LoadNewScene", 1f);
    }

    private void LoadNewScene()
    {
        SceneManager.LoadSceneAsync("01_SceneAuthentication", LoadSceneMode.Single);
    }
}
