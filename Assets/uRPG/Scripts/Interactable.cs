using UnityEngine;

public interface Interactable
{
    bool IsInteractable();
    string GetInteractionText();
    void OnInteract(Player player);
}
