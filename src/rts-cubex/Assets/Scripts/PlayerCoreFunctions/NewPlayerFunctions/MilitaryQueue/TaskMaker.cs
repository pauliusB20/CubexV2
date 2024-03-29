﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TaskMaker : MonoBehaviour
{
    [SerializeField] GameObject taskBuilding;
    [SerializeField] int energonPrice;
    [SerializeField] int creditsPrice;
    [SerializeField] string taskName = "LightTroop";
    [SerializeField] Text priceDisplayText;
    private void Start() {
       priceDisplayText.text = "Train " + taskName + "\n" + " ("+ creditsPrice + " credits)";
    }
    public void train()
    {
        taskBuilding.GetComponent<TaskManager>().createTask(taskName, energonPrice, creditsPrice);
    }
}
