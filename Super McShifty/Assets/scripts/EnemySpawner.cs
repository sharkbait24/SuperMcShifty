using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * Manages spawning of enemies at its location.  It has a list of prefab units that it can spawn,
 * and after spawning, the enemy is handed over to the EnemyManager class.  If the enemy makes
 * it to an exit, the EnemyManager class will tell the spawner to respawn that enemy.
 * 
 * Once an enemy is selected to Spawn / Respawn, it is added to a queue.  The spawner will
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
        [Range(1, 100)] public int priority;        // Chance to select this unit is ( priority / sum of all priorities in list)
    }

    [System.Serializable]
    public struct SpawnRequest                      // A request in the spawnQueue to be spawned
    {
        public Enemy enemy;                         // Enemy to spawn (may already be instantiated if this is a respawn)
        public bool requiresInstantiation;          // If that enemy is a prefab that needs to be instantiated

        public SpawnRequest(Enemy enemy, bool instantiate)
        {
            this.enemy = enemy;
            requiresInstantiation = instantiate;
        }
    }

    public enum MoveDirection                       // Simple way to represent the two possible horizontal move directions
    {
        Left = -1,
        Right = 1
    }

    public class EnemySpawner : MonoBehaviour
    {
        public float firstEnemyTimer = 2f;                      // Seconds before spawning will start (will create new enemy if queue is empty)
        public float spawnTimer = 3f;                           // Seconds between spawning enemies in the queue (after first enemy)
        public float newEnemyTimer = 10f;                       // Seconds between creating a new enemy after the first
        public MoveDirection moveDirectionOnSpawn;              // Horizontal move direction for spawned unit
        [Range(0f, 1f)] public float chanceReverseDirection;    // Probability of spawned unit moving in opposite direction
        [SerializeField] List<SpawnOption> spawnableUnits;      // All enemy units that can be spawned at this spawner

        bool hasSpawnedFirstEnemy;                  // If the first enemy has been spawned
        float timeSinceSpawning;                    // Seconds since last spawn
        float timeSinceNewEnemy;                    // Seconds since last new enemy spawn
        int sumPriorities;                          // Sum of all priorities in spawnList (used as range max)
        LinkedList<SpawnRequest> spawnQueue;        // Queue of enemies waiting to be spawned
        EnemyManager enemyManager;                  // Manager that will handle enemy after spawning

        const int MAX_QUEUE_SIZE = 20;              // Maximum number of requests allowed in spawnQueue


        /********************************************************************
         * Initialization
         ********************************************************************/
        void Start()
        {
            if (spawnableUnits == null || spawnableUnits.Count.Equals(0))
                Debug.Log("EnemySpawner " + name + " has empty spawn list");

            enemyManager = SmGameManager.GetEnemyManager();
            spawnQueue = new LinkedList<SpawnRequest>();
            timeSinceSpawning = 0f;
            timeSinceNewEnemy = 0f;
            hasSpawnedFirstEnemy = false;
            SumPriorities();
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
         * 2) timeSinceNewEnemy - Adds new enemy to spawnQueue using spawnableUnits list.
         * 3) spawnTimer - Time between taking enemies off of the spawnQueue.
         ********************************************************************/
        void Update()
        {
            timeSinceSpawning += Time.deltaTime;
            timeSinceNewEnemy += Time.deltaTime;

            if (!EnemyManager.CanSpawnEnemies())
                return;

            if (!hasSpawnedFirstEnemy)
            {
                if (timeSinceSpawning >= firstEnemyTimer)
                {
                    if (spawnQueue.Count.Equals(0))
                    {
                        if (!AddNewEnemySpawn())
                            return;
                    }
                    if (SpawnNextInQueue())
                    {
                        hasSpawnedFirstEnemy = true;
                        timeSinceSpawning = 0f;
                        timeSinceNewEnemy = 0f;
                    }
                }
            }
            else
            {
                if (timeSinceNewEnemy >= newEnemyTimer && spawnQueue.Count < MAX_QUEUE_SIZE)
                {
                    if (AddNewEnemySpawn())
                        timeSinceNewEnemy = 0f;
                }
                if (timeSinceSpawning >= spawnTimer && spawnQueue.Count > 0)
                {
                    if (SpawnNextInQueue())
                        timeSinceSpawning = 0f;
                }
            }
        }

        /********************************************************************
         * Add an already existing enemy to be spawned.  Generally this is one
         * that reached an exit and was flagged to be respawned.  
         * 
         * The function will fail if spawnQueue is full.
         * 
         * @param   enemy           The enemy to add
         * @return                  Success of adding the enemy
         ********************************************************************/
        public bool AddEnemyToRespawn(Enemy enemy)
        {
            if (spawnQueue.Count >= MAX_QUEUE_SIZE)
                return false;
            spawnQueue.AddLast(new SpawnRequest(enemy, false));
            return true;
        }

        /********************************************************************
         * Add a new enemy prefab to the spawn queue, using the possible enemies
         * in the spawnableUnits list.
         * 
         * The function will fail if spawnQueue is full.
         * 
         * @return                  Success of adding the enemy
         ********************************************************************/
        private bool AddNewEnemySpawn()
        {
            if (spawnableUnits == null || spawnQueue.Count >= MAX_QUEUE_SIZE)
            {
                Debug.Log("Spawner " + name + " tried to add new enemy with max queue size");
                return false;
            }

            Enemy enemy = ChooseEnemyToSpawn();
            if (enemy == null)
                return false;
            spawnQueue.AddLast(new SpawnRequest(enemy, true));
            return true;
        }

        /********************************************************************
         * Remove the first spawn request from the queue and instantiate it
         * if necessary.  Determine the direction to send the enemy in and
         * place it in the game.  Notify the EnemyManager of the new enemy.
         * 
         * @return              Reference to the spawned enemy, or null if failed
         ********************************************************************/
        private Enemy SpawnNextInQueue()
        {
            if (spawnQueue == null || spawnQueue.Count.Equals(0))
            {
                Debug.Log("Spawner " + name + " tried to spawn from empty queue");
                return null;
            }    

            SpawnRequest request = spawnQueue.First.Value;
            spawnQueue.RemoveFirst();
            if (request.requiresInstantiation)
                request.enemy = Instantiate(request.enemy);

            float moveDirection = (float)moveDirectionOnSpawn;
            if (Random.Range(0f, 1f) < chanceReverseDirection)
                moveDirection *= -1f;
            request.enemy.Spawn(transform.position, moveDirection);  // Spawn initializes enemy activity and movement
            enemyManager.TrackEnemy(this, request.enemy);
            return request.enemy;
        }

        /********************************************************************
         * Selects an enemy to spawn by choosing a random number between 1 and
         * sumPriorities.  We can then go through the list adding up the
         * priorities until we meet or exeed the random number.
         * 
         * @return                  The enemy to spawn
         ********************************************************************/
        private Enemy ChooseEnemyToSpawn()
        {
            int targetSum = Random.Range(1, sumPriorities + 1);  // add 1 since max is exclusive
            int currentSum = 0;

            for(int i = 0; i < spawnableUnits.Count; ++i)
            {
                currentSum += spawnableUnits[i].priority;
                if (currentSum >= targetSum)
                    return spawnableUnits[i].enemyPrefab;
            }

            // We should never get here
            Debug.Log("Spawner " + name + " failed to select enemy from spawnableUnits.");
            return null;
        }

        /********************************************************************
         * Adds up all priority values in spawnList.  This total is used to
         * create a max range for the random selection
         ********************************************************************/
        private void SumPriorities()
        {
            sumPriorities = 0;
            spawnableUnits.ForEach(delegate (SpawnOption option) 
            {
                sumPriorities += option.priority;
            });
        }
    }
}