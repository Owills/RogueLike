using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSpeedPowerup : Powerup
{
	protected override void Apply(GameObject other)
	{
		// powerup will be applied to player
		PlayerCharacter pc = other.GetComponent<PlayerCharacter>();
		pc.ApplyProjectileSpeedPowerupToAllWeapons();
	}
}
