﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Sirenix.OdinInspector;

namespace AH.Max.Gameplay
{	
	[RequireComponent(typeof(AH.Max.Gameplay.PlayerLocomotion))]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Animator))]

	public class PlayerController : MonoBehaviour
	{
		[TabGroup("Set Up")]
		[SerializeField]
		private Transform playerCamera;

		private const string LeftStickHorizontal = "LeftStickHorizontal";
		private const string LeftStickVertical = "LeftStickVertical";

		private const string RightStickHorizontal  = "RightStickHorizontal";
		private const string RightStickVertical = "RightStickVertical";

		private const string KeyBoardHorizontal = "Horizontal";
		private const string KeyBoardVertical = "Vertical";

		private const string MouseHorizontal = "Mouse X";
		private const string MouseVertical = "Mouse Y";
		
		private const string RightShoulder_1 = "RightBumper";
		private const string RightShoulder_2 = "RightShoulderTwo";
		private const string LeftShoulder_1= "LeftShoulderOne";
		private const string LeftShoulder_2 = "LeftShoulderTwo";

		private const string ActionButtonBottomRow_1 = "AButton";
		private const string ActionButtonBottomRow_2 = "BButton";
		private const string ActionButtonTopRow_1 = "XButton";
		private const string ActionButtonTopRow_2 = "YButton";

		private Player input;
		private PlayerStateManager playerStateManager;
		private PlayerCamera playerCameraComponent;
		private PlayerLocomotion playerLocomotion;
		public PlayerFreeClimb playerFreeClimb;

		private bool isGrounded = true;
		public bool IsGrounded { get { return isGrounded; } }

		private void Awake () 
		{
			InputSetUp();
			ComponentInitialization();
		}
		
		private void Update () 
		{
			PlatformingInput();
		}

		private void FixedUpdate ()
		{
			LocomotionInput();
		}

		private void LateUpdate () 
		{
			CameraInput();
		}

		private void ComponentInitialization()
		{
			playerStateManager = GetComponent <PlayerStateManager> ();
			playerCameraComponent = playerCamera.GetComponent <PlayerCamera> ();
			playerLocomotion = GetComponent <PlayerLocomotion> ();
			playerFreeClimb = GetComponent <PlayerFreeClimb> ();
		}

		private void InputSetUp()
		{
			input = ReInput.players.GetPlayer(0);
		}

		private void LocomotionInput()
		{
			//Check Player State
			if(playerStateManager.CurrentState != PlayerStateManager.PlayerState.FreeMove) return;

			float h = input.GetAxis(LeftStickHorizontal);
			float v = input.GetAxis(LeftStickVertical);

			if(h == 0)
				h = Input.GetAxis(KeyBoardHorizontal);

			if(v == 0)
				v = Input.GetAxis(KeyBoardVertical);

			var moveDirection = new Vector3(h, 0, v);
			playerLocomotion.PlayerMove(playerCamera, moveDirection);
		}

		private void PlatformingInput(){
			if( input.GetButtonDown(ActionButtonBottomRow_1) || Input.GetKeyDown(KeyCode.Space) )
			{
				if(!playerFreeClimb.isClimbing)
				{
					if( playerFreeClimb.CheckForClimb() ) return;

					if( playerStateManager.CurrentState == PlayerStateManager.PlayerState.FreeMove )
					{
						playerLocomotion.Jump();
					}
				}
			}
		}

		private void CameraInput()
		{
			float _x = input.GetAxis(RightStickHorizontal);
			float _y = input.GetAxis(RightStickVertical);

			if( _x == 0 )
			{
				_x = Input.GetAxis(MouseHorizontal);
			}

			if( _y == 0 )
			{
				_y = Input.GetAxis(MouseVertical);
			}

			playerCameraComponent.MouseOrbit(_x, _y);
		}
	}
}