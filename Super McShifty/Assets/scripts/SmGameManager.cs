using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * Manages the game and handles loosing and victory conditions.  Also provides some useful
 * functionality related to the visible screen space that are used by movement classes.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{
    public enum LayerMaskValues                 // Bit masks for each custom layer that is used in the game
    {
        Player = 256,
        Enemy = 512,
        Environment = 1024,
        Background = 2048,
        BackgroundEnemy = 4096
    }

    public class SmGameManager : MonoBehaviour
    {
        Camera m_Camera;                        // Main camera for game
        static ScreenBounds screenBounds;       // Contains game's screen bounds based on camera position and settings
        static Player player;                   // The player in the game
        static EnemyManager enemyManager;       // The manager for all enemies in the game

        public static ScreenBounds GetScreenBounds
        {
            get { return screenBounds; }
        }

        public static Player GetPlayer
        {
            get { return player; }
        }

        public static EnemyManager GetEnemyManager
        {
            get { return enemyManager; }
        }


        /********************************************************************
         * Perform Game Manager initialization first so other classes can
         * access it in their Start() initialization.
         ********************************************************************/
        void Awake()
        {
            m_Camera = Camera.main;
            screenBounds = new ScreenBounds(m_Camera);
            
            player = FindObjectOfType<Player>();
            if (player == null)
                throw new MissingComponentException("Failed to find \"Player\".");

            enemyManager = FindObjectOfType<EnemyManager>();
            if (enemyManager == null)
                throw new MissingComponentException("Failed to find \"EnemyManager\".");

            Physics2D.IgnoreLayerCollision(9, 9, true);  // Disable collisions between enemies
        }

        /********************************************************************
         * Called after update, so that all units and camera have finished moving
         * before we potentially change the screen bounds.
         ********************************************************************/
        void LateUpdate()
        {
            screenBounds.CheckBounds();
        }
    }
}