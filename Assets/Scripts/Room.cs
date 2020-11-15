using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int id;
    public int roomW;
    public int roomH;
    public Vector2 roompos;

    public void set(int roomid,int roomw,int roomh,Vector2 pos)
    {
        id = roomid;
        roomW = roomw;
        roomH = roomh;
        roompos = pos;
    }
}
