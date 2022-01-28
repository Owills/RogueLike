// Copyright (c) 2021 RogueWare

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
	[SerializeField] private GameObject[] objects;

	void Start()
	{
		int rand = Random.Range(0, objects.Length);
		GameObject instance = Instantiate(objects[rand], transform.position, Quaternion.identity);
		instance.transform.parent = transform;
	}
}
