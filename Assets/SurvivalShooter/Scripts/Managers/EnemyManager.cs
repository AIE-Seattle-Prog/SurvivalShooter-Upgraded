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
    public float initialSpawnDelay = 1f;
    public float minimumSpawnDelay = 1f;            // How long between each spawn.
    public float minimumWaveDelay = 5f;
    public int spawnsPerWave = 5;
    public int maxActiveEnemies = 30;
    public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.

    public bool IsCurrentlySpawning { get; private set; }

    private CancellationTokenSource spawnerCancellationSource;

    // TODO: replace this with the 'enemiesSpawned' hashset
    private int playerTargetSelectionIndex;
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
    private HashSet<EnemyController> enemiesSpawned;
    public int EnemiesRemaining => EnemyQuota - enemiesKilled;

    public void SetSpawnerConfig(EnemyRoundConfig config)
    {
        spawnerConfig = config;
    }

    public void ResetSpawnCounts()
    {
        enemySpawnCount = 0;
        enemiesKilled = 0;
        enemiesSpawned.Clear();
    }

    private async Task<bool> DoSpawnWaveInterval(CancellationToken cancelToken)
    {
        try
        {
            bool isFirstWave = true;

            while (enemySpawnCount < EnemyQuota)
            {
                if (isFirstWave)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(initialSpawnDelay), cancellationToken: cancelToken);
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(minimumWaveDelay), cancellationToken: cancelToken);
                }

                int spawnCountThisWave = Mathf.Min(EnemyQuota - enemySpawnCount, spawnsPerWave);
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
                GameObject nextEnemyPrefab = null;
                if(!spawnerConfig.GetPlannedEnemy(enemySpawnCount, out nextEnemyPrefab))
                {
                    nextEnemyPrefab = spawnerConfig.GetRandomEnemy();
                }

                // do we need to wait for room for another enemy
                while (maxActiveEnemies != -1 &&       // no limit
                    EnemyCount >= maxActiveEnemies)  // configured limit
                {
                    Debug.Log("Spawn blocked by enemy limit. Waiting one spawn cycle...");
                    // TODO: find a better way to wait to spawn the next enemy
                    await UniTask.Delay(TimeSpan.FromSeconds(minimumSpawnDelay), cancellationToken: cancelToken);
                }

                // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
                SpawnEnemy(nextEnemyPrefab, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);

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

    private EnemyController SpawnEnemy(GameObject enemyPrefab, Vector3 position, Quaternion rotation)
    {
        var babyEnemy = Instantiate(enemyPrefab, position, rotation);
        ++EnemyCount;
        ++enemySpawnCount;

        EnemyController newEnemy = babyEnemy.GetComponent<EnemyController>();

        playerTargetSelectionIndex = (playerTargetSelectionIndex + 1) % PlayerManagerSystem.PlayerCount;

        newEnemy.TargetPlayer = PlayerManagerSystem.GetPlayer(playerTargetSelectionIndex).GetComponent<PlayerCharacter>();
        enemiesSpawned.Add(newEnemy);
        newEnemy.Health.OnDeath.AddListener(HandleEnemyDeath);

        return newEnemy;
    }

    private void HandleEnemyDeath(EnemyHealth enemy)
    {
        ++enemiesKilled;
        --EnemyCount;
        enemiesSpawned.Remove(enemy.Controller);
    }

    private void RefreshCancellationTokenSource()
    {
        if(spawnerCancellationSource != null)
        {
            spawnerCancellationSource.Cancel();
            spawnerCancellationSource.Dispose();
            spawnerCancellationSource = null;
        }

        spawnerCancellationSource = new CancellationTokenSource();
    }

    private void Awake()
    {
        // HACK: pre-allocate hashset w/ capacity requires creating a dummy list w/ a given capacity
        //       
        //       capacity will be retained after a clear
        //       https://stackoverflow.com/questions/6771917/why-cant-i-preallocate-a-hashsett
        enemiesSpawned = new HashSet<EnemyController>(new List<EnemyController>(maxActiveEnemies));
        enemiesSpawned.Clear();
    }

    private async void OnEnable()
    {
        RefreshCancellationTokenSource();

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
            spawnerCancellationSource = null;
        }
    }
}