﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Sirenix.OdinInspector;

using AH.Max.System;

public class PlayerLocomotion : MonoBehaviour 
{
	[TabGroup("TestIng")]
	[SerializeField]
	private bool LockedOn; // Remove

	[TabGroup("TestIng")]
	[SerializeField]
	private bool defending; // Remove

	[TabGroup("TestIng")]
	[SerializeField]
	private bool attacking;	// Remove
	
	[TabGroup("TestIng")]
	[SerializeField]
	private Transform target; // Remove

	[TabGroup(Tabs.Locomotion)]
	[SerializeField]
	private float baseSpeed;

	[TabGroup(Tabs.Locomotion)]
	[SerializeField]
	private float sprintSpeed;

	[TabGroup(Tabs.Locomotion)]
	[SerializeField]
	private float turnDamping;

	public Vector3 playerOrientationDirection = new Vector3();
	public Vector3 playerOrientationDirectionNotNormalized = new Vector3();

	private Animator animator;
	private Rigidbody _rigidbody;
	private PlayerLocomotionAnimationHook playerLocomotionAnimationHook;
	private PlayerAttackAnimationController playerAttackAnimationController;

	private void Start () 
	{
		animator = GetComponentInChildren<Animator>();
		_rigidbody = GetComponentInChildren<Rigidbody>();
		playerLocomotionAnimationHook = GetComponentInChildren<PlayerLocomotionAnimationHook>();
		playerAttackAnimationController = GetComponent<PlayerAttackAnimationController>();
	}
	
	private void FixedUpdate () 
	{
		Move(LockedOn);
		
		if(LockedOn)
		{
			if(defending || attacking)
			{
				FaceTarget();
				return;
			}
		}
		
		RotatePlayer();
	}
	
	private void Move(bool lockedOn)
	{
		Vector3 _direction = GetOrientationDirection();

		if(CanMove())
		{
			_rigidbody.velocity =
				new Vector3( (_direction.x * baseSpeed) * InputDriver.LocomotionDirection.magnitude, 
				_rigidbody.velocity.y, 
				(_direction.z * baseSpeed) * InputDriver.LocomotionDirection.magnitude ) ;
		}
	}

	///<Summary>
	/// Is the player currently in a state where movement should be applied
	///</Summary>
	private bool CanMove()
	{
		if(animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
		{
			return true;
		}

		if(playerAttackAnimationController.IsAttacking)
		{
			return false;
		}

		if(GetOrientationDirection() == Vector3.zero)
		{
			return false;
		}
		
		return true;
	}

	private void RotatePlayer()
	{
		if(playerOrientationDirection != Vector3.zero)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, GetOrientationRotation(), turnDamping);
		}
	}

	private void FaceTarget()
	{
		Vector3 _direction = target.position - transform.position;
		_direction.y = 0;
		Quaternion _rotation = Quaternion.LookRotation(_direction);

		transform.rotation = Quaternion.Lerp(transform.rotation, _rotation, turnDamping);
	}
	
	private Vector3 GetOrientationDirection()
	{
		Vector3 _direction = InputDriver.LocomotionDirection;
		_direction = EntityManager.Instance.GameCamera.transform.TransformDirection(_direction).normalized;
		_direction.y = 0;

		InputDriver.LocomotionOrientationDirection = _direction;
		playerOrientationDirection = _direction;

		return _direction;
	}

	private Quaternion GetOrientationRotation()
	{
		return Quaternion.LookRotation(GetOrientationDirection());
	}
}
