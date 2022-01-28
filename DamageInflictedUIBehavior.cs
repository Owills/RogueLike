using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageInflictedUIBehavior : MonoBehaviour
{
	[SerializeField] private TMP_Text damageText;
	[SerializeField] private float timeToVanish = 3f;
	private float timeStarted;

	private void Start()
	{
		timeStarted = Time.time;
		Destroy(gameObject,timeToVanish);
	}

	private void Update()
	{
		float timeElapsed = Time.time - timeStarted;
		float lerpPecent = timeElapsed / timeToVanish;
		transform.position += Vector3.up * Time.fixedDeltaTime * .1f;
		damageText.alpha = Mathf.Lerp(1f,0f,lerpPecent);
	}

	public void SetDamageToDisplay(float damage)
	{
		damageText.text = damage.ToString();
	}
}
