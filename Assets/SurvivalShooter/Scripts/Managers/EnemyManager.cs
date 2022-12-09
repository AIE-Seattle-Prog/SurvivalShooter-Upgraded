using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
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
    private int enemySpawnCount = 0;

    [field: SerializeField]
    public UnityEvent<int> OnEnemyCountChanged { get; private set; }

    [field: SerializeField]
    public UnityEvent<int> OnEnemyQuotaMet { get; private set; }

    public int EnemyQuota => spawnerConfig.numberOfEnemies;
    public bool IsEnemyQuotaMet => enemySpawnCount >= EnemyQuota;
    private EnemyRoundConfig spawnerConfig;
    
    // TODO: figure this out from enemyCount
    private int enemiesKilled;
    public int EnemiesRemaining => EnemyQuota - enemiesKilled;

    public void SetSpawnerConfig(EnemyRoundConfig config)
    {
        spawnerConfig = config;
    }

    public void ResetCounters()
    {
        enemySpawnCount = 0;
        enemiesKilled = 0;
    }

    private async Task<bool> DoSpawnWaveInterval(CancellationToken cancelToken)
    {
        try
        {
            bool isFirstWave = true;

            while (enemySpawnCount < EnemyQuota)
            {
                if (!isFirstWave)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(minimumWaveDelay), cancellationToken: cancelToken);
                }

                int spawnCountThisWave = Mathf.Min(EnemyQuota - enemySpawnCount, spawnWaveCount);
                bool waveSpawnSuccess = await DoSpawnWave(spawnCountThisWave, cancelToken);
                isFirstWave = false;
            }

            Debug.Log("Quota met, ending spawn wave complete.");
            return true;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Spawn Interval cancelled.");
            return false;
        }
    }

    private async Task<bool> DoSpawnWave(int spawnCount, CancellationToken cancelToken)
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
                // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
                SpawnEnemy(spawnerConfig.GetNextEnemy(), spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);

                await UniTask.Delay(TimeSpan.FromSeconds(minimumSpawnDelay), cancellationToken: cancelToken);
            }

            IsCurrentlySpawning = false;
            Debug.Log("Spawn wave completed.");
            return true;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Spawn Wave cancelled.");
            return false;
        }
    }

    private EnemyHealth SpawnEnemy(GameObject enemyPrefab, Vector3 position, Quaternion rotation)
    {
        var babyEnemy = Instantiate(enemyPrefab, position, rotation);
        ++EnemyCount;
        ++enemySpawnCount;

        EnemyHealth enemyHealth = babyEnemy.GetComponent<EnemyHealth>();
        enemyHealth.OnDeath.AddListener(HandleEnemyDeath);

        return enemyHealth;
    }

    private void HandleEnemyDeath()
    {
        ++enemiesKilled;
        --EnemyCount;
    }

    private async void OnEnable()
    {
        spawnerCancellationSource = new CancellationTokenSource();

        bool result = await DoSpawnWaveInterval(spawnerCancellationSource.Token);
        if (result)
        {
            OnEnemyQuotaMet.Invoke(EnemyQuota);
        }
    }

    private void OnDisable()
    {
        if (spawnerCancellationSource != null)
        {
            spawnerCancellationSource.Cancel();
            spawnerCancellationSource.Dispose();
        }
    }
}