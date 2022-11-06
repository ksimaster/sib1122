using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountMonster : MonoBehaviour
{
    public int count;
    public Text countText;
    public GameObject PanelInfo;
    public GameObject TriggerOut;

    void Start()
    {
        PlayerPrefs.SetInt("count", count);
        countText.text = count.ToString();
    }
    void Update()
    {
        countText.text = PlayerPrefs.GetInt("count").ToString();
        if (PlayerPrefs.GetInt("count") == 0) {
            Time.timeScale = 0;
            PanelInfo.SetActive(true);
            PanelInfo.SetActive(true);
        }
        
    }

    public void DecreaseCount()
    {
        count--;
        PlayerPrefs.SetInt("count", count);
    }


}
