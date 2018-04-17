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
        static float screenBoundX;              // Half the width of the camera in Unity units
        static float screenBoundY;              // Half the height of the camera in Unity units
        static Player player;                   // The player in the game
        static EnemyManager enemyManager;       // The manager for all enemies in the game

        public static float GetScreenBoundX()
        {
            return screenBoundX;
        }

        public static float GetScreenBoundY()
        {
            return screenBoundY;
        }

        public static Player GetPlayer()
        {
            return player;
        }

        public static EnemyManager GetEnemyManager()
        {
            return enemyManager;
        }


        /********************************************************************
         * Perform Game Manager initialization first so other classes can
         * access it in their Start() initialization.
         ********************************************************************/
        void Awake()
        {
            m_Camera = Camera.main;
            screenBoundY = m_Camera.orthographicSize;
            screenBoundX = m_Camera.aspect * screenBoundY;
            
            player = FindObjectOfType<Player>();
            if (player == null)
                throw new MissingComponentException("Failed to find \"Player\".");

            enemyManager = FindObjectOfType<EnemyManager>();
            if (enemyManager == null)
                throw new MissingComponentException("Failed to find \"EnemyManager\".");
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /********************************************************************
         * Returns if the position is within the camera frame
         ********************************************************************/
        public static bool IsInCameraFrame(Vector3 position)
        {
            if (position.x > screenBoundX || position.x < -screenBoundX ||
                position.y > screenBoundY || position.y < -screenBoundY)
                return false;
            return true;
        }

        /********************************************************************
         * Takes the current position and returns the position after wrap around
         * is applied.  i.e. If they go off the left side of the screen, make
         * them appear on the right at the same height.
         * 
         * @param   currentPosition     Current position of object
         * @return                      A new position with the wrap around applied
         ********************************************************************/
        public static Vector3 PositionAfterWrapAround(Vector3 currentPosition)
        {
            Vector3 newPosition = currentPosition;

            if (currentPosition.x < -screenBoundX)
                newPosition.x = screenBoundX + (currentPosition.x + screenBoundX);
            else if (currentPosition.x > screenBoundX)
                newPosition.x = -screenBoundX + (currentPosition.x - screenBoundX);

            if (currentPosition.y < -screenBoundY)
                newPosition.y = screenBoundY + (currentPosition.y + screenBoundY);
            else if (currentPosition.y > screenBoundY)
                newPosition.y = -screenBoundY + (currentPosition.y - screenBoundY);

            return newPosition;
        }
    }
}