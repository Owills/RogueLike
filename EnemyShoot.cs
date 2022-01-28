using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
	private List<Transform> targets;
	private bool started = false;
	private Transform currentTarget;
	private Enemy enemyChar;
	
	[SerializeField] private float maxRange = 20f;
	[SerializeField] private float fireRate = .5f;

	private float fireTimer;

	private void Start()
	{
		fireTimer = fireRate;
		enemyChar = GetComponent<Enemy>();
		targets = new List<Transform>();
		var players = GameObject.FindGameObjectsWithTag("Player");
		foreach (var player in players)
		{
			targets.Add(player.transform);
		}

		currentTarget = targets[0];

		var pos = transform.position;
		started = true;
	}

	private void PickClosestTarget()
	{
		Transform closestTarget = targets[0];
		foreach (var target in targets)
		{
			bool isTargetCloser =
				Vector3.Distance(
					target.transform.position,
					transform.position
					) <
				Vector3.Distance(
					closestTarget.position,
					transform.position
					);
			if(isTargetCloser)
			{
				closestTarget = target.transform;
			}
		}

		currentTarget = closestTarget;
	}

	private void FixedUpdate()
	{
		if(started)
		{
			PickClosestTarget();
			if(IsInRange(currentTarget))
			{
				if(fireTimer<=0)
				{
					enemyChar.GetCurrentWeapon().PullTrigger(currentTarget.position);
					enemyChar.GetCurrentWeapon().ReleaseTrigger();
					fireTimer = fireRate;
				}
				else
				{
					fireTimer -= Time.fixedDeltaTime;
				}
			}
		}
	}

	private bool IsInRange(Transform tForm)
	{
		bool isInRange =
			Vector3.Distance(currentTarget.position, transform.position) <= maxRange;
		return isInRange;
	}
}
