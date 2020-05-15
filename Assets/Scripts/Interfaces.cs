using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickup
{
    void Pickup();
    void Drop();
    
    float TimeSinceDropped();

    Transform ReturnLeftHandTarget();
    Transform ReturnRightHandTarget();
}

public interface IInteract
{
    void Interact();
}

public interface IQuestItem
{
    float ReturnItemValue();
    
}
