using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("ExampleScene");
    }
    public void Resume()
    {
        Time.timeScale = 1;
    }
}
