// Copyright (c) 2021 RogueWare

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TriggerFight : MonoBehaviour{
    //public event EventHandler OnPlayerEnterTrigger;
    private void Update() {
        //BossActions.player = this.transform;
        Enemy.player = this.transform;
	//MiniMap.player = this.transform;
    }
    private void OnTriggerEnter2D(Collider2D other) { 
        if (other.CompareTag("Boss")) {
            //Debug.Log("hit");
            Boss.startBattle = true; 

        } else if(other.CompareTag("Enemy")) {
            //Debug.Log("hit");
            Enemy.startFight = true;
        }
    
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            //Debug.Log("hit");
            Enemy.startFight = false;
        }
    }

    /*
        private void OnTriggerEnter2D(Collider2D collider) {
        PlayerCharacter player = collider.GetComponent<PlayerCharacter>(); 
        if(player != null) {
            OnPlayerEnterTrigger?.Invoke(this, EventArgs.Empty);
        }
    }
    */
}
