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

        float horizontalMoveDirection;              // The current direction the enemy is heading in

        /********************************************************************
         * Initialization of Unit base class
         ********************************************************************/
        void Start()
        {
            base.Init();
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
            throw new System.NotImplementedException();
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
            collider.enabled = true;
            state = State.Active;
        }

        /********************************************************************
         * Move in direction already established.  This will only change if
         * collision with another enemy unit happens.
         ********************************************************************/
        private void GroundMove()
        {
            unitMover.Move(horizontalMoveDirection, false);
        }

        /********************************************************************
         * Damage the player on collision.
         ********************************************************************/
        public void CollisionWithPlayer(Unit player)
        {
            player.TakeDamage(damageOnContact);
        }

        /********************************************************************
         * Change direction of movement on collision with enemy unit.
         ********************************************************************/
        public void CollisionWithEnemy(Unit enemy)
        {
            horizontalMoveDirection *= -1;
        }
    }
}