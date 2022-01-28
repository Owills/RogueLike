// Copyright (c) 2021 RogueWare

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Character : NetworkBehaviour
{
	// Editor Variables
	[SerializeField] private int maxHealth;
	[SerializeField] protected int startingHealth;
	[SerializeField] protected float movementSpeed = 1f;
	[SerializeField] private List<GameObject> startingWeapons;
	[SerializeField] protected bool isFriendly;
	[FormerlySerializedAs("DamageCounter")] [SerializeField] private GameObject damagePointUI;

	// State Variables
	[SerializeField] protected float health;
	protected Weapon currentWeapon;
	[SyncVar (hook = nameof(UpdateClientCurrentWeapon))] protected int currentWeaponIndex = 0;
	protected List<GameObject> weaponInventory;

	public override void OnStartClient()
	{
		weaponInventory = new List<GameObject>();

		health = startingHealth; // initialise health
		
		// instantiate weapons in starting inventory list, and add them to active inventory
		foreach (GameObject weaponPrefab in startingWeapons)
		{
			var wpnInstance = 
				Instantiate(weaponPrefab, transform.position, Quaternion.identity);
			AddWeaponToInventory(wpnInstance);
		}
		
	}

	protected virtual void UpdateClientCurrentWeapon(int oldIndex, int newIndex)
	{
		
	}

	private void FixedUpdate()
	{
		
	}

	protected virtual void Die()
	{
		Destroy(gameObject);
	}

	virtual public void ApplyDamage(float damage)
	{
		if(health <= 0)
		{
			return;
		}

		health -= Math.Abs(damage);
		var hudDmg = Instantiate(damagePointUI,transform.position,Quaternion.identity);
		hudDmg.GetComponent<DamageInflictedUIBehavior>().SetDamageToDisplay(damage);
		if(health <= 0)
		{
			Die();
		}
	}

	public void Heal(int healAmount)
	{
		// don't let the health go over max health
		health = Mathf.Clamp(health + healAmount, 0, maxHealth);
	}
	

	public virtual void AddWeaponToInventory(GameObject weaponObj)
	{
		if(!AlreadyHasWeapon(weaponObj)) // if we don't already have this weapon in the inventory
		{
			weaponInventory.Add(weaponObj); // add reference to our inventory
			Weapon weapon = weaponObj.GetComponent<Weapon>();
			weapon.SetIsFriendly(isFriendly); // register character as owner of the weapon
			Transform weaponTransform = weapon.transform; // get weapon transform
			weaponTransform.SetParent(transform.Find("WeaponSocket")); //child weapon transform to character's socket
			weaponTransform.localPosition = Vector3.zero; // zero position relative to socket
			weapon.PickUp();

			// if we previously had nothing in our inventory then we should select our new weapon
			if(weaponInventory.Count == 1)
			{
				currentWeapon = weapon;
			}
			else // this isn't our first weapon and it should be invisible
			{
				weapon.GetComponent<SpriteRenderer>().enabled = false;
			}
		}
	}

	private bool AlreadyHasWeapon(GameObject weaponObj)
	{
		bool alreadyHasWeapon = false;
		var wpnName = weaponObj.GetComponent<Weapon>().GetName();
		// loop through all weapons in inventory to see if that type of weapon already exists in inventory
		// For example if I already have a saxophone. I shouldn't be able to pickup another saxophone.
		foreach (var weaponPrefab in weaponInventory)
		{
			alreadyHasWeapon = weaponPrefab.GetComponent<Weapon>().GetName().Equals(
				wpnName
				);
			if(alreadyHasWeapon) // break out of the loop if the names match
			{
				break;
			}
		}

		return alreadyHasWeapon;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if(other.CompareTag("Projectile")) // if we collide with a projectile
		{
			Projectile projectile = other.GetComponent<Projectile>();
			// enemies can't kill themselves or each other, and player can't kill themselves or other players
			// if enemies have different factions then I'd probably make a team enum
			if(projectile.IsFriendly() != isFriendly)
			{
				
				ApplyDamage(projectile.GetDamage()); // apply projectile's damage to self
				Destroy(other.gameObject); // destroy projectile
			}
		}
		//boss touching player damages player
		if (other.CompareTag("Boss")) {
			Boss boss = other.GetComponent<Boss>();
			if (boss.IsFriendly() != isFriendly) {
				ApplyDamage(boss.GetDamage());
			}
		}
	}	
		public bool IsFriendly()
	{
		return isFriendly;
	}

	public bool IsTestingOffline()
	{
		try
		{
			var netManager = GameObject.FindGameObjectWithTag("TestNetManager").GetComponent<OfflineNetManager>();
			if(netManager != null)
			{
				return true;
			}
		}
		catch (Exception e)
		{
			Debug.Log(e);
			return false;
		}
		return false;
	}

	public bool IsTestingTCP()
	{
		try
		{
			var netManager = GameObject.FindGameObjectWithTag("TestNetManager").GetComponent<RogueNetManagerGame>();
			if(netManager != null)
			{
				return true;
			}
		}
		catch (Exception e)
		{
			Debug.Log(e);
			return false;
		}
		return false;
	}

	public Weapon GetCurrentWeapon()
	{
		return currentWeapon;
	}
	public float GetHealth()
	{
		return health;
	}
	public void SetMaxHealth(int increase)
	{
		maxHealth += increase;
	}
	public float GetMaxHealth()
	{
		return maxHealth;
	}
}
