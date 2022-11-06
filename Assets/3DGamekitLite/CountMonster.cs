using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountMonster : MonoBehaviour
{
    public int count;
    public Text countText;
    void Start()
    {
        PlayerPrefs.SetInt("count", count);
        countText.text = count.ToString();
    }
    void Update()
    {
        countText.text = PlayerPrefs.GetInt("count").ToString();
    }

    public void DecreaseCount()
    {
        count--;
        PlayerPrefs.SetInt("count", count);
    }


}
