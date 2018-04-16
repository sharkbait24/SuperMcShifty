using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * Movement of ground based units, such as the player.  Allows for horizontal movement and
 * jumping, along with horizontal control in air.  The unit will automatically wrap around
 * if they move off the screen, simulating 80s and early 90s gaming.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]

    public class UnitMover : MonoBehaviour
    {
        [SerializeField] float runSpeed = 5f;               // Horizontal velocity applied on Move
        [SerializeField] float jumpVelocity = 12f;          // Vertical velocity applied on jump
        [SerializeField] float groundCheckDistance = 0.5f;  // Distance below box collider that raycast checks for environment

        bool isGrounded;                                    // Flag if player is on the ground
        Rigidbody2D rb;                                     // Rigidbody on object
        BoxCollider2D boxCollider;                          // Collider on this object

        /********************************************************************
         * Initialization and initial ground check since unit may start in the air.
         ********************************************************************/
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            CheckGround();
        }

        /********************************************************************
         * Automatically have the character wrap around if they move offscreen.
         * Then check if they are on ground and apply movement and jump velocities 
         * to rigidbody.  Jump only works if grounded but movement can happen in the air.
         * 
         * @param   horizontal      [-1, 1] value for x axis movement
         * @param   jump            If jump velocity should be applied
         ********************************************************************/
        public void Move(float horizontal, bool jump)
        {
            if (!SmGameManager.IsInCameraFrame(transform.position))
                transform.position = SmGameManager.PositionAfterWrapAround(transform.position);
            CheckGround();
            if (jump && isGrounded)
            {
                rb.velocity = new Vector2(horizontal * runSpeed, jumpVelocity);
            }
            else
            {
                rb.velocity = new Vector2(horizontal * runSpeed, rb.velocity.y);
            }
        }

        /********************************************************************
         * Performs a box collider check bellow the unit to see if there is
         * an object in the "Environment" layer there, and flags isGrounded if
         * there is.
         ********************************************************************/
        private void CheckGround()
        {
            if (Physics2D.BoxCast(new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.center.y - groundCheckDistance), 
                new Vector2(boxCollider.bounds.size.x, boxCollider.bounds.size.y), 0f, Vector2.down, groundCheckDistance, (int)LayerMaskValues.Environment))
            {
                isGrounded = true;
            }
            else
                isGrounded = false;
        }

        /********************************************************************
         * Apply knockback force to unit.
         * 
         * @param   force           Force to apply
         ********************************************************************/
        public void Knockback(Vector2 force)
        {
            rb.AddForce(force);
        }
    }
}