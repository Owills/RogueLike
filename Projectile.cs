// Copyright (c) 2021 RogueWare

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	// these properties are determined by the weapon
	private float damage;
	private bool isFriendly;
	private Vector3 direction;
	private float speed;

	// Every tick of the physics engine, move in the set direction
	private void FixedUpdate()
	{
		Vector3 velocity =
			new Vector3(
				direction.x * speed * Time.fixedDeltaTime,
				direction.y * speed * Time.fixedDeltaTime
			);
		transform.Translate(velocity);
	}

	public void SetDirection(Vector3 directionIn)
	{
		direction = directionIn;
	}

	public void SetDamage(float projectileDamage)
	{
		damage = projectileDamage;
	}

	public float GetDamage()
	{
		return damage;
	}

	public bool IsFriendly()
	{
		return isFriendly;
	}

	public void SetIsFriendly(bool isProjectileFriendly)
	{
		isFriendly = isProjectileFriendly;
	}

	public void SetSpeed(float projectileSpeed)
	{
		speed = projectileSpeed;
	}
	
	private void OnTriggerEnter2D(Collider2D other)
	{
		if(other.CompareTag("Door") || other.CompareTag("Wall"))
		{
			Destroy(gameObject);
		}
	}
}
