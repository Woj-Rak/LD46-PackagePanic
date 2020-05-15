using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class TruckStorage : MonoBehaviour, IInteract
{
    public List<GameObject> objectsInStorage = new List<GameObject>();
    public List<Transform> objectSpots = new List<Transform>();
    
    // Private Components
    private GameObject _playerObject;
    private InteractionScript _playerInteractionLogic;
    
    // Start is called before the first frame update
    private void Start()
    {
        _playerObject = GameObject.Find("player");
        _playerInteractionLogic = _playerObject.GetComponent<InteractionScript>();
        
        // Initialize the spots list
        PopulateSpotsList();     
    }

    // Update is called once per frame
    private void Update()
    {
        if (objectsInStorage.Count == 0) return;
        
        var counter = 0;
        foreach (var obj in objectsInStorage)
        {
            obj.transform.position = objectSpots[counter].position;
            obj.transform.rotation = objectSpots[counter].rotation;
            counter++;
        }
    }

    private void AddItem(GameObject item)
    {
        var pickupInterface = item.GetComponent<IPickup>();
        pickupInterface.Pickup();
        objectsInStorage.Add(item); 
    }

    private void RemoveItem()
    {
        // Apply the right logic on the item
        var item = objectsInStorage[objectsInStorage.Count - 1];
        var pickupInterface = item.GetComponent<IPickup>();
        
        // Remove the item from the storage list
        objectsInStorage.Remove(item);
       
        // Put item in the players hands
        //pickupInterface.Drop();       
        _playerInteractionLogic.PutItemInHands(item);
    }
    
    private void PopulateSpotsList()
    {
        foreach (Transform spot in transform)
        {
           objectSpots.Add(spot);
        }
    }

    public void Interact()
    {
        if (objectsInStorage.Count == 0) return;
        RemoveItem();
    }

    // Trigger stuff
    private void OnTriggerEnter(Collider other)
    {
        // Check if the item is of the right type and if there's space for it
        if (!other.CompareTag("Interactable") || objectsInStorage.Count == 6) return;
        
        var interfacePickup = other.GetComponent<IPickup>();

        if (interfacePickup.TimeSinceDropped() < 1)
        {
            AddItem(other.gameObject); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }
}
