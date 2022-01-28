// Copyright (c) 2021 RogueWare

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomType : MonoBehaviour
{
    //info for box generationn
    public int type;

    // info for branch generation
    public int size; //dimensions

    //read left to right, then top to bottom
    public int[] exitsGridVertical; // 0 == wall, 1 == exit, 2 == walkthrough, -1 == empty
    public int[] exitsGridHorizontal; // 0 == wall, 1 == exit, 2 == walkthrough, -1 == empty
    public int[] containsRoom; // 0 == empty, 1 == room
    public void RoomDestruction()
    {
        Destroy(gameObject);
    }
    

    
}
