using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

// TODO: Move this class somewhere else?
public class Quest
{
    public GameObject dropPoint;
    public float reward;
    public List<GameObject> deliveryItems = new List<GameObject>();
}

public enum GameStates
{
    Playing,
    InMenu,
    GameOver
}

public enum TransitionType
{
    NextDay,
    GameFinished,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public Quest currentQuest;

    public GameStates currentGameState = GameStates.Playing;
    // Player Values
    public float playerMoney = 0f;
    private float playerMoneyBanked = 0f;
    
    // Telemetry for stats screen
    private int _totalPackagesDelivered = 0;
    private float _totalDistanceTraveled = 0;

    [Header("References")] 
    public GameObject playerObject;
    private PlayerMovement _playerMovement;
    public GameObject truckObject;
    [SerializeField] private TruckScript truckScript;
    public List<GameObject> dropPoints = new List<GameObject>(); 
    public List<GameObject> questItems = new List<GameObject>();
    public List<GameObject> ObstacleList = new List<GameObject>();
    public UIManager uiManager;

    [Header("Game Structure")] 
    public int maxDays;
    public int baseQuestsPerDay;
    public float baseTimePerDay;

    private int maxQuests;
    private float timeLimit;
    private float timeLeft;

    private int currentDay = 0;
    [HideInInspector] public int finishedQuests = 0;
    [HideInInspector] public bool dayActive = false;
    
    // Other
    // Should only be able to take a quest when there isn't one already active
    public bool questActive = false;
    
    // Init player positions
    private Vector3 _initPlayerPos;
    private Vector3 _initTruckPos;

    private void Awake()
    {
        // Finds all the Dropzones on the map and puts them in the game manager list for reference
        PopulateDropZones();
    }

    private void Start()
    {
        _playerMovement = playerObject.GetComponent<PlayerMovement>();
        
        DisableAllObstacles();
        
        _initPlayerPos = playerObject.transform.position;
        _initTruckPos = truckObject.transform.position;
        
        maxQuests = baseQuestsPerDay;
        timeLimit = baseTimePerDay;
    }

    private void Update()
    {
        if (!questActive) return;
        
        Navigator();
    }

    // Main game logic loop 
    private void FixedUpdate()
    {
        if (!dayActive) return;
        PassTime();
    }

    private void DayStart(int day)
    {
        currentDay++;
        
        // Initialize all the values
        timeLeft = timeLimit;
        dayActive = true; 
        
        // Show/Hide UI
        uiManager.UpdateDay(currentDay, maxDays);
        uiManager.ShowDayInfo(true);
    }

    private void DayEnd()
    {
        dayActive = false;
        uiManager.ShowDayInfo(false);

        if (currentDay != maxDays)
        {
            uiManager.DisplayMessage("Congratulations! You have successfully completed another day as a delivery man. Interact with the yellow terminal to clock out for the day.", 5);    
        }
        else
        {
            currentGameState = GameStates.InMenu;
            uiManager.UpdateTransitionScreen(TransitionType.GameFinished, playerMoneyBanked, _totalDistanceTraveled, _totalPackagesDelivered, currentDay);
        }
        
    }
    
    public void SetUpNextDay() 
    { 
        // Reset completed quest count
        finishedQuests = 0;
        // Bank player money 
        playerMoneyBanked += playerMoney;
        playerMoney = 0;
        // Increase the requirements
        maxQuests += 1;
        // Increase the timer
        timeLimit += 15f;
        // Reset fuel
        // TODO: Do this properly
        truckScript.truckFuel = 100f;
        // Reset the player/van positions
        ResetPlayerPos();
        // Draw the between levels UI
        currentGameState = GameStates.InMenu;
        uiManager.UpdateTransitionScreen(TransitionType.NextDay, playerMoneyBanked, _totalDistanceTraveled, _totalPackagesDelivered, currentDay);
        // Activate the obstacles for tomorrow
        EnableObstaclesForDay(currentDay);
    }
    
    private void PassTime()
    { 
        timeLeft -= Time.deltaTime; 
        // Update the timer UI
        uiManager.UpdateTimer(timeLeft);
        
        if (timeLeft < 0 && currentGameState != GameStates.GameOver)
        {
           GameOver(); 
        }
    }

    private void CheckEndDayConditions()
    {
        if (finishedQuests == maxQuests)
        {
            DayEnd();
        }
    }

    private void GameOver()
    {
        currentGameState = GameStates.GameOver;
        playerMoneyBanked += playerMoney;
        uiManager.UpdateTransitionScreen(TransitionType.GameOver, playerMoneyBanked, _totalDistanceTraveled, _totalPackagesDelivered, currentDay);
    }
    
    private void GameReset()
    {
        /*
        dayActive = false;
        timeLeft = 0;
        playerMoney = 0;
        playerMoneyBanked = 0;
        currentDay = 0;
        maxQuests = baseQuestsPerDay;
        timeLimit = baseTimePerDay;
        ResetPlayerPos();
        DisableAllObstacles();
        FinishQuest(false);
        uiManager.ShowTransitionScreen(false);
        uiManager.ShowDayInfo(false);
        
        currentGameState = GameStates.Playing;
        */
        Application.LoadLevel(0);
    }

    public void TransitionMenuConfirmed()
    {
        if (currentGameState == GameStates.InMenu && currentDay != maxDays)
        {
           uiManager.ShowTransitionScreen(false);
           currentGameState = GameStates.Playing;
           uiManager.DisplayMessage("Interact with the yellow terminal to start your day!", 5f);
        }
        else
        {
            GameReset();
        }
    }
    
    public void GenerateQuest(Transform QuestGiverLocation)
    {
        var newDeliveryItems = new List<GameObject>();       
        // select random drop point from all available drop points
        var dropPoint = dropPoints[Random.Range(0, dropPoints.Count - 1)];
        dropPoint.GetComponent<DropZoneScript>().active = true;

        // assign/create the necessary quest items
        var totalItemCount = Random.Range(1, 6);

        for (var i = 0; i < totalItemCount; i++)
        {
            newDeliveryItems.Add(questItems[Random.Range(0,questItems.Count-1)]);
        }
       
        // calculate the reward
        var reward = CalculateReward(newDeliveryItems, dropPoint);
        
        // Create a new quest and assign the new values
        var newQuest = new Quest {reward = reward, dropPoint = dropPoint, deliveryItems = newDeliveryItems};

        questActive = true;
        currentQuest = newQuest; 
        SpawnQuestItems(currentQuest.deliveryItems, QuestGiverLocation);
        
        uiManager.ShowNavigator(true);
        
        // If this is the first quest of the day fire the "start day" logic
        // And initialize the Deliveries UI
        if (finishedQuests == 0)
        {
            uiManager.UpdateQuestCount(finishedQuests, maxQuests);
            DayStart(currentDay);
        }
        
        if (currentDay == 1 && finishedQuests == 0)
        {
            uiManager.DisplayMessage("Pick up the packages with the interact button. Pack them onto your van by dropping them by back of the van.", 4f);
        }
    }

    private float CalculateReward(List<GameObject> deliveryItems, GameObject dropPoint){ 
        var reward = 0.0f;
        var distance = Vector3.Distance(transform.position, dropPoint.transform.position);

        // Used for stats screen
        _totalDistanceTraveled += distance;
        
        reward += distance;
        
        foreach (var item in deliveryItems)
        {
            var interfaceQuestItem = item.GetComponent<IQuestItem>();
            reward += interfaceQuestItem.ReturnItemValue();
        }  
        return reward;
    }

    private void SpawnQuestItems(List<GameObject> deliveryItems, Transform spawnLocation)
    {
        foreach (var item in deliveryItems)
        {
            var dropLocation = spawnLocation.position;
            dropLocation.x += Random.Range(-2, 2);
            dropLocation.z += Random.Range(-3, 0);
            
            Instantiate(item, dropLocation, Quaternion.Euler(Vector3.zero));
        }
    }

    public void FinishQuest(bool success)
    {
        uiManager.ShowNavigator(false);
        
        if (success)
        {
            // Give player his money + update the UI
            playerMoney += currentQuest.reward;
            uiManager.UpdateMoneyCount(playerMoney, 3f);

            // Used for stats screens
            _totalPackagesDelivered += questItems.Count;
            
            // Destroy the objects
            // TODO: Add a particle effect to this and re-enable the time before they're destroyed
            foreach (var questItem in currentQuest.dropPoint.GetComponent<DropZoneScript>().itemsInZone)
            {
               Destroy(questItem.transform.parent.gameObject, 0); 
            }

            finishedQuests++;
            uiManager.UpdateQuestCount(finishedQuests, maxQuests);
            
            // Lose references to the current quest 
            currentQuest.dropPoint.GetComponent<DropZoneScript>().active = false;
            questActive = false;
            
            // Check Win conditions
            CheckEndDayConditions();

            if (finishedQuests == 1 && currentDay == 1)
            {
                uiManager.DisplayMessage("Drive back to the yellow terminal and interact with it to accept your next delivery!", 8f);
            } else if (finishedQuests == 2 && currentDay == 1)
            {
                uiManager.DisplayMessage("Remember to re-fuel your van! Just park your van next to one of the fuel pumps behind the yellow terminal.", 5f);
            }
        }
        else
        {
            foreach (var questItem in currentQuest.deliveryItems)
            {
                Destroy(questItem.transform.parent.gameObject, 0);
            }

            // Lose references to the current quest 
            currentQuest.dropPoint.GetComponent<DropZoneScript>().active = false;
            currentQuest = null;
            questActive = false;
        }
    }

    private void Navigator()
    {
        var heading = currentQuest.dropPoint.transform.position - playerObject.transform.position;
        var distance = heading.magnitude;
        var direction = heading / distance;
      
        uiManager.UpdateNavigator(direction, distance, heading);
    }

    private void PopulateDropZones()
    {
        var dropZones = GameObject.FindGameObjectsWithTag("DropZone");
        foreach (var obj in dropZones)
        {
            dropPoints.Add(obj);
        }
    }

    private void ResetPlayerPos()
    {
        playerObject.transform.position = _initPlayerPos;
        truckObject.transform.position = _initTruckPos;
    }

    // TODO: Move this into an obstacle manager of some sort
    private void EnableObstaclesForDay(int day)
    {
        ObstacleList[day].gameObject.SetActive(true);
    }

    private void DisableAllObstacles()
    {
        foreach (var obstacleZone in ObstacleList)
        {
           obstacleZone.SetActive(false); 
        } 
    }

}
