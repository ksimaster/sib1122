using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController: MonoBehaviour
{
    public GameObject textSoundON;
    public GameObject textSoundOFF;


    public AudioSource [] sounds;


    private void Start()
    {
        if (PlayerPrefs.HasKey("SwitchSound"))
        {
            SoundSwitch(PlayerPrefs.GetInt("SwitchSound") == 1); // 1 - sound on (true), 0 - sound off (false)
        }
        else
        {
            PlayerPrefs.SetInt("SwitchSound", 1);
            SoundSwitch(PlayerPrefs.GetInt("SwitchSound") == 1);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (textSoundON.activeSelf)
            {
                PlayerPrefs.SetInt("SwitchSound", 0);
                SoundSwitch(PlayerPrefs.GetInt("SwitchSound") == 1);
            }
            else
            {
                PlayerPrefs.SetInt("SwitchSound", 1);
                SoundSwitch(PlayerPrefs.GetInt("SwitchSound") == 1);
            }
        }
    }

    public void SoundSwitch(bool swichVar)
    {
        foreach (AudioSource s in sounds)
        {
           s.volume = swichVar ? 1f : 0f;
        }
        textSoundON.SetActive(swichVar);
        textSoundOFF.SetActive(!swichVar);
    }



}
