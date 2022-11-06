using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DetectorDeath : MonoBehaviour
{
    
    public GameObject PanelDeath;
    private void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Death")
        {
            // SceneManager.LoadScene("ExampleScene");
            Time.timeScale = 0f;
            PanelDeath.SetActive(true);
        }
    }
}
