using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * Manages all enemies in game and handles their death and spawning.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{
    public class EnemyManager : MonoBehaviour
    {
        public int maxEnemyCount = 15;              // Maximum number of enemies on screen

        static bool canSpawnEnemies;                // Flags if the game is in a state where enemy spawning is allowed

        public static bool CanSpawnEnemies()
        {
            return canSpawnEnemies;
        }

        // Use this for initialization
        void Start()
        {
           
            canSpawnEnemies = true;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TrackEnemy(EnemySpawner spawner, Enemy enemy)
        {

        }
    }
}