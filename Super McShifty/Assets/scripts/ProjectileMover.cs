using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/********************************************************************************************
 * Upon starting to move, the object will continue to move until it collides with another 
 * object with the correct ObjectType, or moves out of the visible play area or past its distance limit.
 * Projectiles will use Raycasting to detect collisions, and thus will not have a collider.
 * 
 * This class invokes the event moveFinished when it stops moving. You will need to add a listener 
 * to that event to cleanup the projectile after it stops.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{

    public class ProjectileMover : MonoBehaviour
    {
        public List<UnitType> ignoreUnitType;               // All units types the projectile will pass through
        public bool ignoreEnvironment;                      // If projectile can pass through the environment
        public float distanceLimit;                         // Maximum distance the object can move before stopping
        public UnityEvent projectileStopped;                // Event that fires everytime this object stops moving

        Vector3 moveVector;                                 // Direction and magnitude of movement
        bool isMoving;                                      // Flag for if the object is currently moving
        float distanceTravelled;                            // Distance travelled in current move


        /********************************************************************
         * Initialization
         ********************************************************************/
        void Start()
        {
            distanceTravelled = 0.0f;
        }

        /********************************************************************
         * Updates position of object if it is currently moving.  Also handles
         * If the object has moved off camera or too far.
         ********************************************************************/
        void FixedUpdate()
        {
            if (isMoving)
            {
                transform.position = transform.position + moveVector;
                distanceTravelled += moveVector.magnitude;
                if (distanceTravelled > distanceLimit || !SmGameManager.IsInCameraFrame(transform.position))
                {
                    Stop();
                }
            }
        }

        /********************************************************************
         * Begins a move in the given direction if the object is
         * not already moving.
         * 
         * @param   moveVector      Magnitude and direction of move to begin
         * @return                  Success or failure of starting the move
         ********************************************************************/
        public bool StartMove(Vector3 moveVector)
        {
            if (!isMoving)
            {
                this.moveVector = moveVector;
                isMoving = true;
                return true;
            }
            return false;
        }

        /********************************************************************
         * Changes the speed of the object by +/- delta, but doesn't change 
         * the direction.  The projectile must have already started to move.
         * 
         * @param   delta       Amount to modify the speed by
         * @return              Success or failure of changing the speed
         ********************************************************************/
        public bool ChangeSpeed(float delta)
        {
            if (isMoving)
            {
                float newSpeed = moveVector.magnitude + delta;
                moveVector = moveVector.normalized * newSpeed;
                return true;
            }
            return false;
        }

        /********************************************************************
         * Finishes a move.  Resetting all move variables and invoking the
         * projectileStopped event to alert the appropriate component.
         ********************************************************************/
        public void Stop()
        {
            if (isMoving)
            {
                moveVector = Vector3.zero;
                isMoving = false;
                distanceTravelled = 0.0f;
                projectileStopped.Invoke();
            }
        }
    }
}