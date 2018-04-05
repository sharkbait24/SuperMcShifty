using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * A base class for all game objects, which includes the terrian in this game since you can
 * move most / all of the terrain blocks.  ObjectType and ForceResponse are used by all of 
 * the collision dectection to determine how to interact with each object in the world.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

// All gameobjects need a collider to be interacted with
[RequireComponent(typeof(Collider2D))]

public class SmGameObject : MonoBehaviour {
    public enum ObjectType                  // Defines all possible object types in the game
    {
        Pickup = 0,                         // Coins and other items that can be interacted with but don't stop movement
        Block,                              // Environment terrain, which may or may not be movable
        Player,                             // The player
        AiActor                             // In general enemies, but can be any actor that isn't a player
    }

    public enum ForceResponse               // Defines all responses to being "pushed" by a moving block
    {
        Resist = 0,                         // Treated as immovable and thus will stop anything moving that collides with it
        Move,                               // Will be moved by the object that collided with it
        Ignore                              // Object isn't affected / doesn't effect other moving objects
    }

    protected ObjectType objectType;
    protected ForceResponse forceResponse;

    // Getter for objectType
    public ObjectType GetObjectType()
    {
        return objectType;
    }

    // Getter for forceResponse
    public ForceResponse GetForce()
    {
        return forceResponse;
    }
}
