using System.Collections;
using System.Collections.Generic;
using Rewired;
using RootMotion.FinalIK;
using UnityEngine;

public class TruckScript : MonoBehaviour, IInteract
{
	public float testVar;
	[Header("UI")] public UIManager uiManager;
    // Rewired stuff
    private const int PlayerId = 0;
    private int timesDriven = 0;

    [Header("Parameters")]
    [Range(5.0f, 40.0f)] public float acceleration = 30f;
    [Range(20.0f, 160.0f)] public float steering = 80f;
    [Range(0.0f, 20.0f)] public float gravity = 10f;
    [Range(0.0f, 1.0f)] public float drift = 1f;

    [Header("Vehicle Components")] 
    public TrailRenderer trailLeft;
    public TrailRenderer trailRight;
    public ParticleSystem exhaust;
    public Transform wheelFrontLeft;
    public Transform wheelFrontRight;
    public GameObject CarExit;
    public Rigidbody sphere;
    public Transform vehicleModel;
    public Transform body;
    
    // Private
    private float speed, speedTarget = 0.0f;
    private float rotate, rotateTarget = 0.0f;
    private bool accInput, brakeInput = false;
    private Vector3 turnVector;
    private bool onGround, nearGround = false;
    private bool carLeaveInput;
    
    // Private Components
    private Player _player;
    private GameObject _playerGameObject;
    private PlayerMovement _playerMove;
    private bool _playerDriving = false;
    private bool _controllable = true;

    private Transform _container;
    private Vector3 _containerBase; 
    
    //Fuel Stuff
    public float truckFuel = 100f;
    public float fuelConsumptionRate;
    
    private void Awake()
    {
        _player = ReInput.players.GetPlayer(PlayerId);
        _container = vehicleModel.GetChild(0);
        _containerBase = _container.localPosition;
    }
    
    // Start is called before the first frame update
    private void Start()
    { 
        _playerGameObject = GameObject.Find("player");
        _playerMove = _playerGameObject.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    private void Update()
    {
        // If the player is not driving don't bother...
        if (!_playerDriving)
        {
	        //sphere.velocity = Vector3.Lerp(sphere.velocity, Vector3.zero, Time.deltaTime * 2.0f);
	        vehicleModel.transform.localRotation = Quaternion.Euler(0, 0,0);
	        sphere.velocity = Vector3.zero;
	        return;
        }

        // Get Input
        GetInput();
            
        // Let the player leave the car
        if (carLeaveInput)
        {
            Interact(); 
            return;
        }
        
        // Keep the player gameobject by the car
        _playerGameObject.transform.position = CarExit.transform.position; 
       
        // Acceleration
		speedTarget = Mathf.SmoothStep(speedTarget, speed, Time.deltaTime * 12f); speed = 0f;
		
		if(accInput){ ControlAccelerate(); }
		if(brakeInput){ ControlBrake(); }
		
		// Steering
		rotateTarget = Mathf.Lerp(rotateTarget, rotate, Time.deltaTime * 4f); rotate = 0f;

        if (turnVector.x != 0 && sphere.velocity.magnitude > 1.5)
        { 
            var steerDir = turnVector.x < 0 ? -1 : 1; 
            ControlSteer(steerDir);
        }

		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0, transform.eulerAngles.y + rotateTarget, 0)), Time.deltaTime * 2.0f);
        
        // Wheel and body tilt
		if(wheelFrontLeft != null){  wheelFrontLeft.localRotation  = Quaternion.Euler(0 , 0, rotateTarget); }
		if(wheelFrontRight != null){ wheelFrontRight.localRotation = Quaternion.Euler(0  , 0, rotateTarget); }
		
		//body.localRotation = Quaternion.Slerp(body.localRotation, Quaternion.Euler(new Vector3(speedTarget / 4, 0, rotateTarget / 6)), Time.deltaTime * 4.0f);
		
		// Vehicle tilt
		var tilt = 0.0f; 
		
		// Commented out because this caused the truck to tip over
		//_container.localPosition = _containerBase + new Vector3(0, Mathf.Abs(tilt) / 2000, 0);
		//_container.localRotation = Quaternion.Slerp(_container.localRotation, Quaternion.Euler(0, rotateTarget / 8, tilt), Time.deltaTime * 10.0f);
        
        // Effects
        ParticleSystem.EmissionModule exhaustEmission = exhaust.emission;
        // Change that 10.0f if exhaust emission should happen at slower/faster car speeds
        exhaustEmission.enabled = sphere.velocity.magnitude > (acceleration / 4) &&
                                  (Vector3.Angle(sphere.velocity, transform.forward) > 10.0f);

        trailLeft.emitting = exhaust.emission.enabled;
        trailRight.emitting = exhaust.emission.enabled;
        
        // Stop the vehicle from drifting when not driving
        if (speed == 0 && sphere.velocity.magnitude < 6f)
        {
            sphere.velocity = Vector3.Lerp(sphere.velocity, Vector3.zero, Time.deltaTime * 2.0f);
        }

    }

    // Physics go brbrbrbrbrbr
    private void FixedUpdate()
    {
	    onGround   = Physics.Raycast(transform.position, Vector3.down, out var hitOn, 1.1f);
	    nearGround = Physics.Raycast(transform.position, Vector3.down, out var hitNear, .5f);
	    
	    // Normal
	    vehicleModel.up = Vector3.Lerp(vehicleModel.up, Vector3.up, Time.deltaTime * 8.0f);
	    vehicleModel.Rotate(0, transform.eulerAngles.y, 0);
		
	    // Movement
	    if(nearGround){ 
		    sphere.AddForce(vehicleModel.forward * speedTarget, ForceMode.Acceleration);
	    }else{
		    sphere.AddForce(vehicleModel.forward * (speedTarget / 10), ForceMode.Acceleration);
		    // Simulated gravity
		    sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
	    }
	    
	    transform.position = sphere.transform.position + new Vector3(0, 0.35f, 0);
	    // Simulated drag on ground thanks to Adam Hunt
	    var localVelocity = transform.InverseTransformVector(sphere.velocity);
	    localVelocity.x *= 0.9f + (drift / 10);
	    if(nearGround){
		    sphere.velocity = transform.TransformVector(localVelocity);
	    }
    }

    private void GetInput()
    {
        turnVector.x = _player.GetAxis("Move Horizontal");
        accInput = _player.GetButton("Car Accelerate");
        brakeInput = _player.GetButton("Car Brake");
        carLeaveInput = _player.GetButtonDown("Car Leave");
    }

    public void Interact()
    {
        _playerDriving = !_playerDriving;
        
        if (_playerDriving)
        {
            CarEnter();
        }
        else
        {
            CarLeave();
        }
    }

    private void ControlAccelerate()
    {
	    if (truckFuel <= 0) return;
	    
        speed = acceleration;
        truckFuel += fuelConsumptionRate * -1;
        uiManager.UpdateFuelBarUI(truckFuel);
    }

    private void ControlBrake()
    {
	    if (truckFuel <= 0) return;
	    
        speed = -acceleration;
        truckFuel += fuelConsumptionRate * -1;
        uiManager.UpdateFuelBarUI(truckFuel);
    }

    private void ControlSteer(float direction)
    {
        rotate = steering * direction;
    }

    private void CarEnter()
    {
	    timesDriven++;
        _playerMove.controllable = false;
        _playerMove.ToggleRenderer(false);
        _playerMove.SetCameraTarget(gameObject.transform);
        
        uiManager.ShowFuelBar(true);

        if (timesDriven == 1)
        {
	        uiManager.DisplayMessage("Unpack the packages within the green zone at your destination!", 8f);
        } else if (timesDriven == 2)
        {
	        uiManager.DisplayMessage("The red bar displays your current fuel level. You can refuel your truck by stopping next to a fuel pump.", 8f);
        }
    }

    private void CarLeave()
    {
        _playerMove.controllable = true;
        _playerMove.ToggleRenderer(true);
        _playerMove.SetCameraTarget(_playerGameObject.transform);
        
        uiManager.ShowFuelBar(false);
    }
}
