// Copyright (c) 2021 RogueWare
using System;
using UnityEngine;

public class AutoWeapon : Weapon
{
	protected bool isTriggerPressed = false;
	protected Transform targetTransform;
	private float fireTimer = 0f;
	private void FixedUpdate()
	{
		if(isTriggerPressed && fireTimer <= 0f)
		{
			base.Fire(targetTransform.position);
			fireTimer = fireRate;
		}
		fireTimer -= Time.fixedDeltaTime;
		if(fireTimer <= 0)
		{
			fireTimer = -1f; //don't want to overflow lol
		}
	}

	public override void PullTrigger()
	{
		isTriggerPressed = true;
	}
	public override void PullTrigger(Vector3 target)
	{
		isTriggerPressed = true;
	}

	public override void ReleaseTrigger()
	{
		isTriggerPressed = false;
	}

	public bool IsTriggerHeld()
	{
		return isTriggerPressed;
	}

	public void SetTargetTransform(Transform target)
	{
		targetTransform = target;
	}
}