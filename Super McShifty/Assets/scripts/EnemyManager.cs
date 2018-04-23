using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * Manages all active enemies in game and handles their death and respawning.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{
    public class EnemyManager : MonoBehaviour
    {
        public int maxEnemyCount = 15;              // Maximum number of enemies on screen
        public int freeListSize = 20;               // Starting size of the freeNodeList in EnemyList

        EnemyList enemies;                          // All enemies on screen
        EnemySpawner[] spawners;                    // All enemy spawners
        SortedEnemyList deadRespawnList;            // List of all dead enemies waiting to respawn (sorted by time left until respawn)

        static bool canSpawnEnemies;                // Flags if the game is in a state where enemy spawning is allowed
        static uint nextEnemyId = 1;                // The next available id for enemies


        public static bool CanSpawnEnemies
        {
            get { return canSpawnEnemies; }
        }

        public static uint GetNextEnemyId()
        {
            return nextEnemyId++;
        }

        /********************************************************************
         * Look for any enemies already in the game and enemy spawners.
         * Also flag canSpawnEnemies to let EnemySpawners know if they can spawn.
         ********************************************************************/
        void Awake()
        {
            enemies = new EnemyList();
            EnemyList.AddNewFreeListNodes(freeListSize);

            Enemy[] foundEnemies = FindObjectsOfType<Enemy>();
            foreach(Enemy enemy in foundEnemies)
            {
                enemies.AddLast(enemy);
            }

            CheckCanSpawnEnemies();
            spawners = FindObjectsOfType<EnemySpawner>();
            deadRespawnList = new SortedEnemyList(new EnemyComparerRespawnTime());
        }

        // Update is called once per frame
        void Update()
        {
            UpdateRespawnTimers();
            CheckForRespawn();
        }

        /********************************************************************
         * Add a new enemy to the list of active enemies.
         * 
         * @param   enemy       Enemy that was spawned
         * @return              If the enemy was placed in the enemies list
         ********************************************************************/
        public bool TrackEnemy(Enemy enemy)
        {
            if (canSpawnEnemies)
            {
                enemies.AddLast(enemy);
                CheckCanSpawnEnemies();
                return true;
            }
            return false;
        }

        /********************************************************************
         * Checks if enemy respawns on death and places in deadRespawnList if
         * so, otherwise the object is destroyed.
         * 
         * @param   enemy       Enemy that died
         ********************************************************************/
        public void EnemyDied(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.Log("EnemyManager told null enemy died");
                return;
            }

            if (!enemies.RemoveBySearch(enemy))
            {
                Debug.Log("EnemyManager told enemy died, but it was not tracked.", enemy);
            }
            if (enemy.respawnOnDeath)
            {
                enemy.StartRespawnCountdown();
                deadRespawnList.AddSorted(enemy);
            }
            else
            {
                Destroy(enemy);
            }
        }

        /********************************************************************
         * Flag canSpawnEnemies true / false
         ********************************************************************/
        private void CheckCanSpawnEnemies()
        {
            canSpawnEnemies = (enemies.Count >= maxEnemyCount) ? false : true;
        }

        /********************************************************************
         * Tell the spawner with the shortest queue time to respawn the enemy.
         * 
         * @param   enemy       Enemy to respawn
         ********************************************************************/
        private void RespawnEnemy(Enemy enemy)
        {
            int spawnIndex = FindShortestSpawnQueue();
            spawners[spawnIndex].AddEnemyToRespawn(enemy);
        }

        /********************************************************************
         * Find the lowest queue time of all spawners.
         * 
         * @return              Index of spawner with lowest queue time
         ********************************************************************/
        private int FindShortestSpawnQueue()
        {
            int index = 0;
            float shortestTime = spawners[0].GetQueueTime();
            float time;
            for (int i = 1; i < spawners.Length; ++i)
            {
                time = spawners[i].GetQueueTime();
                if (time < shortestTime)
                {
                    shortestTime = time;
                    index = i;
                }
            }
            return index;
        }

        /********************************************************************
         * Iterate over the deadRespawnList and decrement each enemy's respawn 
         * timer by the time since the last update.
         ********************************************************************/
        private void UpdateRespawnTimers()
        {
            LinkedListNode<Enemy> current = deadRespawnList.First;
            float deltaTime = Time.deltaTime;
            while (current != null)
            {
                current.Value.UpdateRespawnCountdown(deltaTime);
                current = current.Next;
            }
        }

        private void CheckForRespawn()
        {

        }
    }
}