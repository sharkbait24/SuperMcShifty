﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * An abstract base class for all units.  Provides the basic state logic and Update loop.
 * Basic functionality for dying is implemented, but other states require the derived
 * classes to implement their own versions of the state functions.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{
    public enum UnitType                                    // Defines all possible units in the game
    {
        Player,                                             // The player
        Friendly,                                           // Ai units that help the player
        Enemy                                               // Units that can hurt the player
    }

    public enum State                                       // Current state of the unit
    {
        Active = 0,                                         // Typical state for an unit in the game
        Dying,                                              // Identifies that the unit is currently in its "death animation"
        Dead,                                               // Death animation complete, but unit still requires cleanup
        Inactive                                            // Unit is not currently being used in the game
    }

    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]

    public abstract class Unit : MonoBehaviour
    {
        public UnitType unitType;
        public State state;
        public int health = 1;                              // Hit points of the unit
        public float deathAnimationIterationTime = 1f;      // Time a single flash on and off of the sprite takes
        public float deathAnimationTotalTime = 2f;          // Total time the death animation lasts
        protected float deathAnimationElapsedTime;          // Amount of time spent in the animation
        protected UnitMover unitMover;                      // Optional mover component to move game object

        SpriteRenderer spriteRenderer;                      // Sprite renderer on this object
        new Collider2D collider;                            // Collider on this object



        /********************************************************************
         * Initialization
         ********************************************************************/
        protected void Init()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (collider == null)
                collider = GetComponent<Collider2D>();
            if (unitMover == null)
                unitMover = GetComponent<UnitMover>();
            deathAnimationElapsedTime = 0.0f;
        }

        /********************************************************************
         * Main update for the object.  Calls functions based on the state the
         * object is in.  Derived classes should override the functions as needed.
         ********************************************************************/
        void FixedUpdate()
        {
            switch (state)
            {
                case State.Active:
                    UpdateActive();
                    break;
                case State.Dying:
                    UpdateDying();
                    break;
                case State.Dead:
                    UpdateDead();
                    break;
                case State.Inactive:
                    UpdateInactive();
                    break;
                default:
                    Debug.Log("Object \"" + gameObject.name + "\" in unkown State (" + state + ")");
                    break;
            }
        }

        /********************************************************************
         * Update logic for an active object
         ********************************************************************/
        protected abstract void UpdateActive();

        /********************************************************************
        * A generic "death animation" which causes the sprite to flash on and
        * off for a few seconds.  The sprite is on for the first half of the
        * deathAnimationIterationTime and off for the last half.
        * This repeats until the deathAnimationTotalTime is reached.
        ********************************************************************/
        protected virtual void UpdateDying()
        {
            deathAnimationElapsedTime += Time.deltaTime;
            if (deathAnimationElapsedTime >= deathAnimationTotalTime)
            {
                deathAnimationTotalTime = 0.0f;
                state = State.Dead;
            }
            else
            {
                if (deathAnimationElapsedTime % deathAnimationIterationTime < 0.5f * deathAnimationIterationTime)
                    spriteRenderer.enabled = true;
                else
                    spriteRenderer.enabled = false;
            }
        }

        /********************************************************************
         * Logic for handling and object after it has finished its
         * "death animation"
         ********************************************************************/
        protected abstract void UpdateDead();

        /********************************************************************
         * Do nothing with inactive units, but allows for derived classes to use it.
         ********************************************************************/
        protected virtual void UpdateInactive() { }

        /********************************************************************
         * Place the object in the dying state and disable the collider to 
         * prevent collisions with other objects while in the dying animation.
         ********************************************************************/
        public virtual void Kill()
        {
            if (state == State.Active)
            {
                state = State.Dying;
                collider.enabled = false;
            }
        }

        /********************************************************************
         * Take a positive amount of damage that is subtracted from the unit's
         * health.  Calls Kill() if no health left.
         * 
         * @param   amount          A number greater than 0
         ********************************************************************/
        public virtual void TakeDamage(int amount)
        {
            if (amount <= 0)
                return;

            health -= amount;
            if (health <= 0)
                Kill();
        }

        /********************************************************************
         * Take a positive amount of damage and knockback the unit.
         * 
         * @param   amount          A number greater than 0
         * @param   knockback       Force of the knockback
         ********************************************************************/
        public virtual void TakeDamage(int amount, Vector2 knockback)
        {
            TakeDamage(amount);
            if (state != State.Dying && unitMover != null)
                unitMover.Knockback(knockback);
        }
    }
}