// Copyright (c) 2021 RogueWare

using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Enemy : Character
{
	private Rigidbody2D Rigidbody;
	private float changeDirTimer;
	private int rand;
	[SerializeField] private int WeaponDropChance;
	[SerializeField] private int PowerupDropChance;
	[SerializeField] private List<GameObject> Powerups;

	private Vector3 translationDir;

	public static Transform player;

	public static bool startFight = true;

	// Start is called before the first frame update
	void Start()
	{
		base.OnStartClient();
		changeDirTimer = 1f;
		Rigidbody = GetComponent<Rigidbody2D>();
		rand = 1;
	}

	public void Shoot()
	{
		//this.currentWeapon.(GameObject.FindGameObjectWithTag("Player").transform.position);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if(!startFight)
		{
			if(changeDirTimer <= 0)
			{
				rand = Random.Range(1, 5);
				changeDirTimer = 1;
			}
			else
			{
				changeDirTimer -= Time.fixedDeltaTime;
			}
			
			GetComponent<Rigidbody2D>().velocity = Vector3.zero;
			GetComponent<Rigidbody2D>().angularVelocity = 0f;
			if(rand == 1)
			{
				translationDir = new Vector3(1, 0, 0);
			}
			else if(rand == 2)
			{
				translationDir = new Vector3(0, 1, 0);
			}
			else if(rand == 3)
			{
				translationDir = new Vector3(-1, 0, 0);
			}
			else if(rand == 4)
			{
				translationDir = new Vector3(0, -1, 0);
			}

			GetComponent<EnemyMovement>().MoveTo(transform.position + translationDir.normalized * (Time.fixedDeltaTime * movementSpeed));
			
			
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if(!other.gameObject.CompareTag("Projectile") && !other.gameObject.CompareTag("Player"))
		{
			// then it is a wall
			translationDir *= -1; // invert translation
		}
	}

	public void SetStartFight(bool isFighting)
	{
		startFight = isFighting;
	}
    protected override void Die()
    {
		int Random1 = (int) Random.Range(0,WeaponDropChance);
		int Random2 = (int) Random.Range(0, PowerupDropChance);
		if (Random1 == 0)
        {
			currentWeapon.ResetWeapon();
        } else if (Random2 == 0)
        {
			int Random3 = (int)Random.Range(0, Powerups.Count);
			Instantiate(Powerups[Random3], transform.position, Quaternion.identity);
        }
		
		
        base.Die();
    }

    /// <summary>
    /// Changes Enemy's current weapon to a new one in their inventory.
    /// </summary>
    /// <param name="weaponName"></param>
    public void SwapWeapon(string weaponName)
    {
	    for(int i=0;i<weaponInventory.Count;i++)
	    {
		    var w = weaponInventory[i];
		    var wpn = w.GetComponent<Weapon>();
		    if(wpn.GetName() == weaponName)
		    {
			    currentWeapon.GetComponent<SpriteRenderer>().enabled = false; // make wielded weapon invisible
			    currentWeapon.ReleaseTrigger();
			    currentWeaponIndex = i;
			    currentWeapon = wpn;
			    currentWeapon.GetComponent<SpriteRenderer>().enabled = true;
			    return;
		    }
	    }
    }
}
