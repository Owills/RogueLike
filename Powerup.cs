using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		// if a player collides with the powerup
		if(other.CompareTag("Player"))
		{
			Apply(other.gameObject);
			Destroy(gameObject);
		}
	}

	protected virtual void Apply(GameObject other)
	{
		
	}
}
