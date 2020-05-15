using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProperties : MonoBehaviour, IQuestItem
{
    public float Value;
    
    public float ReturnItemValue() 
    { 
        return Value; 
    }
}
