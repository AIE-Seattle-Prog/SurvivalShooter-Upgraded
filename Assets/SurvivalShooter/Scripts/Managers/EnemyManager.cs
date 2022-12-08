using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemies;                // The enemy prefab to be spawned.
    public float minimumSpawnDelay = 1f;            // How long between each spawn.
    public float minimumWaveDelay = 5f;
    public int spawnWaveCount = 5;
    public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.

    public bool IsCurrentlySpawning { get; private set; }

    private CancellationTokenSource spawnerCancellationSource;

    private int enemyCount;
    public int EnemyCount
    {
        get => enemyCount;
        set
        {
            if(enemyCount == value) { return; }
            enemyCount = value;
            OnEnemyCountChanged.Invoke(enemyCount);
        }
    }

    [field: SerializeField]
    public UnityEvent<int> OnEnemyCountChanged { get; private set; }

    private void OnEnable()
    {
        spawnerCancellationSource = new CancellationTokenSource();

        DoSpawnWaveInterval(spawnerCancellationSource.Token);
    }

    private void OnDisable()
    {
        if (spawnerCancellationSource != null)
        {
            spawnerCancellationSource.Cancel();
            spawnerCancellationSource.Dispose();
        }
    }

    private async void DoSpawnWaveInterval(CancellationToken cancelToken)
    {
        try
        {
            while (!cancelToken.IsCancellationRequested)
            {
                DoSpawnWave(spawnWaveCount, cancelToken);
                await UniTask.WaitUntil(() => !IsCurrentlySpawning, cancellationToken: cancelToken);
                await UniTask.Delay(TimeSpan.FromSeconds(minimumWaveDelay), cancellationToken: cancelToken);
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
            Debug.Log("New wave is spawning...");

            // Find a random index between zero and one less than the number of spawn points.
            int spawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Length);

            // Begin wave spawn at selected spawnpoint
            for (int i = 0; i < spawnCount; ++i)
            {
                int enemyIndex = UnityEngine.Random.Range(0, enemies.Length);

                // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
                SpawnEnemy(enemies[enemyIndex], spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);

                await UniTask.Delay(TimeSpan.FromSeconds(minimumSpawnDelay), cancellationToken: cancelToken);
            }

            IsCurrentlySpawning = false;
            Debug.Log("Spawn wave completed.");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Spawn Wave cancelled.");
        }
    }

    private EnemyHealth SpawnEnemy(GameObject enemyPrefab, Vector3 position, Quaternion rotation)
    {
        var babyEnemy = Instantiate(enemyPrefab, position, rotation);
        ++EnemyCount;

        EnemyHealth enemyHealth = babyEnemy.GetComponent<EnemyHealth>();
        enemyHealth.OnDeath.AddListener(HandleEnemyDeath);

        return enemyHealth;
    }

    private void HandleEnemyDeath()
    {
        --EnemyCount;
    }
}