using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPowerup : Powerup
{
	[SerializeField] private int healAmount;
	protected override void Apply(GameObject other)
	{
		// powerup will be applied to player
		PlayerCharacter pc = other.GetComponent<PlayerCharacter>();
		pc.Heal(healAmount);
		pc.UpdateHealthHud();
	}
}
