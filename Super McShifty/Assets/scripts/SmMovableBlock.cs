using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * A terrain block which the player can move with their gun.  The block can only be moved
 * in a single direction (up, down, left, right).  Upon starting to move, the block will
 * continue to move until it collides with another block in its path, or it moves out of bounds.
 * After either case, the block will disappear and then be moved back to its starting position.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

public class SmMovableBlock : SmGameObject {
    private Vector3 startPosition;              // Position of block at start of game
    private Vector3 prevPosition;               // Previous update's position of block to return to on collision
    private bool hasCollided;                   // Flagged in OnCollisionEnter2D function when block hits moves into 

	// Use this for initialization
	void Start () {
        startPosition = transform.position;
        prevPosition = transform.position;
        hasCollided = false;
        objectType = ObjectType.Block;
        forceResponse = ForceResponse.Resist;
	}
	
	// Update is called once per frame
	void Update () {
        prevPosition = transform.position;
        if (!hasCollided)
            transform.position = transform.position + Vector3.up * 0.01f;
	}

    // On each collision, we need to check if we have hit another block in our path and can then stop our movement.
    // If we hit a player or aiActor, we also need to push them along with us.
    void OnCollisionEnter2D (Collision2D collision)
    {
       hasCollided = true;
       transform.position = prevPosition;
       Debug.Log("hit " + collision.gameObject.name + " " + collision.gameObject.transform.localPosition + " " + collision.gameObject.transform.position);
        
    }

}
