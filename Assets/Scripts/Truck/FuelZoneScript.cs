using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelZoneScript : MonoBehaviour
{
    public GameManager gameManager;
    public UIManager uiManager;
    
    [Header("FuelPump Properties")] 
    public float refuelRate;
    public float fuelCost;
    private GameObject _vehicle;

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!_vehicle) return;
        
        Refuel();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Vehicle")) return;

        uiManager.ShowMoney(true);
        _vehicle = other.gameObject.transform.parent.Find("vehicle").gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Vehicle")) return;

        uiManager.ShowMoney(false);
        _vehicle = null;
    }

    private void Refuel()
    {
        if (gameManager.playerMoney <= 0 || _vehicle.GetComponent<TruckScript>().truckFuel >= 100) return;
        _vehicle.GetComponent<TruckScript>().truckFuel += refuelRate;
        gameManager.playerMoney -= fuelCost;
        uiManager.UpdateMoneyCount(gameManager.playerMoney); 
        uiManager.UpdateFuelBarUI(_vehicle.GetComponent<TruckScript>().truckFuel);
    }
}
