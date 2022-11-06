using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableEnable : MonoBehaviour
{
    public GameObject DisableObject;
    


    private void OnEnable()
    {
        DisableObject.SetActive(false);
    }



}
