using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DetectorDeath : MonoBehaviour
{
    public string Tag;
    public GameObject PanelDeath;
    private void OnTriggerEnter(Collider col)
    {
        if(col.tag == Tag)
        {
            // SceneManager.LoadScene("ExampleScene");
            Debug.Log("ялепрэ!!!!");
            Time.timeScale = 0f;
            PanelDeath.SetActive(true);
        }
    }
}
