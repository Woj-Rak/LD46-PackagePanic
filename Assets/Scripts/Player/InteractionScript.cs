using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine;
using Object = UnityEngine.Object;

public class InteractionScript : MonoBehaviour
{
    // Stores all items currently in range of the player
    public List<GameObject> objectsInRange = new List<GameObject>();
    public GameObject objectHolder;

    // IK systems
    private FullBodyBipedIK _fik;
    private LookAtIK _headIk;
    private LookAtController _lookAt;

    public GameObject _objectInHands;
    
    // Start is called before the first frame update
    private void Start()
    {
        _fik = GetComponent<FullBodyBipedIK>();
        _headIk = GetComponent<LookAtIK>();
        _lookAt = GetComponent<LookAtController>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_objectInHands) return;
        
        _objectInHands.transform.position = objectHolder.transform.position;
        _objectInHands.transform.rotation = objectHolder.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Interactable")) return;
        
        objectsInRange.Add(other.gameObject);

        // Make the player look at the closest object in his range
        if (_objectInHands) return;
        var closestObject = FindClosestObject();
        _lookAt.target = closestObject.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Interactable")) return;
        
        objectsInRange.Remove(other.gameObject);

        // If nothing is in range reset head IK
        if (objectsInRange.Count == 0)
        {
            _lookAt.target = null;
        }
    }

    public void InteractPressed()
    {
        // If hands are occupied then drop
        if (_objectInHands)
        {
            var interfacePickup = _objectInHands.GetComponent<IPickup>();
            SetHandTargets(true);
            interfacePickup.Drop();
            _objectInHands = null;
            return;
        }
        
        if (objectsInRange.Count > 0)
        {
            var interactionObject = FindClosestObject();
            var interfaceInteract = interactionObject.GetComponent<IInteract>();
            if (interactionObject.GetComponent<IPickup>() != null)
            {
                _objectInHands = interactionObject;
                interfaceInteract.Interact();
                SetHandTargets(false);               
            }
            else
            {
                interfaceInteract.Interact();
            }

        }
    }

    private GameObject FindClosestObject()
    {
        float dist = 0; 
        GameObject returnObject = null;
        
        // Removes weird nulls left from deleting boxes upon delivery
        // TOOD: Make this call less often
        CleanObjectsInRange();
        
        foreach (var curObj in objectsInRange)
        {
            var newDist = Vector3.Distance(curObj.transform.position, transform.position);
            // If the current object is closer to the player make that the new object
            if (!(newDist < dist) && dist != 0) continue;
            dist = newDist;
            returnObject = curObj;
        }
        return returnObject;
    }

    private void CleanObjectsInRange()
    {
        foreach (var curObj in objectsInRange.Where(curObj => curObj == null))
        {
            objectsInRange.Remove(curObj);
            break;
        }
    }

    private void SetHandTargets(bool Drop)
    {
        var pickupInterface = _objectInHands.GetComponent<IPickup>();
        if (!Drop)
        {
            _fik.solver.leftHandEffector.target = pickupInterface.ReturnLeftHandTarget();
            _fik.solver.rightHandEffector.target = pickupInterface.ReturnRightHandTarget();

            _fik.solver.leftHandEffector.positionWeight = 1f;
            _fik.solver.rightHandEffector.positionWeight = 1f;
        }
        else
        {
            _fik.solver.leftHandEffector.target = null;
            _fik.solver.rightHandEffector.target = null;

            _fik.solver.leftHandEffector.positionWeight = 0f;
            _fik.solver.rightHandEffector.positionWeight = 0f;
        }
    }

    public void PutItemInHands(GameObject item)
    {
        _objectInHands = item;
        SetHandTargets(false);
    }
}
