using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * Manages spawning of enemies at its location.  It has a list of prefab units that it,
 * instantiates at the start of the game and then sets them inactive until placed in game.  
 * When an enemy is selected to Spawn / Respawn, it is added to a queue.  The spawner will
 * actually spawn the unit once the spawnTimer has gone off.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{
     [System.Serializable]
    public struct SpawnOption                       // Holds one possible enemy that can be spawned by the spawner
    {
        public Enemy enemyPrefab;                   // Enemy to instantiate
        [Range(1, 20)] public int quantity;         // Number of enemies placed in spawnPool
    }

    public enum MoveDirection                       // Simple way to represent the two possible horizontal move directions
    {
        Left = -1,
        Right = 1
    }


    public class EnemySpawner : MonoBehaviour
    {
        public float firstEnemyTime = 2f;                       // Seconds before spawning will start (will create new enemy if queue is empty)
        public float spawnTime = 3f;                            // Seconds between spawning enemies in the queue (after first enemy)
        public float newEnemyTime = 10f;                        // Seconds between creating a new enemy after the first
        public MoveDirection moveDirectionOnSpawn;              // Horizontal move direction for spawned unit
        [Range(0f, 1f)] public float chanceReverseDirection;    // Probability of spawned unit moving in opposite direction
        [SerializeField] List<SpawnOption> spawnPoolCreator;    // The list and quantity of enemies to place in the spawn pool

        bool hasSpawnedFirstEnemy;                  // If the first enemy has been spawned
        float timeSinceSpawning;                    // Seconds since last spawn
        float timeSinceNewEnemy;                    // Seconds since last new enemy spawn
        int sumPriorities;                          // Sum of all priorities in spawnList (used as range max)
        List<Enemy> spawnPool;                      // All enemies that this spawner has left to create
        EnemyList spawnQueue;                       // Queue of enemies waiting to be spawned
        
        static EnemyManager enemyManager;           // Manager that will handle enemy after spawning


        /********************************************************************
         * Initialization and creation of spawn pool
         ********************************************************************/
        void Start()
        {
            spawnQueue = new EnemyList();
            if (enemyManager == null)
                enemyManager = SmGameManager.GetEnemyManager;

            timeSinceSpawning = 0f;
            timeSinceNewEnemy = 0f;
            hasSpawnedFirstEnemy = false;

            if (spawnPoolCreator.Count.Equals(0))
            {
                Debug.Log("Spawner has empty spawn list", this);
                spawnPool = new List<Enemy>();
            }
            else
            {
                BuildSpawnPool();
            }
        }

        /********************************************************************
         * Create all enemies that will be spawned as new enemies by this 
         * spawner and place in spawn pool.  The enemies will be inactive 
         * until spawned.
         ********************************************************************/
        private void BuildSpawnPool()
        {
            int poolSize = 0;
            foreach (SpawnOption option in spawnPoolCreator)
            {
                poolSize += option.quantity;
            }

            spawnPool = new List<Enemy>(poolSize);

            foreach (SpawnOption option in spawnPoolCreator)
            {
                for (int i = 0; i < option.quantity; ++i)
                {
                    Enemy enemy = Instantiate(option.enemyPrefab);
                    enemy.Init();
                    enemy.gameObject.SetActive(false);
                    spawnPool.Add(enemy);
                }
            }
        }

        /********************************************************************
         * The spawner runs on 3 timers to determine spawning of units. 
         * Units are added to the spawnQueue periodically either as new enemies
         * through the firstEnemyTimer and timeSinceNewEnemy timers, or as respawns
         * through the AddEnemyToRespawn() function.  Spawning of units is simply
         * taking the unit off the spawnQueue and placing it in the game.
         * 
         * 1) firstEnemyTimer - Spawns the first enemy (either new or from the spawnQueue)
         * -- The other 2 only start working after firstEnemyTimer --
         * 2) timeSinceNewEnemy - Adds new enemy to spawnQueue from the spawnPool.
         * 3) spawnTimer - Time between placing enemies in game from the spawnQueue.
         ********************************************************************/
        void Update()
        {
            timeSinceSpawning += Time.deltaTime;
            timeSinceNewEnemy += Time.deltaTime;

            if (ReadyToAddNewEnemyToQueue())
            {
                AddNewEnemySpawn();
                timeSinceNewEnemy = 0f;
            }
            if (ReadyToSpawnEnemy())
            {
                if (SpawnNextInQueue())
                {
                    hasSpawnedFirstEnemy = true;
                    timeSinceSpawning = 0f;
                }
            }
        }

        /********************************************************************
         * Calculate the time it would take an enemy being added to the queue
         * to be spawned.  Note: It is possible for the timeSinceSpawning to
         * be > spawnTimer if the EnemyManager is at the enemy on screen limit.
         * 
         * Time = time to spawn next enemy + time to spawn all others in queue (plus added enemy)
         * 
         * @return              Time an enemy would wait in the queue before spawning
         ********************************************************************/
        public float GetQueueTime()
        {
            if (spawnQueue.Count.Equals(0))
                return 0f;
            return spawnTime - Mathf.Clamp(timeSinceSpawning, 0f, spawnTime) + (spawnQueue.Count * spawnTime);
        }

        /********************************************************************
         * Add an already existing enemy to be spawned.  Generally this is one
         * that reached an exit and was flagged to be respawned.  
         * 
         * @param   enemy           The enemy to add
         ********************************************************************/
        public void AddEnemyToRespawn(Enemy enemy)
        {
            spawnQueue.AddLast(enemy);
        }

        /********************************************************************
         * Checks if there is an available enemy to add and if it is time to
         * do so.  If we are ready to spawn the first enemy, this will check
         * if a new enemy needs to be added to the spawnQueue.
         * 
         * @return              If a new enemy can be added to the spawnQueue
         ********************************************************************/
        private bool ReadyToAddNewEnemyToQueue()
        {
            if (spawnPool.Count.Equals(0))
                return false;

            if (hasSpawnedFirstEnemy)
            {
                if (timeSinceNewEnemy >= newEnemyTime)
                    return true;
            }
            else if (timeSinceSpawning >= firstEnemyTime && spawnQueue.Count.Equals(0))
            {
                return true;
            }
            return false;
        }

        /********************************************************************
         * Checks if there is an available enemy to spawn and if it is time to
         * do so.  Checks against firstEnemyTime for the first spawn and
         * spawnTime for all remaining spawns.
         * 
         * @return              If an enemy can be spawned
         ********************************************************************/
        private bool ReadyToSpawnEnemy()
        {
            if (!EnemyManager.CanSpawnEnemies || spawnQueue.Count <= 0)
                return false;

            if (hasSpawnedFirstEnemy)
            {
                if (timeSinceSpawning >= spawnTime)
                    return true;
            }
            else if (timeSinceSpawning >= firstEnemyTime)
            {
                return true;
            }
            return false;
        }

        /********************************************************************
         * Remove an enemy from the spawnPool and add it to the spawnQueue.
         * 
         * @return                  Success of adding the enemy
         * @throws EnemySpawnerFailed   When ChooseEnemyToSpawn fails to select from spawnPool
         ********************************************************************/
        private void AddNewEnemySpawn()
        {
            Enemy enemy = ChooseEnemyToSpawn();
            spawnQueue.AddLast(enemy);
        }

        /********************************************************************
         * Remove the first enemy from the queue, determine the direction to 
         * send it in and places it in the game.  Notifies the EnemyManager 
         * of the new enemy.
         * 
         * @return              Success of spawning enemy
         ********************************************************************/
        private bool SpawnNextInQueue()
        {
            Enemy enemy = spawnQueue.RemoveFirst();
            float moveDirection = (float)moveDirectionOnSpawn;
            if (Random.Range(0f, 1f) < chanceReverseDirection)
                moveDirection *= -1f;
            enemy.Spawn(transform.position, moveDirection);  // Spawn initializes enemy activity and movement

            if (enemyManager.TrackEnemy(enemy))
            {
                return true;
            }
            else
            {
                Debug.Log("Spawner tried to add new enemy to full game", this);
                enemy.gameObject.SetActive(false);
                spawnQueue.AddFirst(enemy);
                return false;
            }
        }

        /********************************************************************
         * Selects an enemy to spawn by choosing a random enemy from the spawnPool.
         * 
         * @return                  The enemy to spawn
         ********************************************************************/
        private Enemy ChooseEnemyToSpawn()
        {
            int targetEnemy = Random.Range(0, spawnPool.Count);  // add 1 since max is exclusive
            Enemy enemy = spawnPool[targetEnemy];
            spawnPool.RemoveAt(targetEnemy);
            return enemy;
        }
    }
}