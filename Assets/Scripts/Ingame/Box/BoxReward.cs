using UnityEngine;

public class BoxStarReward : MonoBehaviour, IBoxReward
{
    [SerializeField] private GameObject starPrefab;

    public void SpawnReward(Vector3 position)
    {
        Instantiate(starPrefab, position, Quaternion.identity);
    }
}