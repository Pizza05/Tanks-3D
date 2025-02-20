using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkManager : NetworkBehaviour
{
    // Timer for HP Box spawn delay
    [Networked] private TickTimer HPboxDelay { get; set; }

    // List to store all active tanks
    public List<NetworkObject> allTanks = new List<NetworkObject>();

    // Reference to the CameraControl script to update targets
    public CameraControl camRig;

    // Prefab of the HP Box
    public HPbox HPboxPrefab;

    // Min and Max boundaries for HP Box spawn
    public Vector3 spawnAreaMin = new Vector3(-50, 0, -50);
    public Vector3 spawnAreaMax = new Vector3(50, 0, 50);

    public override void FixedUpdateNetwork()
    {
        // If the HP Box spawn timer has expired
        if (HPboxDelay.ExpiredOrNotRunning(Runner))
        {
            // Reset the spawn timer to 5 seconds
            HPboxDelay = TickTimer.CreateFromSeconds(Runner, 5.0f);

            // Generate a random spawn position within defined boundaries
            Vector3 spawnPosition = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );

            // Spawn the HP Box
            Runner.Spawn(HPboxPrefab, spawnPosition, Quaternion.identity, Object.InputAuthority);
        }
    }

    // Add a tank to the list and update camera tracking
    public void addTank(NetworkObject tank)
    {
        if (!allTanks.Contains(tank))
        {
            allTanks.Add(tank);
            UpdateCameraTargets();
        }
    }

    // Remove a tank from the list and update camera tracking
    public void removeTank(NetworkObject tank)
    {
        if (allTanks.Contains(tank))
        {
            allTanks.Remove(tank);
            UpdateCameraTargets();
        }
    }

    // Updates the camera targets based on active tanks
    private void UpdateCameraTargets()
    {
        Transform[] tanks = new Transform[allTanks.Count];

        for (int i = 0; i < allTanks.Count; i++)
        {
            tanks[i] = allTanks[i].transform;
        }

        // Update the camera to follow the active tanks
        camRig.targets = tanks;
    }

    // Deactivate the tank and start the respawn process
    public void setTankInactive(NetworkObject tank)
    {
        if (allTanks.Contains(tank))
        {
            tank.gameObject.SetActive(false);

            // Set a respawn delay of 3 seconds
            TickTimer respawnDelay = TickTimer.CreateFromSeconds(Runner, 3.0f);

            // Start coroutine to handle respawn
            StartCoroutine(respawnTank(tank, respawnDelay));
        }
    }

    // Coroutine to respawn a tank after a delay
    private IEnumerator respawnTank(NetworkObject tank, TickTimer respawnDelay)
    {
        // Wait until the respawn timer expires
        yield return new WaitUntil(() => respawnDelay.ExpiredOrNotRunning(Runner));

        // Reactivate the tank and reset its health
        tank.gameObject.SetActive(true);
        tank.GetComponent<TankHealthMP>().Respawn();
    }
}
