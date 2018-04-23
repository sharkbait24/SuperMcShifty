﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************************************************************
 * Manages the player state and user input.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{
    [RequireComponent(typeof(UnitMover))]

    public class Player : Unit
    {
        SmGameManager gameManager;
        Player defaultTemplate;                               // An instance of the prefab player this was instantiated from (for reseting default values)

        /********************************************************************
         * Initialization of Player and base class Unit
         ********************************************************************/
        void Start()
        {
            Init();
        }

        public override void Init()
        {
            base.Init();
            gameManager = FindObjectOfType<SmGameManager>();
        }

        /********************************************************************
         * Handles user input and movement / shooting
         ********************************************************************/
        protected override void UpdateActive()
        {
            unitMover.Move(Input.GetAxis("Horizontal"), Input.GetKeyDown("space"));
        }

        /********************************************************************
         * Player died
         ********************************************************************/
        protected override void UpdateDead()
        {
            throw new System.NotImplementedException();
        }

        protected override void NonUnitCollision(Collision2D collision)
        {
            
        }
    }
}