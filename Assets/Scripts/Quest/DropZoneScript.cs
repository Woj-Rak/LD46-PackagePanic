using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DropZoneScript : MonoBehaviour
{
    public List<GameObject> itemsInZone = new List<GameObject>();
    
    private GameObject _gameManagerObject;
    public GameObject marker;
    private MeshRenderer _renderer;
    private GameManager _gameManager;

    public bool active = false;

    
    // Start is called before the first frame update
    private void Start()
    {
        _gameManagerObject = GameObject.Find("GameManager");
        _gameManager = _gameManagerObject.GetComponent<GameManager>();
        _renderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!active)
        {
            _renderer.enabled = false;
            marker.SetActive(false);
        }
        else
        {
            _renderer.enabled = true;
            marker.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Interactable") || !active) return;
        
        var interfacePickup = other.GetComponent<IPickup>();
        if (interfacePickup.TimeSinceDropped() < 1) 
        { 
            itemsInZone.Add(other.gameObject);           
        }

        if (CheckQuestConditions())
        {
            _gameManager.FinishQuest(true);
            itemsInZone.Clear();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Interactable") || !active) return;

        itemsInZone.Remove(other.gameObject);
    }

    private bool CheckQuestConditions()
    {
        return itemsInZone.Count == _gameManager.currentQuest.deliveryItems.Count;
    }
}
