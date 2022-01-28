// Copyright (c) 2021 RogueWare

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
	[SerializeField] private string weaponName = "NameUnassigned";
	[SerializeField] protected float damage = 1f;
	[SerializeField] protected float accuracy = .15f;
	[SerializeField] protected GameObject projectilePrefab;
	[SerializeField] protected float projectileLifetime = 5f;
	[SerializeField] protected float projectileSpeed = 16f;
	[SerializeField] protected AudioClip fireNoise;
	[SerializeField] protected float fireRate = 0.1f;
	protected bool isFriendly = false;
	private bool isPickedUp = false;
	protected Transform muzzle; // Determines where the projectiles are fired from
	private bool triggerReleased = true;

	[FormerlySerializedAs("extraDamage")] [SerializeField]
	private float powerupDamageIncrement = .25f;

	[FormerlySerializedAs("extraSpeed")] [SerializeField]
	private float powerupSpeedIncrement = 0f;


	protected virtual void Start()
	{
		// Get transform of muzzle
		muzzle = transform.Find("Muzzle");
	}

	public virtual void PullTrigger(Vector3 targetPos)
	{
		if(triggerReleased)
		{
			Fire(targetPos);
			triggerReleased = false;
		}
	}

	protected virtual void Fire(Vector3 targetPos)
	{
		// instantiate a projectile
		GameObject projectileInstance = Instantiate(
			projectilePrefab,
			muzzle.transform.position,
			Quaternion.identity
			);
		Projectile projectile = projectileInstance.GetComponent<Projectile>();
		// set necessary properties
		Vector3 error = Random.insideUnitCircle * accuracy;
		projectile.SetDirection(
			(-(muzzle.position - targetPos).normalized) + error
			); // vector between target and muzzle
		projectile.SetIsFriendly(isFriendly); // if it's friendly it damages enemies but not players
		projectile.SetDamage(damage);
		projectile.SetSpeed(projectileSpeed);
		AudioSource.PlayClipAtPoint(fireNoise, new Vector3(muzzle.position.x, muzzle.position.y, -10f), 0.07f);
		
		Destroy(projectileInstance, projectileLifetime); // Destroy after a certain amount of seconds
	}

	public virtual void PullTrigger()
	{
		// Can be overridden by children. No implementation up here
	}

	public virtual void ReleaseTrigger()
	{
		triggerReleased = true;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		// if a player collides with the weapon itself and it has no owner
		if(other.CompareTag("Player") && !isPickedUp)
		{
			// player will pickup the weapon
			PlayerCharacter pc = other.GetComponent<PlayerCharacter>();
			pc.AddWeaponToInventory(gameObject);
			isPickedUp = true;
		}
	}

	public string GetName()
	{
		return weaponName;
	}

	public void SetIsFriendly(bool isCharacterFriendy)
	{
		isFriendly = isCharacterFriendy;
	}

	public void PickUp()
	{
		isPickedUp = true;
	}

	public void ResetWeapon()
	{
		isPickedUp = false;
		gameObject.transform.parent = null;
	}

	public void AddDamage()
	{
		damage += powerupDamageIncrement;
	}

	public void AddSpeed()
	{
		projectileSpeed += powerupSpeedIncrement;
	}

	public void FlipMuzzle()
	{
		var muzzlePos = muzzle.localPosition;
		muzzle.localPosition = new Vector3(-muzzlePos.x, 0f, 0f);
	}
}
