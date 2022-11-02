using UnityEngine;

public class DropRandomItemsOnDeath : MonoBehaviour
{
    [Header("Drop Radius")]
    public float radiusMultiplier = 1;
    public int dropSolverAttempts = 3; // attempts to drop without being behind a wall, etc.

    [Header("Items")]
    public ItemDropChance[] dropChances;

    [Header("Gold")]
    public int goldMin = 0;
    public int goldMax = 10;
    public GoldDrop goldDrop;

    void DropItemAtRandomPosition(GameObject dropPrefab)
    {
        // drop at random point on navmesh that is NOT behind a wall
        // -> dropping behind a wall is just bad gameplay
        // -> on navmesh because that's the easiest way to find the ground
        //    without accidentally raycasting ourselves or something else
        Vector3 position = Utils.ReachableRandomUnitCircleOnNavMesh(transform.position, radiusMultiplier, dropSolverAttempts);

        // drop
        Instantiate(dropPrefab, position, Quaternion.identity);
    }

    public void OnDeath()
    {
        // drop items
        foreach (ItemDropChance itemChance in dropChances)
            if (Random.value <= itemChance.probability)
                DropItemAtRandomPosition(itemChance.drop.gameObject);

        // drop gold
        int gold = Random.Range(goldMin, goldMax);
        if (gold > 0)
        {
            // drop at random point on navmesh that is NOT behind a wall
            // -> dropping behind a wall is just bad gameplay
            // -> on navmesh because that's the easiest way to find the ground
            //    without accidentally raycasting ourselves or something else
            Vector3 position = Utils.ReachableRandomUnitCircleOnNavMesh(transform.position, radiusMultiplier, dropSolverAttempts);
            GameObject go = Instantiate(goldDrop.gameObject, position, Quaternion.identity);
            go.GetComponent<GoldDrop>().gold = gold;
        }
    }
}
