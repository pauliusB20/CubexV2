﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootBox : MonoBehaviour
{
    [Header("Main config parameters")]
    [SerializeField] GameObject boxCore;
    [SerializeField] GameObject boxMainBody;
    [SerializeField] GameObject displayInfoCanvas;
    [SerializeField] Text infoText;
    [Range(0, 1)]
    [SerializeField] int boxType = 0; //0 - energon, 1 - credits
    [Tooltip("Ground looter unit tag(troop, worker, etc...)")]
    [SerializeField] List<string> looterTags;
    //Resource amounts to give to user
    [SerializeField] int energonToAdd = 15;
    [SerializeField] int creditsToAdd = 20;
    [SerializeField] float destroyDelayTime = 1f;
    private Base playerBase;
    private ParticleSystem boxSmoke;
    private Color boxColor;
    private bool isBoxOpened = false;

    public int BoxType {set { boxType=value; } get { return boxType; }}
    public int EnergonToAdd {set {energonToAdd = value;} get {return energonToAdd;}}
    public int CreditsToAdd {set {creditsToAdd = value;} get {return creditsToAdd;}}
    enum Box{Energon, Credits};
    void Start()
    {       
        //Find player base, to which energon or credits will be added
        playerBase = FindObjectOfType<Base>();
        boxSmoke = GetComponent<ParticleSystem>();
        
        //Setuping box color
        boxColor = new Color(0f, 122f, 255f); //Assigning default color

        //If box type is credits, then change color to yellow
        if (boxType == (int)Box.Credits)
        {
            boxColor = new Color(248f, 252f, 114f);
            
        }        
        
        boxCore.GetComponent<MeshRenderer>().material.color = boxColor;
        boxColor.a = 0.5f;
        boxSmoke.startColor = boxColor;
    }

    // Update is called once per frame
    void Update()
    {
        //Main logic for box beging opened
        //Notfiaction look at main camera
        displayInfoCanvas.transform.LookAt(Camera.main.transform);
        if (isBoxOpened)
        {
            // Destroy(gameObject);
             handleDestruction();
        }
    }
    private void OnCollisionEnter(Collision other) {       
        if (isLooterTag(other.gameObject.tag))
        {
            //Gives res
            if (boxType == (int)Box.Energon)
            {
                if (playerBase)
                {
                    if (playerBase.getEnergonAmount() + energonToAdd < playerBase.MaxBEnergon)
                    {
                        //Displaying add res info
                        displayInfo(energonToAdd);
                        playerBase.GetComponent<PlayerScoring>().addScoreAfterOpenedLootBox();
                        playerBase.setEnergonAmount(playerBase.getEnergonAmount() + energonToAdd);
                        isBoxOpened = true;
                    }
                    // else
                    // {
                    //     //Add score points instead
                    // }
                    
                   
                }
            }
            else if (boxType == (int)Box.Credits)
            {
                if (playerBase)
                {   
                    if (playerBase.getCreditsAmount() + creditsToAdd < playerBase.MaxBCredits)
                    {
                        //Displaying add res info
                        displayInfo(creditsToAdd);
                        playerBase.GetComponent<PlayerScoring>().addScoreAfterOpenedLootBox();
                        playerBase.setCreditsAmount(playerBase.getCreditsAmount() + creditsToAdd);
                         isBoxOpened = true;
                    }
                    // else
                    // {
                    //     //Add score points instead
                    // }
                   
                }
            }
            
        }
        
    }
    //Handle box destruction after pickup
    private void handleDestruction()
    {
        StartCoroutine(boxDestruction());
    }
    IEnumerator boxDestruction()
    {
        boxMainBody.SetActive(false);
        boxSmoke.enableEmission = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic  = true;
        yield return new WaitForSeconds(destroyDelayTime);
        Destroy(gameObject);
    }
    //Notfication display
    private void displayInfo(int resAmount)
    {
        displayInfoCanvas.SetActive(true);
        infoText.text = ("+" + resAmount);
        boxColor.a = 1f;
        infoText.color = boxColor;
    }
    //For checking touched unit tag
    public bool isLooterTag(string t)
    {
        foreach(var tag in looterTags)
            if (tag == t)
                return true;

        return false;
    }
}
