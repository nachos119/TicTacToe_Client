using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSceneController : MonoBehaviour
{
    private void Start()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("IntroScene");
    }
}
