// Copyright (c) 2021 RogueWare

using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class PlayerCharacter : Character
{
	private PlayerInput playerInput;

	private enum PowerupType
	{
		Damage,
		ProjectileSpeed,
		Health
	}

	[FormerlySerializedAs("currentPowerups")] [SerializeField] private List<PowerupType> currentWeaponPowerups;

	// just an object with a circle sprite to test mouse movement
	[SerializeField] private GameObject reticle;
	[SyncVar(hook = nameof(OnReticlePositionSync))] private Vector3 reticlePosition;

	[SerializeField] private GameObject hud;
	
	private GameObject mainCamera;
	private Rigidbody2D rb2d;
	private bool freezeReticle = false;

	private TMP_Text healthHUD;
	private Slider healthBar;

	private GameObject mapCam;

	private bool gameOver = false; // TODO make a game instance class to put stuff like this there
	[SerializeField] private GameObject gameOverUI;

	public override void OnStartAuthority()
	{
		playerInput = GetComponent<PlayerInput>();

		// subscribe to player inputs
		playerInput.FirePressed += OnFirePressed;
		playerInput.FireReleased += OnFireReleased;
		playerInput.MouseMove += OnMouseMove;
		playerInput.PlayerMove += OnPlayerMove;
		playerInput.InventoryCycle += OnInventoryCycle;
		playerInput.PlayerStopMoving += OnPlayerStopMoving;

		// subscribe to level generation done event
		//levelGenerator = GameObject.FindWithTag("LevelGenerator").GetComponent<BranchLevelGeneration>();
		//levelGenerator.LevelDone += OnLevelDone;

		// Instantiate reticle prefab
		reticle = Instantiate(reticle);
		DontDestroyOnLoad(reticle);
		
		transform.position = Vector3.zero;
		var newHud = Instantiate(hud);
		DontDestroyOnLoad(newHud);
		healthHUD = GameObject.FindGameObjectWithTag("HealthValue").GetComponent<TMP_Text>();
		healthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Slider>();
	}

	public void Reset()
	{
		foreach (var w in weaponInventory)
		{
			Destroy(w);
		}
		GetComponent<SpriteRenderer>().color = Color.white;
		weaponInventory.Clear();
		currentWeaponPowerups.Clear();
		health = startingHealth;
		UpdateHealthHud();
		transform.position = Vector3.zero;
		gameOver = false;
		playerInput.EnableInput();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		SceneManager.sceneLoaded += OnSceneLoaded;
		if(IsTestingOffline()||IsTestingTCP())
		{
			OnSceneLoaded(new Scene(), 0);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		mainCamera = GameObject.FindWithTag("MainCamera");
		rb2d = GetComponent<Rigidbody2D>();
		mapCam = GameObject.FindGameObjectWithTag("MapCam");
		mapCam.GetComponent<MiniMap>().SetPlayerRef(gameObject);
	}

	private void Update()
	{
		if(!hasAuthority || !NetworkClient.ready)
		{
			return;
		}

		// Move reticle over to mouse cursor
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.visible = true;
			freezeReticle = !freezeReticle;
		}

		if(!freezeReticle&&!gameOver)
		{
			Cursor.visible = false;
			reticlePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			reticlePosition.z = 0f;
			reticle.transform.position = reticlePosition;
		}
	}

	private void OnReticlePositionSync(Vector3 oldPos, Vector3 newPos)
	{
		reticlePosition = newPos;
	}

	private void FixedUpdate()
	{
		if(mainCamera == null || mapCam == null || reticle == null || !hasAuthority || gameOver)
		{
			OnSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);
			return;
		}

		// Make camera follow player
		Vector3 playerPosition = transform.position;
		Vector3 destination = new Vector3(
			(playerPosition.x + reticlePosition.x) / 2,
			(playerPosition.y + reticlePosition.y) / 2,
			-10
			);

		//Smooth camera
		mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, destination, 0.3f);
	}

	private void OnLevelDone(LevelGeneration.StartPosArg arg)
	{
		// move player to starting room
		transform.position = arg.GetArg();
	}

	public override void ApplyDamage(float damage)
	{
		base.ApplyDamage(damage);
		if(healthHUD != null)
		{
			healthHUD.text = health.ToString();
		}
		if(healthBar != null)
		{
			healthBar.value = health;
		}

	}

	public void UpdateHealthHud()
	{
		healthHUD.text = health.ToString();
		healthBar.value = health;
	}
	
	public void UpdateWeaponSpeed()
	{
		foreach (GameObject weapon in weaponInventory)
		{
			Weapon w = weapon.GetComponent<Weapon>();
			//w.AddSpeed((float)extraAttackSpeed);
		}
	}

	protected override void Die()
	{
		// Display Death Screen and stuff here
		Debug.Log("Player has died");
		playerInput.GameOver();
		GetComponent<SpriteRenderer>().color = Color.clear;
		Instantiate(gameOverUI);
		gameOver = true;
	}

	#region Input

	[Client]
	private void OnPlayerMove(PlayerInput.EventArgs2D deltaPos)
	{
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		GetComponent<Rigidbody2D>().angularVelocity = 0f;
		
		float dx = deltaPos.GetX();
		float dy = deltaPos.GetY();
		
		Vector3 translation = new Vector3(dx, dy).normalized / 60 * movementSpeed;
		if(rb2d == null)
		{
			rb2d = GetComponent<Rigidbody2D>();
		}

		rb2d.MovePosition(transform.position + translation);

		if(dx < 0)
		{
			GetComponent<Animator>().SetBool("isRunning", true);
			GetComponent<SpriteRenderer>().flipX = true;
			if(currentWeapon != null)
			{
				currentWeapon.transform.SetParent(transform.Find("LeftWeaponSocket"));
				currentWeapon.transform.localPosition = Vector3.zero;
				SpriteRenderer spriteRenderer = currentWeapon.GetComponent<SpriteRenderer>();
				if(!spriteRenderer.flipX)
				{
					currentWeapon.FlipMuzzle();
					spriteRenderer.flipX = true;
				}
			}
		}
		else if(dx > 0)
		{
			GetComponent<Animator>().SetBool("isRunning", true);
			GetComponent<SpriteRenderer>().flipX = false;
			if(currentWeapon != null)
			{
				currentWeapon.transform.SetParent(transform.Find("WeaponSocket"));
				currentWeapon.transform.localPosition = Vector3.zero;
				SpriteRenderer spriteRenderer = currentWeapon.GetComponent<SpriteRenderer>();
				if(spriteRenderer.flipX)
				{
					currentWeapon.FlipMuzzle();
					spriteRenderer.flipX = false;
				}
			}
		}

		if(dx != 0f || dy != 0f)
		{
			GetComponent<Animator>().SetBool("isRunning", true);
		}
	}

	[Client]
	private void OnPlayerStopMoving()
	{
		GetComponent<Animator>().SetBool("isRunning", false);
	}

	[Command]
	private void CmdSetReticlePos(Vector3 newPos)
	{
		reticlePosition = newPos;
	}

	private void OnMouseMove(PlayerInput.EventArgs2D mouseDelta)
	{
		CmdSetReticlePos(reticle.transform.position);
	}

	[Command]
	private void CmdPullTrigger(Vector3 reticlePos)
	{
		RpcTriggerPull();
		reticlePosition = reticlePos;
	}

	[Command]
	private void CmdReleaseTrigger()
	{
		RpcReleaseTrigger();
	}

	[ClientRpc]
	private void RpcTriggerPull()
	{
		if(!hasAuthority && currentWeapon != null)
		{
			currentWeapon.PullTrigger();
		}
	}

	[ClientRpc]
	private void RpcReleaseTrigger()
	{
		if(!hasAuthority && currentWeapon != null)
		{
			currentWeapon.ReleaseTrigger();
		}
	}

	[Client]
	private void OnFirePressed(PlayerInput.EventArgs2D mousePos)
	{
		if(currentWeapon != null)
		{
			currentWeapon.PullTrigger(reticlePosition);
			CmdPullTrigger(reticle.transform.position);
		}
	}

	private void OnFireReleased(EventArgs args)
	{
		if(currentWeapon != null)
		{
			currentWeapon.ReleaseTrigger();
			CmdReleaseTrigger();
		}
	}
	
	[Command]
	private void CmdUpdateCurrentIndex(int index)
	{
		UpdateCurrentWeapon(index);
	}

	private void UpdateCurrentWeapon(int index)
	{
		currentWeapon.GetComponent<SpriteRenderer>().enabled = false;
		currentWeaponIndex = index;
		currentWeapon = weaponInventory[currentWeaponIndex].GetComponent<Weapon>();
		currentWeapon.GetComponent<SpriteRenderer>().enabled = true;
	}

	protected override void UpdateClientCurrentWeapon(int oldIndex, int newIndex)
	{
		UpdateCurrentWeapon(newIndex);
	}

	private void OnInventoryCycle(PlayerInput.EventArgs2D scrollDelta2D)
	{
		float scrollDelta = scrollDelta2D.GetX();

		// if we don't have any weapons in our inventory then this shouldn't do anything.
		if(weaponInventory.Count == 0)
		{
			return;
		}

		currentWeapon.GetComponent<SpriteRenderer>().enabled = false; // make wielded weapon invisible
		currentWeapon.ReleaseTrigger();
		int index = currentWeaponIndex;
		if(scrollDelta > 0) // if we scroll up, increment index in inventory
		{
			// if we are already on the last index, wrap back to the first index
			// don't want to go out of bounds
			if(index == weaponInventory.Count - 1)
			{
				currentWeapon = weaponInventory[0].GetComponent<Weapon>();
				index = 0;
			}
			else // increment index, and wield next weapon
			{
				index++;
				currentWeapon = weaponInventory[index].GetComponent<Weapon>();
			}
		}
		else // we are scrolling down, decrement index
		{
			// if we are on the first index, wrap to the last index
			// don't want out of bounds
			if(index == 0)
			{
				currentWeapon = weaponInventory[weaponInventory.Count - 1].GetComponent<Weapon>();
				index = weaponInventory.Count - 1;
			}
			else // decrement index and wield next weapon
			{
				index--;
				currentWeapon = weaponInventory[index].GetComponent<Weapon>();
			}
		}

		if(hasAuthority)
		{
			currentWeaponIndex = index;
			currentWeapon.GetComponent<SpriteRenderer>().enabled = true; // make newly wielded weapon visible
		}
		CmdUpdateCurrentIndex(index);
	}

	#endregion

	public float GetMoveSpeed()
	{
		return movementSpeed;
	}

	public Transform GetReticleTransform()
	{
		return reticle.transform;
	}

	public override void AddWeaponToInventory(GameObject weaponObj)
	{
		base.AddWeaponToInventory(weaponObj);
		AutoWeapon autoWeapon = weaponObj.GetComponent<AutoWeapon>();
		if(autoWeapon != null)
		{
			// if this is an auto weapon
			autoWeapon.SetTargetTransform(reticle.transform);
		}
		// apply all equipped powerups
		Weapon weapon = weaponObj.GetComponent<Weapon>();
		foreach (var powerupType in currentWeaponPowerups)
		{
			switch (powerupType)
			{
				case PowerupType.Damage:
					weapon.AddDamage();
					break;
				case PowerupType.ProjectileSpeed:
					weapon.AddSpeed();
					break;
			}
		}
	}

	public void ApplyDamagePowerupToAllWeapons()
	{
		currentWeaponPowerups.Add(PowerupType.Damage);
		foreach (GameObject weapon in weaponInventory)
		{
			Weapon w = weapon.GetComponent<Weapon>();
			w.AddDamage();
		}
	}

	public void ApplyProjectileSpeedPowerupToAllWeapons() 
	{
		currentWeaponPowerups.Add(PowerupType.ProjectileSpeed);
		foreach (GameObject weapon in weaponInventory) {
			Weapon w = weapon.GetComponent<Weapon>();
			w.AddSpeed();
		}
	}
}
