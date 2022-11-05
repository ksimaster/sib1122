using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject player;
    private UnityEngine.AI.NavMeshAgent NMA;

    void Start()
    {
        NMA = (UnityEngine.AI.NavMeshAgent)this.GetComponent("NavMeshAgent");
    }
    void Update()
    {
        NMA.SetDestination(player.transform.position);
    }
}
