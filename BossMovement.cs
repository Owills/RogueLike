using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
	
	//private bool started = false;
	private Transform currentTarget;
	private Vector3 homePos;
	private Boss bossChar;
	private float bossSpeed;
	private float _angle;
	private Rigidbody2D rb2d;
	private Vector2 center;
	private float bossHealth;
	private List<Transform> targets;
	[SerializeField] private float maxRange;
	[SerializeField] private float minRange;
	// Start is called before the first frame update
	void Start()
	{
		Invoke(nameof(LoadUp), 3f);
	}

	private void LoadUp()
	{
		bossChar = this.GetComponent<Boss>();
		bossHealth = bossChar.GetComponent<Boss>().GetHealth();
		rb2d = GetComponent<Rigidbody2D>();
		bossSpeed = 2f; 
		center = transform.position;
		targets = new List<Transform>();
		var players = GameObject.FindGameObjectsWithTag("Player");
		foreach (var player in players) {
			targets.Add(player.transform);
		}
		currentTarget = targets[0];
		
		var pos = transform.position;
		homePos = new Vector3(pos.x, pos.y, pos.z);
		//started = true;
	}

	private void FixedUpdate()
	{
		if(Boss.startBattle){
			if (bossChar.GetComponent<Boss>().GetHealth() > (bossHealth / 2)) {
				_angle += 5f * Time.deltaTime;
				var offset = new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle)) * 0.5f;
				transform.position = center + offset;
			}

			if (bossChar.GetComponent<Boss>().GetHealth() <= (bossHealth / 2)) {
				bool isInRange =
					Vector3.Distance(currentTarget.position, transform.position) <= maxRange &&
					Vector3.Distance(currentTarget.position, transform.position) >= minRange;
				if (bossChar == null) {
					return;
				}
				if (isInRange) {
					FollowPlayer();
				}				 
			}
		}
	}

	public void FollowPlayer() {
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		GetComponent<Rigidbody2D>().angularVelocity = 0f;
		Transform closestTarget = targets[0];
		foreach (var target in targets) {
			bool isTargetCloser =
				Vector3.Distance(
					target.transform.position,
					transform.position
					) <
				Vector3.Distance(
					closestTarget.position,
					transform.position
					);
			if (isTargetCloser) {
				closestTarget = target.transform;
			}
		}

		currentTarget = closestTarget;
		MoveTo(closestTarget.position);
	}

	public void MoveTo(Vector3 translation) {
		var lastX = transform.position.x;

		var newX = Vector3.MoveTowards(transform.position, translation, bossSpeed * Time.fixedDeltaTime);
		rb2d.MovePosition(newX);

		float dx = newX.x - lastX;		
	}
}
