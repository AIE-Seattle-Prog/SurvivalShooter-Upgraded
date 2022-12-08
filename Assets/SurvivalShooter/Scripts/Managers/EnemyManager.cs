using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemies;                // The enemy prefab to be spawned.
    public float minimumSpawnDelay = 1f;            // How long between each spawn.
    public float minimumWaveDelay = 5f;
    public int spawnWaveCount = 5;
    public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.

    public bool IsCurrentlySpawning { get; private set; }

    private CancellationTokenSource spawnerCancellationSource;

    private void OnEnable()
    {
        spawnerCancellationSource = new CancellationTokenSource();

        DoSpawnInterval(spawnerCancellationSource.Token);
    }

    private void OnDisable()
    {
        if (spawnerCancellationSource != null)
        {
            spawnerCancellationSource.Cancel();
            spawnerCancellationSource.Dispose();
        }
    }

    private async void DoSpawnInterval(CancellationToken cancelToken)
    {
        try
        {
            while (!cancelToken.IsCancellationRequested)
            {
                DoSpawnWave(spawnWaveCount, cancelToken);
                await UniTask.WaitUntil(() => !IsCurrentlySpawning, cancellationToken: cancelToken);
                await UniTask.Delay(TimeSpan.FromSeconds(minimumSpawnDelay), cancellationToken: cancelToken);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Spawn Interval cancelled.");
        }
    }

    private async void DoSpawnWave(int spawnCount, CancellationToken cancelToken)
    {
        try
        {
            IsCurrentlySpawning = true;

            // Find a random index between zero and one less than the number of spawn points.
            int spawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Length);

            // Begin wave spawn at selected spawnpoint
            for (int i = 0; i < spawnCount; ++i)
            {
                int enemyIndex = UnityEngine.Random.Range(0, enemies.Length);

                // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
                Instantiate(enemies[enemyIndex], spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);

                await UniTask.Delay(TimeSpan.FromSeconds(minimumSpawnDelay), cancellationToken: cancelToken);
            }

            IsCurrentlySpawning = false;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Spawn Wave cancelled.");
        }
    }
}