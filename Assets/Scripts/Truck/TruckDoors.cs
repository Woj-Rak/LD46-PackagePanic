using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckDoors : MonoBehaviour, IInteract
{
    public GameObject mainObject;
    private TruckScript _truckScript;
    
    // Start is called before the first frame update
    void Start()
    {
        _truckScript = mainObject.GetComponent<TruckScript>();
    }

    public void Interact()
    {
       _truckScript.Interact();
    }
}
