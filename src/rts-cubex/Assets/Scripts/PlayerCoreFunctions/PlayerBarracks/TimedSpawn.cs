﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimedSpawn : MonoBehaviour {
    [SerializeField] GameObject spawnee;
    [SerializeField] GameObject heavySpawnee;
    [SerializeField] GameObject sniperSpawnee;
    [SerializeField] Button spawnButton;
    [SerializeField] Button heavySpawnButton;
    [SerializeField] Button exitButton;
    [SerializeField] float spawnDelay;
    [SerializeField] float spawnDelayHeavy;
    [SerializeField] int unitCost = 15;
    [SerializeField] int heavyUnitCost = 25;
    [SerializeField] int sniperUnitCost = 35;
    [SerializeField] Transform unitPosition;
    private Base playerBase;
    private Research research;
    private Base addValue;
    private openErrorPanel error;
    [SerializeField] GameObject TroopSpawningPanel;
    [SerializeField] int lightTroopWeight=1;
    [SerializeField] int heavyTroopWeight=2;
    [SerializeField] int sniperTroopWeight=3;
    [SerializeField] LightTroopTimer lightTroopTimer;
    [SerializeField] HeavyTroopTimer heavyTroopTimer;
    [SerializeField] SniperTroopTimer sniperTroopTimer;
    [SerializeField] ResearchConf oBGResearch;

    [SerializeField] int unitCodeIndex = 0;
    private void Start() {
        if(FindObjectOfType<Base>() == null)
        {
           return;
        }
        else
        {
           playerBase = FindObjectOfType<Base>();
        }
        error = FindObjectOfType<openErrorPanel>();
    }
    public void SpawnUnit () {
        if (playerBase.getCreditsAmount () <= 0) {
            closeTroop ();
            error.openError();
            error.setText("Not enough credits");
            return;
        }
        if (playerBase.getCreditsAmount () >= unitCost) {
            if(playerBase.getPlayerTroopsAmount() <  playerBase.getPlayerMaxTroopsAmount()){
            playerBase.setCreditsAmount (playerBase.getCreditsAmount () - unitCost);
            lightTroopTimer.startTimer(spawnDelay);

        }
        else {
            closeTroop ();
            error.openError();
            error.setText("Troop limit reached");
            return;
        }
        }
    }

    public void SpawnHeavyUnit () {
        //playerBase = FindObjectOfType<Base> ();
        if (FindObjectOfType<Research> () != null) {
        if ((playerBase.getCreditsAmount () >= heavyUnitCost) && (oBGResearch.getResearchLevel () >= 1)) {
            if((playerBase.getPlayerTroopsAmount() <  playerBase.getPlayerMaxTroopsAmount())&&(playerBase.getPlayerTroopsAmount()+2 <=  playerBase.getPlayerMaxTroopsAmount())) {
            playerBase.setCreditsAmount (playerBase.getCreditsAmount () - heavyUnitCost);
            heavyTroopTimer.startTimer(spawnDelayHeavy);
            }
            else {
                closeTroop ();
                error.openError();
                error.setText("Troop limit reached");
                return;
            }
        } else {
            closeTroop ();
            error.openError();
            error.setText("Not enough research level");
            return;
        }
        }
        else{
            closeTroop ();
            error.openError();
            error.setText("Research center not built");
        }
        /* spawning light or heavy troop units require the Base's credits amount and Research centre's 
        research level to check if the player has enough of both. Then a coroutine is started to add a 
        little cooldown for spawning */
    }

    public void SpawnSniperUnit () {
        //playerBase = FindObjectOfType<Base> ();
        if (FindObjectOfType<Research> () != null) {
        if ((playerBase.getCreditsAmount () >= sniperUnitCost) && (oBGResearch.getResearchLevel () >= 1)) {
            if((playerBase.getPlayerTroopsAmount() <  playerBase.getPlayerMaxTroopsAmount())&&(playerBase.getPlayerTroopsAmount()+2 <=  playerBase.getPlayerMaxTroopsAmount())) {
            playerBase.setCreditsAmount (playerBase.getCreditsAmount () - sniperUnitCost);
            heavyTroopTimer.startTimer(spawnDelayHeavy);
            }
            else {
                closeTroop ();
                error.openError();
                error.setText("Troop limit reached");
                return;
            }
        } else {
            closeTroop ();
            error.openError();
            error.setText("Not enough research level");
            return;
        }
        }
        else{
            closeTroop ();
            error.openError();
            error.setText("Research center not built");
        }
        /* spawning light or heavy troop units require the Base's credits amount and Research centre's 
        research level to check if the player has enough of both. Then a coroutine is started to add a 
        little cooldown for spawning */
    }
    
    // ATKOMENTUOTI !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public void spawnObject () {
        //if(playerBase.getPlayerTroopsAmount()<=playerBase.getPlayerMaxTroopsAmount()){
        GameObject spawnTrooper = Instantiate (spawnee, unitPosition.position, Quaternion.identity);
        spawnTrooper.name = spawnTrooper.name + unitCodeIndex;
        unitCodeIndex++;
        //FindObjectOfType<FogOfWar> ().AppendList (spawnTrooper);
        playerBase.addPlayerTroopsAmount(lightTroopWeight); 

    }
    public void spawnHeavyObject () {
        //if(playerBase.getPlayerTroopsAmount()<=playerBase.getPlayerMaxTroopsAmount()){
        GameObject spawnTrooper = Instantiate (heavySpawnee, unitPosition.position, Quaternion.identity);
        // spawnTrooper.name = spawnTrooper.name + unitCodeIndex;
        unitCodeIndex++;
        //FindObjectOfType<FogOfWar> ().AppendList (spawnTrooper);
        playerBase.addPlayerTroopsAmount(heavyTroopWeight); 

    }

    public void spawnSniperObject () {
        //if(playerBase.getPlayerTroopsAmount()<=playerBase.getPlayerMaxTroopsAmount()){
        GameObject spawnTrooper = Instantiate (sniperSpawnee, unitPosition.position, Quaternion.identity);
        spawnTrooper.name = spawnTrooper.name + unitCodeIndex;
        unitCodeIndex++;
        //FindObjectOfType<FogOfWar> ().AppendList (spawnTrooper);
        playerBase.addPlayerTroopsAmount(heavyTroopWeight); 

    }
    public void closeTroop () {
        TroopSpawningPanel.SetActive (false);
    }
    public int getLightTroopUnitCost()
    {
        return unitCost;
    }
    public int getHeavyTroopUnitCost()
    {
        return heavyUnitCost;
    }
    public int getSniperTroopUnitCost()
    {
        return sniperUnitCost;
    }
}