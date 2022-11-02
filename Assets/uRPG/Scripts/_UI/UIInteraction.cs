﻿using UnityEngine;
using UnityEngine.UI;

public class UIInteraction : MonoBehaviour
{
    public GameObject panel;
    public Text hotkeyText;
    public Text actionText;

    void Update()
    {
        // looking at something interactable?
        Player player = Player.player;
        if (player != null)
        {
            if (player.interaction != null && player.interaction.current != null && player.interaction.current.IsInteractable())
            {
                panel.SetActive(true);
                hotkeyText.text = player.interaction.key.ToString();
                actionText.text = player.interaction.current.GetInteractionText();
            }
            else panel.SetActive(false);
        }
        else panel.SetActive(false);
    }
}
