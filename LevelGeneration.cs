// Copyright (c) 2021 RogueWare

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGeneration : MonoBehaviour
{
	[SerializeField] private Transform[] startingPositions;
	public GameObject[] rooms; //0LR 1LRB 2LRT 3LRTB
	[SerializeField] private float moveAmount;

	private float timeBtwRoom;
	[SerializeField] private float startTimeBtwRoom = 0.25f;

	[SerializeField] private float minX;
	[SerializeField] private float maxX;
	[SerializeField] private float minY;
	[SerializeField] private bool stopGeneration = false;

	[SerializeField] private LayerMask room;
	private int downCounter = 0;
	private int direction;
	private Vector2 startPos;
	private long timeToGenerateMilis = 0;

	public delegate void LevelFinishedEventHandler(StartPosArg arg);
	public event LevelFinishedEventHandler LevelDone;

	void Start()
	{
		timeToGenerateMilis = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		int randStartingPos = Random.Range(0, startingPositions.Length);
		transform.position = startingPositions[randStartingPos].position;
		Vector3 lvlGenPos = startingPositions[randStartingPos].position;
		Instantiate(rooms[0], lvlGenPos, Quaternion.identity);
		startPos = lvlGenPos;
		direction = Random.Range(1, 6);
	}

	private void Move()
	{
		Vector3 currentPos = transform.position;
		if(direction == 1 || direction == 2)
		{
			if(currentPos.x < maxX)
			{
				downCounter = 0;
				Vector2 pos = new Vector2(currentPos.x + moveAmount, currentPos.y);
				transform.position = pos;
				currentPos = pos;

				int randRoom = Random.Range(0, rooms.Length);
				Instantiate(rooms[randRoom], currentPos, Quaternion.identity);

				direction = Random.Range(1, 6);
				if(direction == 3)
				{
					direction = 1;
				}
				else if(direction == 4)
				{
					direction = 5;
				}
			}
			else
			{
				direction = 5;
			}

		}
		else if(direction == 3 || direction == 4)
		{
			if(currentPos.x > minX)
			{
				downCounter = 0;
				Vector2 pos = new Vector2(currentPos.x - moveAmount, currentPos.y);
				transform.position = pos;
				currentPos = pos;

				int rand = Random.Range(0, rooms.Length);
				Instantiate(rooms[rand], currentPos, Quaternion.identity);

				direction = Random.Range(3, 6);

			}
			else
			{
				direction = 5;
			}

		}
		else if(direction == 5)
		{
			downCounter++;
			if(currentPos.y > minY)
			{
				Collider2D roomDetection = Physics2D.OverlapCircle(currentPos, 3, room);
				if(roomDetection != null
				   && roomDetection.GetComponent<RoomType>().type != 1
				   && roomDetection.GetComponent<RoomType>().type != 3
				)
				{
					if(downCounter >= 2)
					{
						roomDetection.GetComponent<RoomType>().RoomDestruction();
						Instantiate(rooms[3], currentPos, Quaternion.identity);
					}
					else
					{
						roomDetection.GetComponent<RoomType>().RoomDestruction();
						int rand = Random.Range(1, rooms.Length);
						if(rand == 2)
						{
							rand = 1;
						}

						Instantiate(rooms[rand], currentPos, Quaternion.identity);
					}

				}

				Vector2 pos = new Vector2(currentPos.x, currentPos.y - moveAmount);
				transform.position = pos;
				currentPos = pos;
				int rand1 = Random.Range(2, rooms.Length);
				Instantiate(rooms[rand1], currentPos, Quaternion.identity);

				direction = Random.Range(1, 6);
			}
			else
			{
				stopGeneration = true;
				timeToGenerateMilis = DateTimeOffset.Now.ToUnixTimeMilliseconds() - timeToGenerateMilis;
				Debug.Log($"Level Generated in {timeToGenerateMilis}ms");
				OnLevelFinished(new Vector3(startPos.x, startPos.y, 0));
			}
		}
	}

	private void Update()
	{
		if(timeBtwRoom <= 0 && stopGeneration == false)
		{
			Move();
			timeBtwRoom = startTimeBtwRoom;
		}
		else
		{
			timeBtwRoom -= Time.deltaTime;
		}

	}

	public class StartPosArg : EventArgs
	{
		private Vector3 startPosArg;

		public StartPosArg(Vector3 inPos)
		{
			startPosArg = inPos;
		}

		public Vector3 GetArg()
		{
			return startPosArg;
		}
	}

	private void OnLevelFinished(Vector3 inStartPos)
	{
		if(LevelDone != null)
		{
			LevelDone(new StartPosArg(inStartPos));
		}
	}

	public bool IsFinishedGenerating()
	{
		return stopGeneration;
	}
}
