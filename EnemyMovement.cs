using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
	private Animator animator;
	private List<Transform> targets;
	private Rigidbody2D rb2d;
	[SerializeField] private float speed;
	[SerializeField] private float maxRange;
	[SerializeField] private float minRange;
	private bool started = false;
	private Transform currentTarget;
	private Vector3 homePos;
	private Enemy enemyChar;
	[SerializeField] private float stuckThreshold;
	private float stuck;
	private bool allGood = false;

	// Start is called before the first frame update
	void Start()
	{
		enemyChar = GetComponent<Enemy>();
		animator = GetComponent<Animator>();
		targets = new List<Transform>();
		rb2d = GetComponent<Rigidbody2D>();
		var players = GameObject.FindGameObjectsWithTag("Player");
		foreach (var player in players)
		{
			targets.Add(player.transform);
		}

		currentTarget = targets[0];

		var pos = transform.position;
		homePos = new Vector3(pos.x, pos.y, pos.z);
		started = true;
	}

	private void FixedUpdate()
	{
		if(started)
		{
			bool isInRange =
				Vector3.Distance(currentTarget.position, transform.position) <= maxRange &&
				Vector3.Distance(currentTarget.position, transform.position) >= minRange;
			if(enemyChar == null)
			{
				return;
			}

			if(isInRange)
			{
				enemyChar.SetStartFight(true);
				FollowPlayer();
			}
			else
			{
				enemyChar.SetStartFight(false);
				//animator.SetBool("isRunning", false);
			}
		}
	}

	public void FollowPlayer()
	{
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		GetComponent<Rigidbody2D>().angularVelocity = 0f;
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
		MoveTo(closestTarget.position);
	}

	public void MoveTo(Vector3 translation)
	{
		var lastX = transform.position.x;
		
		var newX = Vector3.MoveTowards(transform.position, translation, speed * Time.fixedDeltaTime);
		rb2d.MovePosition(newX);

		float dx = newX.x - lastX;
		if(dx > 0)
		{ 
			GetComponent<SpriteRenderer>().flipX = false;
			var currentWpn = enemyChar.GetCurrentWeapon();
			if(currentWpn != null)
			{
				currentWpn.transform.SetParent(transform.Find("WeaponSocket"));
				currentWpn.transform.localPosition = Vector3.zero;
				var spriteRenderer = currentWpn.GetComponent<SpriteRenderer>();
				if(spriteRenderer.flipX)
				{
					currentWpn.GetComponent<SpriteRenderer>().flipX = false;
					currentWpn.FlipMuzzle();
				}
			}
		}
		else if(dx < 0)
		{
			GetComponent<SpriteRenderer>().flipX = true;
			var currentWpn = enemyChar.GetCurrentWeapon();
			if(currentWpn != null)
			{
				currentWpn.transform.SetParent(transform.Find("LeftWeaponSocket"));
				currentWpn.transform.localPosition = Vector3.zero;
				var spriteRenderer = currentWpn.GetComponent<SpriteRenderer>();
				if(!spriteRenderer.flipX)
				{
					currentWpn.FlipMuzzle();
					currentWpn.GetComponent<SpriteRenderer>().flipX = true;
				}
			}
			
		}

		if(Math.Abs(dx) > 0)
		{
			animator.SetBool("isRunning", true);
		}
	}

	private void OnCollisionStay2D(Collision2D other)
	{
		if(allGood)
		{
			return;
		}

		if(other.gameObject.CompareTag("Wall"))
		{
			stuck += Time.fixedDeltaTime;
			if(stuck >= stuckThreshold)            
			{
				Destroy(gameObject);
			}
		}
	}

	private void AllIsWell()
	{
		allGood = true;
	}

	public void InTheLevel()
	{
		Invoke(nameof(AllIsWell),1f);
	}
}
