// Copyright (c) 2021 RogueWare

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialProjectiles : MonoBehaviour {
	[SerializeField] public int numberOfProjectiles;
	[SerializeField] public static GameObject projectile;

	public static Vector2 startPoint;
	public static float radius, moveSpeed;
	public float time = 1f;	
	void Start() {
		radius = 0.5f;
		moveSpeed = 0.5f;
		projectile = GameObject.Find("Ball");
		//StartCoroutine(FireCoroutine());
	}
	void Update() {
	
	}
	public static void SpawnProjectiles(int numberOfProjectiles) {
		float angleStep = 360f / numberOfProjectiles;
		float angle = 0f;

		for (int i = 0; i <= numberOfProjectiles - 1; i++) {

			float projectileDirXposition = startPoint.x + Mathf.Sin((angle * Mathf.PI) / 180) * radius;
			float projectileDirYposition = startPoint.y + Mathf.Cos((angle * Mathf.PI) / 180) * radius;

			Vector2 projectileVector = new Vector2(projectileDirXposition, projectileDirYposition);
			Vector2 projectileMoveDirection = (projectileVector - startPoint).normalized * moveSpeed;

			var proj = Instantiate(projectile, startPoint, Quaternion.identity);
			proj.GetComponent<Rigidbody2D>().velocity =
				new Vector2(projectileMoveDirection.x, projectileMoveDirection.y);

			angle += angleStep;
		}
	}
	IEnumerator FireCoroutine() {
		//Declare a yield instruction.
		//WaitForSeconds wait = new WaitForSeconds(1);
		//while(BossActions.startBattle) {
			SpawnProjectiles(numberOfProjectiles);
		//}		
		yield return new WaitForSeconds(1);
	}
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Wall")) {
			Destroy(gameObject);
		}
	}
}
