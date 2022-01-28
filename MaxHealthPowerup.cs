using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxHealthPowerup : Powerup
{
	[SerializeField] private int MaxHealthIncreaseAmount;
	protected override void Apply(GameObject other)
	{
		// powerup will be applied to player
		PlayerCharacter pc = other.GetComponent<PlayerCharacter>();
		pc.SetMaxHealth(MaxHealthIncreaseAmount);
		pc.UpdateHealthHud();
	}
}
