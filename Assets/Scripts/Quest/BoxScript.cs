using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxScript : MonoBehaviour, IInteract, IPickup
{
    public enum BoxTypes
    {
        Small,
        Medium,
        Big
    }
    
    private Rigidbody _rb;

    [Header("Hand Targets")]
    public GameObject leftHandTarget;
    public GameObject rightHandTarget;

    private bool _startTiming = false;
    public float timeSinceDropped = 0f;
    
    // Start is called before the first frame update
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!_startTiming) return;
        timeSinceDropped += Time.deltaTime;
    }
    
    public void Interact()
    {
        Pickup();
    }
    
    public void Pickup()
    {
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;              
        _rb.detectCollisions = false;
        _rb.isKinematic = true;
        _rb.useGravity = false;
    }

    public void Drop()
    {
        _rb.isKinematic = false;       
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;             
        _rb.detectCollisions = true;
        _rb.useGravity = true;
        
        if (!_startTiming) _startTiming = true;
        timeSinceDropped = 0f;
    }

    public Transform ReturnLeftHandTarget()
    {
        return leftHandTarget.transform;
    }

    public Transform ReturnRightHandTarget()
    {
        return rightHandTarget.transform;
    }

   

    public float TimeSinceDropped()
    {
        if (_startTiming)
        {
            return timeSinceDropped;    
        }

        return 9999;
    }
}
