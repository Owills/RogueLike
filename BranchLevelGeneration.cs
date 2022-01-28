// Copyright (c) 2021 RogueWare

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BranchLevelGeneration : MonoBehaviour
{
	public GameObject[] rooms; // index 0 = starting room;
	public int levelSizeX;
	public int levelSizeY;

	public GameObject[] bossRooms;
	public bool spawnedBossRoom;
	public GameObject fog;
	public GameObject fogTag;
	public GameObject door;

	[SerializeField] private Transform startingPosition;
	private int[,] roomGrid; // 0 == empty, 1 == contains room
	public ArrayList roomSpawnPoints;
	public ArrayList roomIndexAtSpawnPoints;

	public Vector2 bossSpawnPoint;
	public int bossRoomIndexAtSpawnPoint;

	//row major order
	private int[,] exitsGridVertical; // 0 == wall, 1 == exit, 2 == walkthrough, -1 == empty
	private int[,] exitsGridHorizontal; // 0 == wall, 1 == exit, 2 == walkthrough, -1 == empty

	private int maxRooms;
	private int minRooms;

	private int nextRoomX;
	private int nextRoomY;
	private Vector2 pos;

	private bool rr = false;

	private List<GameObject> spawnedRooms;


	[SerializeField] private bool stopGeneration = false;

	private int seed = 0;

	public void StartLevelGeneration()
	{

		spawnedBossRoom = false;
		spawnedRooms = new List<GameObject>();
		roomGrid = new int[levelSizeX, levelSizeY];
		roomSpawnPoints = new ArrayList();
		roomIndexAtSpawnPoints = new ArrayList();
		exitsGridVertical = new int[levelSizeX + 1, levelSizeY + 1];
		exitsGridHorizontal = new int[levelSizeX + 1, levelSizeY + 1];
		for (int i = 0; i < exitsGridHorizontal.GetLength(0); i++)
		{
			for (int j = 0; j < exitsGridHorizontal.GetLength(1); j++)
			{
				if(i == 0 || i == levelSizeX || j == 0 || j == levelSizeY)
				{
					exitsGridVertical[i, j] = 0;
					exitsGridHorizontal[i, j] = 0;
				}
				else
				{
					exitsGridVertical[i, j] = -1;
					exitsGridHorizontal[i, j] = -1;
				}


			}
		}

		for (int i = 0; i < roomGrid.GetLength(0); i++)
		{
			for (int j = 0; j < roomGrid.GetLength(1); j++)
			{
				roomGrid[i, j] = 0;
			}
		}

		maxRooms = (int) (0.7f * levelSizeX * levelSizeY); //add ending rooms, so there are no open ends
		minRooms = (int) (0.15f * levelSizeX * levelSizeY); //implement this


		nextRoomX = 0;
		nextRoomY = (int) (0.5f * levelSizeY);

		//add starting room
		roomGrid[nextRoomX, nextRoomY] = 1;
		for (int i = 0;
			i < rooms[0].GetComponent<RoomType>().size * 2;
			i++) //first room in list should match the orientation of starting room
		{
			exitsGridVertical[nextRoomX + i, nextRoomY] = rooms[0].GetComponent<RoomType>().exitsGridVertical[i];
			exitsGridHorizontal[nextRoomX, nextRoomY + i] = rooms[0].GetComponent<RoomType>().exitsGridHorizontal[i];
		}

		nextRoomX = 1;
		pos = new Vector2((nextRoomX) * 10, ((int) (0.5f * levelSizeY) - (nextRoomY)) * 10);

		while (!stopGeneration)
		{
			//get a list of all the possible rooms that can be spawned at a certain location
			ArrayList ValidRoomIndexes = new ArrayList();
			ArrayList CorrespondingStartPosition = new ArrayList(); // computationlly more effiecnet than determining later I think
			if (getFullRooms() > 10 && !spawnedBossRoom)
			{
				for (int i = 0; i < bossRooms.Length; i++) // rooms 0-3 are dead end rooms
				{
					bool valid = true;

						for (int j = 0; j < bossRooms[i].GetComponent<RoomType>().size; j++)
						{
							for (int k = 0; k < bossRooms[i].GetComponent<RoomType>().size; k++)
							{
								valid = true;
								int tempX = nextRoomX - j;
								int tempY = nextRoomY - k;
								//check roomGrid
								for (int numX = tempX; numX < tempX + 2; numX++) //0,0 = 0 | 0,1 = 1 | 1,0 =2 | 1,1 = 3
								{
									for (int numY = tempY; numY < tempY + 2; numY++)
									{
										int roomNum = 2 * (numX - tempX) + (numY - tempY);

										if (numX < roomGrid.GetLength(0) && numY < roomGrid.GetLength(1))
										{
											if (roomGrid[numX, numY] != 0)
											{
												valid = false;
											}

										}
										else
										{
											valid = false;
										}
									}
								}

								//check vertical
								if (valid)
								{

									for (int numY = tempY;
										numY < tempY + 2;
										numY++) //0,0 = 0 | 0,1 = 1 | 0,2 =2 | 1,0 = 3 | 1,1 = 4 | 1,2 = 5;
									{
										for (int numX = tempX; numX < tempX + 3; numX++)
										{
											int roomNum = (numX - tempX) + 3 * (numY - tempY);
											int num = bossRooms[i].GetComponent<RoomType>().exitsGridVertical[roomNum];
											if (exitsGridVertical[numX, numY] != -1 && exitsGridVertical[numX, numY] != num)
											{
												valid = false;
											}

										}
									}

									if (valid)
									{
										//check horizontal
										for (int numY = tempY;
											numY < tempY + 3;
											numY++) //0,0 = 0 | 1,0 = 1 | 0,1 =2 | 1,1 = 3 | 0,2 = 4 | 1,2 = 5;
										{

											for (int numX = tempX; numX < tempX + 2; numX++)
											{

												int roomNum = (numX - tempX) + 2 * (numY - tempY);

												if (exitsGridHorizontal[numX, numY] != -1 &&
												   exitsGridHorizontal[numX, numY] != bossRooms[i].GetComponent<RoomType>()
													   .exitsGridHorizontal[roomNum])
												{
													valid = false;
												}
											}
										}
									}

								}


								//add valid room to arrayLists
								if (valid)
								{
									int[] spawnPoint1 = new int[] { tempX, tempY };
									ValidRoomIndexes.Add(i); //bigger rooms more likely to be chosen 
									CorrespondingStartPosition.Add(spawnPoint1);

									
									
								}
							}
						}
				}
				if (ValidRoomIndexes.Count > 0)
				{
					spawnedBossRoom = true;
					Debug.Log("bossroom");
					int rand = Random.Range(0, ValidRoomIndexes.Count);
					int roomSpawn = (int)ValidRoomIndexes[rand];

					int[] spawnPoint = (int[])CorrespondingStartPosition[rand];
					pos = new Vector2((spawnPoint[0]) * 10, ((int)(0.5f * levelSizeY) - (spawnPoint[1])) * 10);
					if (bossRooms[roomSpawn].GetComponent<RoomType>().containsRoom[0] == 1)
					{
						roomGrid[spawnPoint[0], spawnPoint[1]] = 1;
					}

					if (bossRooms[roomSpawn].GetComponent<RoomType>().containsRoom[1] == 1)
					{
						roomGrid[spawnPoint[0] + 1, spawnPoint[1]] = 1;
					}

					if (bossRooms[roomSpawn].GetComponent<RoomType>().containsRoom[2] == 1)
					{
						roomGrid[spawnPoint[0], spawnPoint[1] + 1] = 1;
					}

					if (bossRooms[roomSpawn].GetComponent<RoomType>().containsRoom[3] == 1)
					{
						roomGrid[spawnPoint[0] + 1, spawnPoint[1] + 1] = 1;
					}

					bossRoomIndexAtSpawnPoint = roomSpawn;
					bossSpawnPoint = pos;

					for (int numY = spawnPoint[1];
						numY < spawnPoint[1] + 2;
						numY++) //0,0 = 0 | 0,1 = 1 | 0,2 =2 | 1,0 = 3 | 1,1 = 4 | 1,2 = 5;
					{
						for (int numX = spawnPoint[0]; numX < spawnPoint[0] + 3; numX++)
						{
							int roomNum = (numX - spawnPoint[0]) + 3 * (numY - spawnPoint[1]);
							if (bossRooms[roomSpawn].GetComponent<RoomType>().exitsGridVertical[roomNum] != -1)
							{
								exitsGridVertical[numX, numY] = bossRooms[roomSpawn].GetComponent<RoomType>().exitsGridVertical[roomNum];
							}

						}
					}

					for (int numY = spawnPoint[1];
						numY < spawnPoint[1] + 3;
						numY++) //0,0 = 0 | 0,1 = 1 | 1,0 =2 | 1,1 = 3 | 2,0 = 4 | 2,1 = 5;
					{
						for (int numX = spawnPoint[0]; numX < spawnPoint[0] + 2; numX++)
						{

							int roomNum = (numX - spawnPoint[0]) + 2 * (numY - spawnPoint[1]);
							if (bossRooms[roomSpawn].GetComponent<RoomType>().exitsGridHorizontal[roomNum] != -1)
							{
								exitsGridHorizontal[numX, numY] = bossRooms[roomSpawn].GetComponent<RoomType>().exitsGridHorizontal[roomNum];
							}

						}
					}
				}

			}
			ValidRoomIndexes = new ArrayList();
			CorrespondingStartPosition = new ArrayList();
			for (int i = 4; i < rooms.Length; i++) // rooms 0-3 are dead end rooms
				{
					bool valid = true;
					if (rooms[i].GetComponent<RoomType>().size == 2) // check multiple different spawn points
					{
						for (int j = 0; j < rooms[i].GetComponent<RoomType>().size; j++)
						{
							for (int k = 0; k < rooms[i].GetComponent<RoomType>().size; k++)
							{
								valid = true;
								int tempX = nextRoomX - j;
								int tempY = nextRoomY - k;
								//check roomGrid
								for (int numX = tempX; numX < tempX + 2; numX++) //0,0 = 0 | 0,1 = 1 | 1,0 =2 | 1,1 = 3
								{
									for (int numY = tempY; numY < tempY + 2; numY++)
									{
										int roomNum = 2 * (numX - tempX) + (numY - tempY);

										if (numX < roomGrid.GetLength(0) && numY < roomGrid.GetLength(1))
										{
											if (roomGrid[numX, numY] != 0)
											{
												valid = false;
											}

										}
										else
										{
											valid = false;
										}
									}
								}

								//check vertical
								if (valid)
								{

									for (int numY = tempY;
										numY < tempY + 2;
										numY++) //0,0 = 0 | 0,1 = 1 | 0,2 =2 | 1,0 = 3 | 1,1 = 4 | 1,2 = 5;
									{
										for (int numX = tempX; numX < tempX + 3; numX++)
										{
											int roomNum = (numX - tempX) + 3 * (numY - tempY);
											int num = rooms[i].GetComponent<RoomType>().exitsGridVertical[roomNum];
											if (exitsGridVertical[numX, numY] != -1 && exitsGridVertical[numX, numY] != num)
											{
												valid = false;
											}

										}
									}

									if (valid)
									{
										//check horizontal
										for (int numY = tempY;
											numY < tempY + 3;
											numY++) //0,0 = 0 | 1,0 = 1 | 0,1 =2 | 1,1 = 3 | 0,2 = 4 | 1,2 = 5;
										{

											for (int numX = tempX; numX < tempX + 2; numX++)
											{

												int roomNum = (numX - tempX) + 2 * (numY - tempY);

												if (exitsGridHorizontal[numX, numY] != -1 &&
												   exitsGridHorizontal[numX, numY] != rooms[i].GetComponent<RoomType>()
													   .exitsGridHorizontal[roomNum])
												{
													valid = false;
												}
											}
										}
									}

								}


								//add valid room to arrayLists
								if (valid)
								{
									int[] spawnPoint = new int[] { tempX, tempY };
									ValidRoomIndexes.Add(i); //bigger rooms more likely to be chosen 
									CorrespondingStartPosition.Add(spawnPoint);
									ValidRoomIndexes.Add(i);
									CorrespondingStartPosition.Add(spawnPoint);
									ValidRoomIndexes.Add(i);
									CorrespondingStartPosition.Add(spawnPoint);
								}
							}
						}
					}
					else if (rooms[i].GetComponent<RoomType>().size == 1)
					{
						for (int j = 0; j < rooms[i].GetComponent<RoomType>().size * 2; j++)
						{
							if (exitsGridVertical[nextRoomX + j, nextRoomY] != -1 &&
							   exitsGridVertical[nextRoomX + j, nextRoomY] !=
							   rooms[i].GetComponent<RoomType>().exitsGridVertical[j])
							{
								valid = false;
							}
							else if (exitsGridHorizontal[nextRoomX, nextRoomY + j] != -1 &&
									exitsGridHorizontal[nextRoomX, nextRoomY + j] !=
									rooms[i].GetComponent<RoomType>().exitsGridHorizontal[j])
							{
								valid = false;
							}
						}

						if (valid)
						{
							ValidRoomIndexes.Add(i);
							int[] spawnPoint = new int[] { nextRoomX, nextRoomY };
							CorrespondingStartPosition.Add(spawnPoint);
						}
					}
				}
			
			// pick randomly from the valid rooms and add to spawn arrayLists
			//some calcuations are happening here for the secondn time for 2by2 and 3by3 rooms. could be done better?
			if(ValidRoomIndexes.Count > 0)
			{
				int rand = Random.Range(0, ValidRoomIndexes.Count);
				int roomSpawn = (int) ValidRoomIndexes[rand];

				int[] spawnPoint = (int[]) CorrespondingStartPosition[rand];
				pos = new Vector2((spawnPoint[0]) * 10, ((int) (0.5f * levelSizeY) - (spawnPoint[1])) * 10);

				if(rooms[roomSpawn].GetComponent<RoomType>().size == 1)
				{
					roomGrid[spawnPoint[0], spawnPoint[1]] = 1;
					roomSpawnPoints.Add(pos);
					roomIndexAtSpawnPoints.Add(roomSpawn);
					for (int j = 0; j < rooms[roomSpawn].GetComponent<RoomType>().size * 2; j++)
					{
						exitsGridVertical[nextRoomX + j, nextRoomY] =
							rooms[roomSpawn].GetComponent<RoomType>().exitsGridVertical[j];
						exitsGridHorizontal[nextRoomX, nextRoomY + j] =
							rooms[roomSpawn].GetComponent<RoomType>().exitsGridHorizontal[j];
					}
				}
				else if(rooms[roomSpawn].GetComponent<RoomType>().size == 2)
				{
					if(rooms[roomSpawn].GetComponent<RoomType>().containsRoom[0] == 1)
					{
						roomGrid[spawnPoint[0], spawnPoint[1]] = 1;
					}

					if(rooms[roomSpawn].GetComponent<RoomType>().containsRoom[1] == 1)
					{
						roomGrid[spawnPoint[0] + 1, spawnPoint[1]] = 1;
					}

					if(rooms[roomSpawn].GetComponent<RoomType>().containsRoom[2] == 1)
					{
						roomGrid[spawnPoint[0], spawnPoint[1] + 1] = 1;
					}

					if(rooms[roomSpawn].GetComponent<RoomType>().containsRoom[3] == 1)
					{
						roomGrid[spawnPoint[0] + 1, spawnPoint[1] + 1] = 1;
					}

					roomSpawnPoints.Add(pos);
					roomIndexAtSpawnPoints.Add(roomSpawn);
					for (int numY = spawnPoint[1];
						numY < spawnPoint[1] + 2;
						numY++) //0,0 = 0 | 0,1 = 1 | 0,2 =2 | 1,0 = 3 | 1,1 = 4 | 1,2 = 5;
					{
						for (int numX = spawnPoint[0]; numX < spawnPoint[0] + 3; numX++)
						{
							int roomNum = (numX - spawnPoint[0]) + 3 * (numY - spawnPoint[1]);
							if(rooms[roomSpawn].GetComponent<RoomType>().exitsGridVertical[roomNum] != -1)
							{
								exitsGridVertical[numX, numY] =
									rooms[roomSpawn].GetComponent<RoomType>().exitsGridVertical[roomNum];
							}

						}
					}

					for (int numY = spawnPoint[1];
						numY < spawnPoint[1] + 3;
						numY++) //0,0 = 0 | 0,1 = 1 | 1,0 =2 | 1,1 = 3 | 2,0 = 4 | 2,1 = 5;
					{
						for (int numX = spawnPoint[0]; numX < spawnPoint[0] + 2; numX++)
						{

							int roomNum = (numX - spawnPoint[0]) + 2 * (numY - spawnPoint[1]);
							if(rooms[roomSpawn].GetComponent<RoomType>().exitsGridHorizontal[roomNum] != -1)
							{
								exitsGridHorizontal[numX, numY] =
									rooms[roomSpawn].GetComponent<RoomType>().exitsGridHorizontal[roomNum];
							}

						}
					}
				}
			}
			else if(getFullRooms() >= minRooms && spawnedBossRoom) // no possible valid rooms and the number of rooms exceeds our minimum room requirment, so levelGeneration is finished
			{
				stopGeneration = true;
			}
			else // there are not enough rooms to finished generation, so some rooms must be deleted a replaced will rooms with more exits
			{
				replaceRooms();
			}

			//limit levelGeneration loading time, by limiting the number of rooms
			if(getFullRooms() >= maxRooms)
			{
				stopGeneration = true;
			}
			else if(stopGeneration == false) // get next location, this most likely could be much more efficeint
			{
				ArrayList ValidNewRoomIndexes = new ArrayList();
				for (int x = 1; x < roomGrid.GetLength(0) - 1; x++)
				{
					for (int y = 1; y < roomGrid.GetLength(1) - 1; y++)
					{
						if(roomGrid[x, y] == 0)
						{

							if(roomGrid[x - 1, y] == 1 || roomGrid[x, y - 1] == 1 || roomGrid[x + 1, y] == 1 ||
							   roomGrid[x, y + 1] == 1)
							{
								if(exitsGridVertical[x, y] == 1 || exitsGridVertical[x + 1, y] == 1 ||
								   exitsGridHorizontal[x, y] == 1 || exitsGridHorizontal[x, y + 1] == 1)
								{
									int[] point = new int[] {x, y};

									ValidNewRoomIndexes.Add(point);
								}
							}
						}
						else
						{
						}
					}
				}

				if(ValidNewRoomIndexes.Count > 0)
				{
					int rand = Random.Range(0, ValidNewRoomIndexes.Count);
					int[] newXY = (int[]) ValidNewRoomIndexes[rand];
					nextRoomX = newXY[0];
					nextRoomY = newXY[1];
					pos = new Vector2((nextRoomX) * 10, ((int) (0.5f * levelSizeY) - (nextRoomY)) * 10);
				}
				else if(getFullRooms() >= minRooms && spawnedBossRoom) // no possible valid rooms and the number of rooms exceeds our minimum room requirment, so levelGeneration is finished
				{
					stopGeneration = true;
				}
				else // there are not enough rooms to finished generation, so some rooms must be deleted a replaced will rooms with more exits
				{
					replaceRooms();
				}
			}
		}
        if (!rr)
        {
			//add ending rooms
			genClosingRooms();
			//spawn rooms
			spawnRooms();
		}
		
	}

	void genClosingRooms() // finish level generation and create deadends on branches
	{
		for (int i = 1; i < exitsGridHorizontal.GetLength(0); i++)
		{
			for (int j = 1; j < exitsGridHorizontal.GetLength(1); j++)
			{
				if(exitsGridVertical[i, j] == 1)
				{
					if(roomGrid[i, j] == 0 && roomGrid[i - 1, j] == 1) //right
					{
						for (int num = 0; num < rooms.Length; num++) //last rooms are 2by2
						{
							if (rooms[num].GetComponent<RoomType>().size == 1)
							{
								if (exitsGridVertical[i, j] == rooms[num].GetComponent<RoomType>().exitsGridVertical[0] ||
								   exitsGridVertical[i, j] == -1)
								{
									if (exitsGridVertical[i + 1, j] ==
									   rooms[num].GetComponent<RoomType>().exitsGridVertical[1] ||
									   exitsGridVertical[i + 1, j] == -1)
									{
										if (exitsGridHorizontal[i, j] ==
										   rooms[num].GetComponent<RoomType>().exitsGridHorizontal[0] ||
										   exitsGridHorizontal[i, j] == -1)
										{
											if (exitsGridHorizontal[i, j + 1] ==
											   rooms[num].GetComponent<RoomType>().exitsGridHorizontal[1] ||
											   exitsGridHorizontal[i, j + 1] == -1)
											{
												pos = new Vector2((i) * 10, ((int)(0.5f * levelSizeY) - (j)) * 10);
												roomSpawnPoints.Add(pos);
												roomIndexAtSpawnPoints.Add(num);
												num = rooms.Length;
											}
										}
									}
								}
							}
						}
					}
					else if(roomGrid[i - 1, j] == 0 && roomGrid[i, j] == 1) //left
					{
						for (int num = 0; num < rooms.Length; num++) //last rooms are 2by2
						{
							if (rooms[num].GetComponent<RoomType>().size == 1)
							{
								if (exitsGridVertical[i - 1, j] ==
								   rooms[num].GetComponent<RoomType>().exitsGridVertical[0] ||
								   exitsGridVertical[i - 1, j] == -1)
								{
									if (exitsGridVertical[i, j] ==
									   rooms[num].GetComponent<RoomType>().exitsGridVertical[1] ||
									   exitsGridVertical[i, j] == -1)
									{
										if (exitsGridHorizontal[i - 1, j] ==
										   rooms[num].GetComponent<RoomType>().exitsGridHorizontal[0] ||
										   exitsGridHorizontal[i - 1, j] == -1)
										{
											if (exitsGridHorizontal[i - 1, j + 1] ==
											   rooms[num].GetComponent<RoomType>().exitsGridHorizontal[1] ||
											   exitsGridHorizontal[i - 1, j + 1] == -1)
											{
												pos = new Vector2((i - 1) * 10, ((int)(0.5f * levelSizeY) - (j)) * 10);
												roomSpawnPoints.Add(pos);
												roomIndexAtSpawnPoints.Add(num);
												num = rooms.Length;
											}
										}
									}
								}
							}
						}
					}
				}

				if(exitsGridHorizontal[i, j] == 1)
				{
					if(roomGrid[i, j - 1] == 0 && roomGrid[i, j] == 1) //top
					{
						for (int num = 0; num < rooms.Length; num++) //last rooms are 2by2
						{
							if (rooms[num].GetComponent<RoomType>().size == 1)
							{
								if (exitsGridVertical[i, j - 1] ==
								   rooms[num].GetComponent<RoomType>().exitsGridVertical[0] ||
								   exitsGridVertical[i, j - 1] == -1)
								{
									if (exitsGridVertical[i + 1, j - 1] ==
									   rooms[num].GetComponent<RoomType>().exitsGridVertical[1] ||
									   exitsGridVertical[i + 1, j - 1] == -1)
									{
										if (exitsGridHorizontal[i, j - 1] ==
										   rooms[num].GetComponent<RoomType>().exitsGridHorizontal[0] ||
										   exitsGridHorizontal[i, j - 1] == -1)
										{
											if (exitsGridHorizontal[i, j] ==
											   rooms[num].GetComponent<RoomType>().exitsGridHorizontal[1] ||
											   exitsGridHorizontal[i, j] == -1)
											{
												//Debug.Log("yay");
												pos = new Vector2((i) * 10, ((int)(0.5f * levelSizeY) - (j - 1)) * 10);
												roomSpawnPoints.Add(pos);
												roomIndexAtSpawnPoints.Add(num);
												num = rooms.Length;
											}
										}
									}
								}
							}
						}
					}
					else if(roomGrid[i, j] == 0 && roomGrid[i, j - 1] == 1) //bottom
					{
						for (int num = 0; num < rooms.Length; num++) //last rooms are 2by2
						{
							if (rooms[num].GetComponent<RoomType>().size == 1)
							{
								if (exitsGridVertical[i, j] == rooms[num].GetComponent<RoomType>().exitsGridVertical[0] ||
								   exitsGridVertical[i, j] == -1)
								{
									if (exitsGridVertical[i + 1, j] ==
									   rooms[num].GetComponent<RoomType>().exitsGridVertical[1] ||
									   exitsGridVertical[i + 1, j] == -1)
									{
										if (exitsGridHorizontal[i, j] ==
										   rooms[num].GetComponent<RoomType>().exitsGridHorizontal[0] ||
										   exitsGridHorizontal[i, j] == -1)
										{
											if (exitsGridHorizontal[i, j + 1] ==
											   rooms[num].GetComponent<RoomType>().exitsGridHorizontal[1] ||
											   exitsGridHorizontal[i, j + 1] == -1)
											{
												//Debug.Log("yay");
												pos = new Vector2((i) * 10, ((int)(0.5f * levelSizeY) - (j)) * 10);
												roomSpawnPoints.Add(pos);
												roomIndexAtSpawnPoints.Add(num);
												num = rooms.Length;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	void replaceRooms() // replaces rooms so that the level can continue to be generated
	{
		Debug.Log("rr");
		stopGeneration = true;
		while (spawnedRooms.Count < minRooms)
		{
			spawnedRooms = new List<GameObject>();
			stopGeneration = false;
			

			StartLevelGeneration();
		}
		
		rr = true;
	}

	public void spawnRooms()
	{
        if (spawnedBossRoom)
        {
			spawnedRooms.Add(Instantiate(bossRooms[bossRoomIndexAtSpawnPoint], (Vector2)bossSpawnPoint, Quaternion.identity));
			for (int j = 0; j < bossRooms[bossRoomIndexAtSpawnPoint].GetComponent<RoomType>().containsRoom.GetLength(0); j++)
			{ //  0-0,0  1-1,0  2-0,1  3-1,1

				if (bossRooms[bossRoomIndexAtSpawnPoint].GetComponent<RoomType>().containsRoom[j] == 1)
				{
					int roomX = 0;
					int roomY = 0;

					if (j == 1)
					{
						roomX = 1;

					}
					else if (j == 2)
					{
						roomY = 1;
					}
					else if (j == 3)
					{
						roomX = 1;
						roomY = 1;

					}
					pos = new Vector2((bossSpawnPoint).x + roomX * 10, (bossSpawnPoint).y - roomY * 10);
					Instantiate(fog, pos, Quaternion.identity);
				}
			}
		}
		
		for (int i = 0; i < roomSpawnPoints.Count; i++)
		{
			spawnedRooms.Add(Instantiate(rooms[(int) roomIndexAtSpawnPoints[i]], (Vector2) roomSpawnPoints[i], Quaternion.identity));
			if (rooms[(int)roomIndexAtSpawnPoints[i]].GetComponent<RoomType>().size == 1){
				Instantiate(fog, (Vector2)roomSpawnPoints[i], Quaternion.identity);
			}else if (rooms[(int)roomIndexAtSpawnPoints[i]].GetComponent<RoomType>().size == 2)
            {
				for(int j = 0; j < rooms[(int)roomIndexAtSpawnPoints[i]].GetComponent<RoomType>().containsRoom.GetLength(0); j++)
                { //  0-0,0  1-1,0  2-0,1  3-1,1
			
					if (rooms[(int)roomIndexAtSpawnPoints[i]].GetComponent<RoomType>().containsRoom[j] == 1)
                    {
						int roomX = 0;
						int roomY = 0;
						
						if (j == 1)
						{
							roomX = 1;

						}
						else if (j == 2)
						{
							roomY = 1;
						}
						else if (j == 3)
						{
							roomX = 1;
							roomY = 1;

						}
						pos = new Vector2( ((Vector2)roomSpawnPoints[i]).x + roomX*10, ((Vector2)roomSpawnPoints[i]).y - roomY*10);
						Instantiate(fog, pos, Quaternion.identity);
					}
                }
			}
			
			}
		for (int i = 0; i < exitsGridHorizontal.GetLength(0); i++)
		{
			for (int j = 0; j < exitsGridHorizontal.GetLength(1); j++)
			{
				if(exitsGridHorizontal[i,j] == 2)
                {
					pos = new Vector2((i) * 10, -5+((int)(0.5f * levelSizeY) - (j - 1)) * 10);
					Instantiate(fogTag, pos, Quaternion.identity);
				}else if (exitsGridHorizontal[i, j] == 1)
				{
					pos = new Vector2((i) * 10, -5 + ((int)(0.5f * levelSizeY) - (j - 1)) * 10);
					Instantiate(door, pos, Quaternion.identity);
				}


			}
		}
		for (int i = 0; i < exitsGridVertical.GetLength(0); i++)
		{
			for (int j = 0; j < exitsGridVertical.GetLength(1); j++)
			{
				if (exitsGridVertical[i, j] == 2)
				{
					pos = new Vector2((i) * 10-5,((int)(0.5f * levelSizeY) - (j)) * 10);
					Instantiate(fogTag, pos, Quaternion.identity);
				}else if (exitsGridVertical[i, j] == 1)
				{
					pos = new Vector2((i) * 10 - 5, ((int)(0.5f * levelSizeY) - (j)) * 10);
					Instantiate(door, pos, Quaternion.identity);
				}


			}
		}
	}


	int getFullRooms() // returns the number of 1by1 room spots that are occpied by any size room
	{
		int num = 0;
		for (int i = 0; i < roomGrid.GetLength(0); i++)
		{
			for (int j = 0; j < roomGrid.GetLength(1); j++)
			{
				if(roomGrid[i, j] == 1)
				{
					num++;
				}
			}
		}

		return num;
	}

	public void SetSeed(int newSeed)
	{
		seed = newSeed;
		Random.InitState(seed);
	}
}
