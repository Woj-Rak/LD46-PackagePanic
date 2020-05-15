using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Rewired;
using RootMotion.FinalIK;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PlayerMovement : MonoBehaviour
{
    public GameManager gameManager;
    
    // Used by rewired
    private const int PlayerId = 0;

    public float moveSpeed;
    public float playerGravity;
    private Vector3 _moveVector;
    private float _animPlayerSpeed;
    private bool _menuConfirmInput;

    public bool controllable = true;
    private bool _interact;

    [SerializeField] private GameObject camera;
    private CameraScript _cameraScript;
    private Player _player;
    private CharacterController _cc;
    private Animator _anim;
    private InteractionScript _interactScript;
    private SkinnedMeshRenderer _renderer;

    private void Awake()
    {
        // Rewired Player
        _player = ReInput.players.GetPlayer(PlayerId);
        // Character controller component
        _cc = GetComponent<CharacterController>();
        // Animator
        _anim = GetComponent<Animator>();
        // Interaction
        _interactScript = GetComponent<InteractionScript>();
        // Renderer
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        // Camera Script
        _cameraScript = camera.GetComponent<CameraScript>();
    }

    // Update is called once per frame
    private void Update()
    {
        // TODO: Move all menu controls into its own script/system
        if (!controllable) return;

        if (gameManager.currentGameState == GameStates.Playing)
        { 
            GetInput();
            ProcessInput();
        } else if (gameManager.currentGameState == GameStates.InMenu || gameManager.currentGameState == GameStates.GameOver)
        { 
            _menuConfirmInput = _player.GetButtonDown("Interact");

            if (_menuConfirmInput)
            {
                gameManager.TransitionMenuConfirmed();
            }
        }

    }

    private void GetInput()
    {
        _moveVector.x = _player.GetAxis("Move Horizontal");
        _moveVector.z = _player.GetAxis("Move Vertical");
        _interact = _player.GetButtonDown("Interact");
    }

    private void ProcessInput()
    {
       // Movement
       ProcessMovement(_moveVector);
       
       // Interaction
       if (_interact)
       {
           _interactScript.InteractPressed();
       }
    }

    private void ProcessMovement(Vector3 moveVector)
    {
        // Apply Gravity
        moveVector.y -= (playerGravity * Time.deltaTime);
        
        // Limits the diagonal movement speed
        moveVector = Vector3.ClampMagnitude(moveVector, 1f);
        
        // Apply the movement Vector to the player
        _cc.Move(moveVector * (moveSpeed * Time.deltaTime));                  
        
        // Rotate the player towards movement direction
        if (moveVector.x != 0 || moveVector.z != 0)
        {
            var lookDirVector = new Vector3(moveVector.x, 0, moveVector.z);
            transform.rotation =  Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirVector.normalized), 0.2f);
            _animPlayerSpeed = _cc.velocity.magnitude;
        }
        else
        {
            if (_animPlayerSpeed > 0)
            { 
                _animPlayerSpeed -= (5.0f * Time.deltaTime);   
            }
        }
        
        // Update the playerSpeed in the animator
        _anim.SetFloat("playerSpeed", _animPlayerSpeed);
    }
    
    // Util
    public void ToggleRenderer(bool On)
    {
        _renderer.enabled = On;
    }

    public void SetCameraTarget(Transform newTarget)
    {
        _cameraScript.target = newTarget;
    }
}
