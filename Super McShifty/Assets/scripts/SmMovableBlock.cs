using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmMovableBlock : SmGameObject {
    private Vector3 startPosition;              // Position of block at start of game
    private Vector3 prevPosition;               // Previous update's position of block to return to on collision
    private bool hasCollided;                   // Flagged in OnCollisionEnter2D function when block hits moves into 

	// Use this for initialization
	void Start () {
        startPosition = transform.position;
        prevPosition = transform.position;
        hasCollided = false;
	}
	
	// Update is called once per frame
	void Update () {
        prevPosition = transform.position;
        if (!hasCollided)
            transform.position = transform.position + Vector3.up * 0.01f;
	}

    void OnCollisionEnter2D (Collision2D collision)
    {
        hasCollided = true;
        transform.position = prevPosition;
        Debug.Log("hit " + collision.gameObject.name + " " + collision.gameObject.transform.localPosition + " " + collision.gameObject.transform.position);
        
    }

}
