// Copyright (c) 2021 RogueWare

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRoom : MonoBehaviour
{
	[SerializeField] private LayerMask whatIsRoom;

	[SerializeField] private LevelGeneration levelGen;

	// Update is called once per frame
	void Update()
	{
		Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, whatIsRoom);
		if(roomDetection == null && levelGen.IsFinishedGenerating())
		{
			int rand = Random.Range(0, levelGen.rooms.Length);
			Instantiate(levelGen.rooms[rand], transform.position, Quaternion.identity);
			Destroy(gameObject);
		}
	}
}
