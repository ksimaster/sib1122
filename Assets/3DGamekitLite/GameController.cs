using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit3D;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject PanelInfo;
    public GameObject PanelDeath;
    public GameObject PanelWin;
    private void Start()
    {
        Time.timeScale = 1;
    }
    public void Restart()
    {
        //SceneController.RestartZone();
        SceneManager.LoadScene("ExampleScene");
    }
    public void Resume()
    {
        Time.timeScale = 1;
    }

    private void Update()
    {
        if(PanelInfo.activeSelf || PanelDeath.activeSelf || PanelWin.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
           
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            
        }
    }
}
