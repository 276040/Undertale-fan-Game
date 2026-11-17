
using UnityEngine;

public class PlayerSpawnPosition : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject player;

    [Header("Spawn Location")]
    public Transform spawnPoint;

    [Header("Options")]
    public bool setPositionOnStart = true;

    private void Start()
    {
        if (!setPositionOnStart)
            return;

        if (player == null)
        {
            Debug.LogError("Player Spawn: Player is not assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Player Spawn: Spawn Point is not assigned!");
            return;
        }

        player.transform.position = spawnPoint.position;
    }
}
