using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * General enemy class that will perform general movement and spawning.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{ 
    


    [RequireComponent(typeof(UnitMover))]

    public class Enemy : Unit
    {
        public int damageOnContact = 1;             // Damage dealt to player on contact
        public bool respawnOnExit = true;           // Respawn enemy on reaching an exit
        public bool respawnOnDeath = false;         // Respawn enemy after it dies
        public float respawnOnDeathTime = 5.0f;     // Time to respawn after dying (if respawnOnDeath is true)

        float timeTillDeathRespawn = 0f;            // Time left until respawn after dying
        float horizontalMoveDirection;              // The current direction the enemy is heading in
        uint id;                                    // Unique ID for the enemy
        Enemy defaultTemplate;                      // An instance of the prefab enemy this was instantiated from (for reseting default values)

        public uint Id
        {
            get { return id; }
        }

        public float TimeTillDeathRespawn
        {
            get { return timeTillDeathRespawn; }
        }

        /********************************************************************
         * Initialization of Unit base class
         ********************************************************************/
        void Start()
        {
            Init();
        }

        public override void Init()
        {
            base.Init();
            unitMover.Init();
            id = EnemyManager.GetNextEnemyId();
        }

        /********************************************************************
         * Move enemy.  Other components will handle ai related to the player.
         ********************************************************************/
        protected override void UpdateActive()
        {
            GroundMove();
        }


        protected override void UpdateDead()
        {
            gameObject.SetActive(false);
            SmGameManager.GetEnemyManager.EnemyDied(this);
        }

        protected override void NonUnitCollision(Collision2D collision)
        {
            
        }

        /********************************************************************
         * Place an enemy at a location and set it active.
         * 
         * @param   position        New position of enemy
         * @param   horizontalMove  Direction of movement [-1, 1] along x axis
         ********************************************************************/
        public void Spawn(Vector3 position, float horizontalMove)
        {
            transform.position = position;
            horizontalMoveDirection = Mathf.Clamp(-1f, horizontalMove, 1f);
            state = State.Active;
            gameObject.SetActive(true);
            unitCollider.enabled = true;
        }

        /********************************************************************
         * Initialize timeTillDeathRespawn
         ********************************************************************/
        public void StartRespawnCountdown()
        {
            timeTillDeathRespawn = respawnOnDeathTime;
        }

        /********************************************************************
         * Decrement timeTillDeathRespawn.
         * 
         * @param   timeSinceLastUpdate     Time since last frame update
         ********************************************************************/
        public void UpdateRespawnCountdown(float timeSinceLastUpdate)
        {
            timeTillDeathRespawn -= timeSinceLastUpdate;
        }

        /********************************************************************
         * Damage the player on collision.
         ********************************************************************/
        public void CollisionWithPlayer(Unit player)
        {
            player.TakeDamage(damageOnContact);
        }

        /********************************************************************
         * Move in direction already established.  This will only change if
         * collision with another enemy unit happens.
         ********************************************************************/
        private void GroundMove()
        {
            unitMover.Move(horizontalMoveDirection, false);
        }
    }
}