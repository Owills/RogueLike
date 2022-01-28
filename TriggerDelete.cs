using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDelete : MonoBehaviour
{
	public GameObject enemy;
	private bool deleted = false;
	void start()
	{

	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.CompareTag("Player"))
		{
			//delete();
		}

	}

	void OnTriggerExit2D(Collider2D other)
	{
		if(other.CompareTag("DestroyFogTag") || other.CompareTag("Door"))
		{
			if(!deleted)
			{
				delete();
				deleted = true;
			}
		}
		else if(other.CompareTag("Fog") && gameObject.tag != "Fog")
		{
			Destroy(gameObject);
		}

	}

	private void delete()
	{
		Destroy(gameObject);
		var translation = new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
		var enemyPos = transform.position + (Vector3) translation;
		var newEnemy = Instantiate(enemy,enemyPos , Quaternion.identity);
		newEnemy.GetComponent<EnemyMovement>().InTheLevel();
	}
}
