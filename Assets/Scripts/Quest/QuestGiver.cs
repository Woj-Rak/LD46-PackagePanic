using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour, IInteract
{
    [Header("References")] 
    public GameObject gameManagerObj;
    private GameManager gameManager;
    public GameObject spinBox;
    public GameObject boxSpawner;

    [Header("Parameters")] 
    [Range(0, 1)] public float spinBoxSpeed; 
    
    // Start is called before the first frame update
    private void Start()
    {
        gameManager = gameManagerObj.GetComponent<GameManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        BoxSpinner();
    }

    public void Interact()
    { 
        // If there's a quest already active then return early
        if (gameManager.questActive) return;

        if (gameManager.finishedQuests > 0 && !gameManager.dayActive)
        {
            gameManager.SetUpNextDay();            
        }
        else
        {
            gameManager.GenerateQuest(boxSpawner.transform);    
        }
    }

    private void BoxSpinner()
    {
        spinBox.transform.Rotate(0, spinBoxSpeed, 0, Space.Self);
    }
}
