﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Rewired;

public class PlayerController : MonoBehaviour {

    [TabGroup("Movement Preference")]
	[SerializeField] 
    private PlayerTargeting pTargeting;

    public PlayerTargeting PTargeting{ get{return pTargeting;} }

    [TabGroup("Movement Preference")]
    [SerializeField]
	private float speed = 8f;

    [TabGroup("Movement Preference")]
    [SerializeField]
	public float jumpHieght = 30;

    [TabGroup("Movement Preference")]
    [SerializeField]
    private float timeSinceGrounded;

    [TabGroup("Movement Preference")]
    [SerializeField]
    private bool grounded = true;

    [TabGroup("Movement Preference")]
    [SerializeField]
    private Transform feetLevel;

    private Player input;
	private PlayerManager pManager;
	private PlayerMovement pMove;
    private FreeClimb freeClimb;
    private LedgeClimb ledgeClimb;
    private WallJump wallJump;
    private ClimbLadder climbLadder;
	private PlayerAttack pAttack;
    private PlayerBlocking pBlocking; 
    private PlayerMenu pMenu;
	private PlayerInventory pInv;
    private PlayerInteraction pInteraction;
	private ThirdPersonCamera pCamera;
    private Rigidbody rb;
	private Animator anim;
    private Vector3 dir = new Vector3();

    private const string LeftStickHorizontal = "LeftStickHorizontal";
    private const string LeftStickVertical = "LeftStickVertical";
    
    private const string RightStickHorizontal = "RightStickHorizontal";
    private const string RightStickVertical = "RightStickVetical";

    private const string AButton = "AButton";
    private const string BButton = "BButton";
    private const string YButton = "YButton";
    private const string XButton = "XButton";

    private const string RightBumper = "RightBumper";
    private const string RightTrigger = "RightTrigger";

    private const string LeftBumper = "LeftBumper";
    private const string LeftTrigger = "LeftTrigger";

    private const string Horizontal = "Horizontal";
    private const string Vertical = "Vertical";

    private const string MouseX = "Mouse X";
    private const string MouseY = "Mouse Y";

	void Start () {
		pManager = PlayerManager.instance;

        input = ReInput.players.GetPlayer(0);
        
		pMove = GetComponent<PlayerMovement>();
        wallJump = GetComponent<WallJump>();
        freeClimb = GetComponent<FreeClimb>();
        ledgeClimb = GetComponent<LedgeClimb>();
        climbLadder = GetComponent<ClimbLadder>();
		pAttack = GetComponent<PlayerAttack>();
        pBlocking = GetComponent<PlayerBlocking>();
        pMenu = GetComponent<PlayerMenu>();
		pInv = GetComponent<PlayerInventory>();
        pInteraction = GetComponent<PlayerInteraction>();
		pCamera = Camera.main.GetComponent<ThirdPersonCamera>();
		anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        timeSinceGrounded = Time.time;
	}
	
	void Update () {
		AttackInput();
        BlockingInput();
		LockOnInput();
		InteractInput();
        EquipmentInput();
        MenuInput();
        PlatFormingInput(dir);
	}

    private void LateUpdate(){
		CamerInput();
    }

	private void FixedUpdate(){
        MoveInput();
        CheckGrounded();
        RootMotionController();
	}

	private void MoveInput(){
		float h = input.GetAxis(LeftStickHorizontal);
        if(h == 0){
            h = Input.GetAxis(Horizontal);
        }

        float v = input.GetAxis(LeftStickVertical);
        if(v == 0){
            v = Input.GetAxis(Vertical);
        }

        Vector3 moveDir = new Vector3(h,0,v);
        dir = moveDir;

        if(freeClimb.isClimbing || ledgeClimb.IsClimbing){
            moveDir = new Vector3(0, 0, 0);
            anim.SetFloat("velocityY", Mathf.Max(Mathf.Abs(0), Mathf.Abs(0)));
            return;
        }
        
        if(pAttack.IsAttacking){
            // moveDir = new Vector3(0, 0, 0);
            anim.SetFloat("velocityY", Mathf.Max(Mathf.Abs(0), Mathf.Abs(0)));
            return;

        }else if(climbLadder.IsClimbing){
            climbLadder.Tick(moveDir.z);
            return;

        }else if(PlayerManager.currentState == PlayerManager.PlayerState.FreeMovement && moveDir != Vector3.zero && pManager.isVulnerable){
            pMove.FreeMovement(moveDir, speed);
            // pMove.AnimatePlayerWalking(moveDir);

        }else if(moveDir == Vector3.zero && grounded){

            if(pManager.isVulnerable && PlayerManager.currentState != PlayerManager.PlayerState.Attacking){
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
        }
	}

	private void PlatFormingInput(Vector3 moveDir = new Vector3()){
		if (Input.GetButtonDown("Jump") && !pAttack.IsAttacking){
            if(!freeClimb.isClimbing && !ledgeClimb.IsClimbing){
                if(freeClimb.CheckForClimb()){
                    return;

                }else if(climbLadder.CheckForClimb(pManager)){
                    return;

                }else if(grounded && PlayerManager.currentState != PlayerManager.PlayerState.Traversing){
                    pMove.Jump(jumpHieght, moveDir);
                    return;

                }else if(!grounded && wallJump.CheckWallJump(jumpHieght - 2)){
                    return;

                }
            }else if(freeClimb.isClimbing){
                if(dir.z == 0){
                    freeClimb.Drop();
                    // wallJump.CheckWallJump(jumpHieght);
                }else if(dir.z < 0){
                    freeClimb.Drop();
                }
            }
            else if(ledgeClimb.IsClimbing){
                if(dir.z == 0){
                    ledgeClimb.Drop();
                    wallJump.CheckWallJump(jumpHieght);
                }else if(dir.z > 0){
                    ledgeClimb.StartCoroutine(ledgeClimb.ClimbUpLedge());
                }
            }

        }else if(Input.GetButtonDown("BButton") || Input.GetKeyDown(KeyCode.E)){
            if(grounded && !pAttack.IsAttacking){
                pMove.Evade(moveDir);
            }
        }
	}

	private void AttackInput(){
		if(PlayerManager.currentState == PlayerManager.PlayerState.FreeMovement){
            if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("XButton")){//X button or click
                if(grounded && !pManager.IsPaused){
                    pAttack.Attack();
                }
            }
        }

        if(pAttack.IsAttacking && pManager.isLockedOn){
            pMove.LookAtTarget(pTargeting.currentTarget.transform);
        }
	}

    private void BlockingInput(){
        if(Input.GetMouseButton(1) && PlayerManager.currentState == PlayerManager.PlayerState.FreeMovement &&
        !pAttack.IsAttacking){
            pBlocking.SetBlocking(true);
        }else{
            pBlocking.SetBlocking(false);
        }
    }
	
	private void CamerInput(){
        if(!pManager.IsPaused){
            if(!pManager.isLockedOn){
                float h = Input.GetAxis("Mouse X") * 2;
                float v = Input.GetAxis("Mouse Y") * 2; 
                pCamera.MouseOrbit(h, v );
            }else{
                pCamera.LockedOnCam();
            }	
        }
	}

	private void LockOnInput(){
		if(Input.GetMouseButtonDown(2) || Input.GetButtonDown("RightJoyStick")){
            pTargeting.ToggleLockedOnEnemies();
            if(!pInv.Equipped){
                pInv.EquipWeapons();
            }
        }

        if(Input.GetKeyDown(KeyCode.Tab)){
            pTargeting.LockOff();
        }

        pTargeting.transform.position = transform.position;
	}

    private void EquipmentInput(){
        if(Input.GetKeyDown(KeyCode.G) || Input.GetButtonDown("YButton")){
            if(!pManager.isLockedOn && PlayerManager.currentState != PlayerManager.PlayerState.Attacking){
                pInv.EquipWeapons();
            }
        }
    }

	private void InteractInput(){
		if(Input.GetKeyDown(KeyCode.F)){
            pInteraction.InitPickUp();
        }
	}

	private void MenuInput(){
        if(Input.GetKeyDown(KeyCode.M) || Input.GetButtonDown("Start")){
            pMenu.OpenClosePlayerMenu();
        }

		if(Input.GetKeyDown(KeyCode.I)){
			pInv.OpenCloseInventory();
		}

        if(Input.GetKeyDown(KeyCode.Escape)){
            pMenu.CloseAllWindows();
        }

        if(Input.GetButtonDown("BButton")){
            pMenu.Back();
        }
	}

	public bool CheckGrounded(){
        if(PlayerManager.currentState == PlayerManager.PlayerState.Attacking)
            return true;

        RaycastHit hit;
        if(Physics.Raycast(feetLevel.position, -Vector3.up, out hit, 0.1f)){
            timeSinceGrounded = Time.time;
            anim.SetBool("isGrounded", true);
            // anim.applyRootMotion = true;
            PlatformParent.ParentToPlatform(hit.transform, transform);

            grounded = true;

            return true;

        }else{
            if(PlayerManager.currentState != PlayerManager.PlayerState.FreeClimbing)
                anim.SetBool("isGrounded", false);
                
            PlatformParent.RemoveParent(transform);
            
            // anim.applyRootMotion = false;

            grounded = false;

            return false;
        }
    }

    private void PickUpKey(GameObject key){
        GameManager.instance.gameLevels[Array.IndexOf(GameManager.instance.gameLevels, GameManager.instance.CurrentLevel)].PickUpKey(key);
    }

    private void RootMotionController(){
        if(grounded){
            anim.applyRootMotion = true;
        }else if(PlayerManager.currentState == PlayerManager.PlayerState.Traversing){
            anim.applyRootMotion = true;
        }else if(!grounded){

        }
    }
}