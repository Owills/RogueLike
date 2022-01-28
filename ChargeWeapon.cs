// Copyright (c) 2021 RogueWare
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChargeWeapon : AutoWeapon
{
	[SerializeField] private float timeToFullyCharge = 3f;
	[SerializeField] private float fullyChargedDamage = 100f;
	[SerializeField] private float fullyChargedAccuracy = 0f;
	[SerializeField] private float fullyChargedProjectileLifetime = 5f;
	[SerializeField] protected float fullyChargedProjectileSpeed = 48f;

	private float currentDamage;
	private float currentAccuracy;
	private float currentRange;
	private float currentSpeed;
	private float triggerPullTimestamp;
	private bool shotFired = true;
	private void FixedUpdate()
	{
		// Charge. Calculate Damage and range etc over time
		if(isTriggerPressed)
		{
			float timeSincePulled = Time.time - triggerPullTimestamp;
			float percentageIntoLerp = timeSincePulled / timeToFullyCharge;
			currentDamage = Mathf.Lerp(damage, fullyChargedDamage, percentageIntoLerp);
			currentAccuracy = Mathf.Lerp(accuracy, fullyChargedAccuracy, percentageIntoLerp);
			currentSpeed = Mathf.Lerp(projectileSpeed, fullyChargedProjectileSpeed, percentageIntoLerp);
			currentRange = Mathf.Lerp(projectileLifetime, fullyChargedProjectileLifetime, percentageIntoLerp);
		}
		// On release, fire projectile with charged stats
		else if(!shotFired)
		{
			// instantiate a projectile
			GameObject projectileInstance = Instantiate(
				projectilePrefab,
				muzzle.transform.position,
				Quaternion.identity
				);
			Projectile projectile = projectileInstance.GetComponent<Projectile>();
			// set necessary properties
			Vector3 error = Random.insideUnitCircle * currentAccuracy;
			projectile.SetDirection((-(muzzle.position - targetTransform.position).normalized)+error); // vector between target and muzzle
			projectile.SetIsFriendly(isFriendly); // if it's friendly it damages enemies but not players
			projectile.SetDamage(currentDamage);
			projectile.SetSpeed(currentSpeed);
			AudioSource.PlayClipAtPoint(fireNoise, new Vector3(muzzle.position.x, muzzle.position.y, -10f));
			Destroy(projectileInstance, currentRange); // Destroy after a certain amount of seconds
			shotFired = true;
			currentDamage = damage;
			currentSpeed = projectileSpeed;
			currentRange = projectileLifetime;
		}
	}

	public override void PullTrigger()
	{
		base.PullTrigger();
		triggerPullTimestamp = Time.time;
		shotFired = false;
		Debug.Log("TRIGGER PULL");
	}

	public override void PullTrigger(Vector3 target)
	{
		PullTrigger();
	}
}
