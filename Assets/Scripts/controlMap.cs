using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Threading;
public class controlMap : MonoBehaviour
{
    // Start is called before the first frame update
    public Tilemap map;
    Dictionary<string, Tile> arrTiles;
    List<string> TilesName;
    string[] TileType;
    int roomcount;
    public List<Room> rooms;
    public int levelW = 100;
    public int levelH = 100;
    public int createRoomTime;
    private int[,] vis;
    int[,] id;
    public int[] fa;
    int[,,] probabilty = new int[4, 4, 2];
    (int, int)[] tempxy = new (int, int)[4];
    void Start()
    {
        TileType = new string[levelH*levelW];
        arrTiles = new Dictionary<string, Tile>();
        TilesName = new List<string>();
        vis = new int[levelH+1,levelW+1];
        rooms = new List<Room>();
        fa = new int[Mathf.Max(levelH,levelW)];
        id = new int[levelH+1, levelW+1];
        tempxy[0] = (0, 1);tempxy[1] = (1, 0);tempxy[2] = (0, -1);tempxy[3] = (-1, 0);
        createProbability();
        InitTile();
        roomcount = 0;
        initialMap();
        InitData();
        createRoom();
        createway();
        link();
        //StartCoroutine(testTime(0));
    }
    IEnumerator testTime(int x)
    {
        yield return new WaitForSeconds(1.0f);
        //Debug.Log(x);
        setground(0, x);
        if (x<10)
        StartCoroutine( testTime(x+1));
    }
    void createProbability()
    {
        probabilty[0, 0, 0] = 0;probabilty[0, 0, 1] = 10;
        probabilty[1, 0, 0] = 0; probabilty[1, 0, 1] = 7;
        probabilty[1, 1, 0] = 7; probabilty[1, 1, 1] = 10;
        probabilty[2, 0, 0] = 0; probabilty[2, 0, 1] = 5;
        probabilty[2, 1, 0] = 5; probabilty[2, 1, 1] = 8;
        probabilty[2, 2, 0] = 8; probabilty[2, 2, 1] = 10;
        probabilty[3, 0, 0] = 0; probabilty[3, 0, 1] = 5;
        probabilty[3, 1, 0] = 5; probabilty[3, 1, 1] = 7;
        probabilty[3, 2, 0] = 7; probabilty[3, 2, 1] = 9;
        probabilty[3, 3, 0] = 9; probabilty[0, 0, 1] = 10;


    }
    public void createmap()
    {
        deleteWay();
        deleteAlley();
        InitData();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    bool checkAlley(int x,int y)
    {
        //Debug.Log("checkalley");
        int noway = 0;
        for(int i=0;i<4;i++)
        {
            int tempx = x + tempxy[i].Item1, tempy = y + tempxy[i].Item2;
           // Debug.Log(tempx.ToString() + " " + tempy.ToString());
            if (!checkBoundary(tempx,tempy)) { noway++; continue; }
            if (id[tempx, tempy] == -1) noway++;
        }
        if (noway >= 3) return true;
        return false;
    }
    void dfs2(int x,int y)
    {
        Debug.Log(x.ToString() + " "+ y.ToString());
        setwall(x, y);
        id[x, y] = -1;
        vis[x, y] = 0;
        for (int i = 0; i < 4; i++)
        {
            int tempx = x + tempxy[i].Item1, tempy = y + tempxy[i].Item2;
            if (!checkBoundary(tempx, tempy)) continue;
            if (checkAlley(tempx, tempy)&&vis[tempx,tempy]!=0) dfs2(tempx, tempy);
        }
    }
    void deleteAlley()
    {
        Debug.Log("alley");
        bool flag = false;
        int x = 0, y = 0;
        while(flag==false)
        {
            flag = true;
            for (int i = 0; i < levelH; i++)
                if (flag)
                for (int j = 0; j < levelW; j++)
                    if(checkAlley(i,j)&&vis[i,j]!=0)
                    {
                        flag = false;
                        x = i;y = j;
                        break;
                    }
            if (flag == true) break;
            Debug.Log(x.ToString() + " " + y.ToString());
            dfs2(x, y);
        }
    }
    void deleteWay()
    {
        for (int i = 0; i < levelH; i++)
            for (int j = 0; j < levelW; j++)
                if (id[i, j] == -1)
                {
                    setwall(i, j);
                    vis[i, j] = 0;
                }
    }
    void dfs1(int x,int y,int father)
    {
        for(int i=0;i<4;i++)
        {
            int tempx=x+tempxy[i].Item1,tempy=y+tempxy[i].Item2;
            if (checkBoundary(tempx,tempy))
            if (vis[tempx,tempy]!=0&&id[tempx,tempy]!=father)
            {
                if (id[tempx,tempy]!=-1)fa[id[tempx,tempy]] = father;
                id[tempx, tempy] = father;
                dfs1(tempx, tempy, father);
            }
        }
    }
    int findfacount = 0;
    int findfa(int x)
    {
        //findfacount++;
        //if (findfacount > 100) return x;
        if (x  < 0) return -1;
        return fa[x]=fa[x] == x ? x : findfa(fa[x]);
    }
    void link()
    {
        Debug.Log(rooms.Count);
        for (int i = 0; i < rooms.Count; i++)
        {
            Room tmproom = rooms[i];
            bool[] isable = new bool[40];
            int x = (int)tmproom.roompos.x, y = (int)tmproom.roompos.y;
            Debug.Log("room:"+i+" "+x.ToString() + " " + y.ToString());
            (int, int)[] canlink = new (int, int)[40];
            (int, int)[] roomlink = new (int, int)[40];
            int linkcount = 0;
            for (int j = y; j < y+tmproom.roomW; j++)
            {
                if (checkBoundary(x - 2, j))
                {
                    if (vis[x - 2, j] != 0)
                    {
                        isable[linkcount] = false;
                        roomlink[linkcount] = (x, j);
                        canlink[linkcount++] = (x - 2, j);
                        
                    }
                }
                if (checkBoundary(x + tmproom.roomH + 1, j))
                {
                    if (vis[x + tmproom.roomH + 1, j] != 0)
                    {
                        isable[linkcount] = false;
                        roomlink[linkcount] = (x+tmproom.roomH-1, j);
                        canlink[linkcount++] = (x + tmproom.roomH + 1, j);
                    }
                }
            }

            for (int j = x; j < x+tmproom.roomH; j++)
            {
                if (checkBoundary(j, y - 2))
                {
                    if (vis[j, y - 2] != 0)
                    {
                        isable[linkcount] = false;
                        roomlink[linkcount] = (j, y);
                        canlink[linkcount++] = (j, y - 2);
                    }
                }
                if (checkBoundary(j, y + tmproom.roomW + 1))
                {
                    if (vis[j, y + tmproom.roomW + 1] != 0)
                    {
                        isable[linkcount] = false;
                        roomlink[linkcount] = (j, y+tmproom.roomW-1);
                        canlink[linkcount++] = (j, y + tmproom.roomW + 1);
                    }
                }
            }
            int seed = (int)Random.Range(0, linkcount - 1);
            isable[seed] = true;
            int tempx = canlink[seed].Item1, tempy = canlink[seed].Item2;
            x = roomlink[seed].Item1;y = roomlink[seed].Item2;
            setground((tempx + x) / 2, (tempy + y) / 2);
            //for(int j=0;j<linkcount;j++)Debug.Log(canlink[j].Item1.ToString() + " " + canlink[j].Item2+ " id"+id[canlink[j].Item1, canlink[j].Item2].ToString());  
            if (id[tempx, tempy] == -1)
            {
                id[(tempx + x) / 2, (tempy + y) / 2] = findfa(i);
                vis[(tempx + x) / 2, (tempy + y) / 2]= 1;
                dfs1(tempx, tempy, findfa(i));                
                Debug.Log(fa[i]);
            }
            else
            {
            Debug.Log(fa[id[tempx, tempy]]);
                //Debug.Log((x + tmproom.roomH - 1).ToString() + " " + (y + tmproom.roomW - 1));
                for (int j = (int)tmproom.roompos.x; j < (int)tmproom.roompos.x + tmproom.roomH; j++)
                    for (int k = (int)tmproom.roompos.y; k < (int)tmproom.roompos.y+tmproom.roomW; k++)
                        id[j, k] = findfa(id[tempx,tempy]);
                id[(tempx + x) / 2, (tempy + y) / 2] = findfa(id[tempx, tempy]);
                vis[(tempx + x) / 2, (tempy + y) / 2] = 1;
                fa[i] = findfa(id[tempx, tempy]);
            }
            Debug.Log(1);
            bool flag = false;
            for(int k=1;k<=linkcount;k++)
            {
                flag = true;
                int nowlink = 0;
                for(int j=0;j<linkcount;j++)
                {
                    tempx = canlink[j].Item1; tempy = canlink[j].Item2;
                    if (findfa(id[tempx, tempy]) != findfa(i))
                    {
                        isable[j] = true;
                        nowlink = j;
                        flag = false;break;
                    }
                }
                if (flag==false)
                {
                    Debug.Log(tempx.ToString() + " " + tempy.ToString());
                    setground((tempx + x) / 2, (tempy + y) / 2);
                    x = roomlink[nowlink].Item1; y = roomlink[nowlink].Item2;
                    id[(tempx + x) / 2, (tempy + y) / 2] = findfa(i);
                    vis[(tempx + x) / 2, (tempy + y) / 2] = 1;
                    if (id[tempx,tempy]!=-1)fa[id[tempx, tempy]] = findfa(i);
                    dfs1(tempx, tempy, findfa(i));
                }
            }
            Debug.Log("fa"+fa[i].ToString());
            for (int j = 0; j < linkcount; j++)
                if(isable[j]==false)
                {
                    float seed1 = Random.Range(0, 10);
                    if (seed1<=1)
                    {
                        x = roomlink[j].Item1; y = roomlink[j].Item2;
                        tempx = canlink[j].Item1; tempy = canlink[j].Item2;
                        setground((tempx + x) / 2, (tempy + y) / 2);
                        id[(tempx + x) / 2, (tempy + y) / 2] = findfa(i);
                        vis[(tempx + x) / 2, (tempy + y) / 2] = 1;
                    }
                    isable[j] = true;
                }
        }
        for (int i = 0; i < roomcount; i++) findfa(i);
    }
    void initialMap()
    {
        for (int i = 0; i < levelH * levelW; i++)
            TileType[i] = "wall";
        for (int i = 0; i < levelH; i++)
            for (int j = 0; j < levelW; j++)
            {
                vis[i, j] = 0;
                id[i, j] = -1;
            }
    }
   
    bool checkBoundary(int i,int j)
    {
        if (0 <= i && i < levelH && 0 <= j && j < levelW) return true;
        return false;
    }
    bool checkmap(int x,int y)
    {
        for (int i = x - 2; i <= x + 2; i++)
            for (int j = y - 2; j <= y + 2; j++)
                if (checkBoundary(i,j))
                    if (vis[i, j] == 1) return false;
        return true;
    }

    void setground(int x,int y)
    {
        TileType[x * levelW + y] = "ground";
        map.SetTile(new Vector3Int(y, x, 0), arrTiles[TileType[x * levelW + y]]);
    }
    void setway(int x,int y)
    {
        TileType[x * levelW + y] = "way";
        map.SetTile(new Vector3Int(y, x, 0), arrTiles[TileType[x * levelW + y]]);
    }
    void setwall(int x, int y)
    {
        TileType[x * levelW + y] = "wall";
        map.SetTile(new Vector3Int(y, x, 0), arrTiles[TileType[x * levelW + y]]);
    }
    int checkRange(int seed,int wayCount)
    {
        //Debug.Log(seed);
        for (int i = 0; i < wayCount; i++)
        {
            if (probabilty[wayCount-1, i, 0] <= seed && seed <= probabilty[wayCount-1, i, 1])
                return i;
            //Debug.Log(probabilty[wayCount - 1, i, 0].ToString() + " "+ probabilty[wayCount - 1, i, 1].ToString());
        }
        return 0;
    }
    int dfscount = 0;
    bool checkway(int x, int y)
    {
        if (!checkBoundary(x, y)) return false;
        if (vis[x, y] != 0) return false;
        for (int i = x - 1; i <= x + 1; i++)
            for (int j = y - 1; j <= y + 1; j++)
                if (checkBoundary(i, j))
                    if (vis[i, j] == 1) return false;
        int wayaround = 0;
            //Debug.Log(x+" "+y+"around");
        for(int i=0;i<4;i++)
        {
            int tempx = x+tempxy[i].Item1, tempy = y+tempxy[i].Item2;
            if (!checkBoundary(tempx, tempy)) continue;
            //Debug.Log(tempx + "          " + tempy+" "+vis[tempx,tempy].ToString());
                if (vis[tempx, tempy] == 2) wayaround++;
        }//Debug.Log(wayaround);
        if (wayaround >= 2) return false;
        return true;
    }
    IEnumerator craWay(int x,int y)
    {
        yield return new WaitForSeconds(0f);
        dfs(x, y);
    }
    void dfs(int x,int y)
    {
        //Thread.Sleep(100);
        //Debug.Log("pos:"+x.ToString() + " " + y.ToString());
        int wayCount = 0;
        (int, int)[] walkableWay = new (int, int)[10];int tmpwayCount;
        (int, int)[] tempwalkableWay = new (int, int)[10];
        setground(x, y);
        map.SetTile(new Vector3Int(y, x, 0), arrTiles[TileType[x * levelW + y]]);
        vis[x, y] = 2;
        if (checkway(x, y + 1)) walkableWay[wayCount++] = (x, y + 1);
        if (checkway(x+1, y)) walkableWay[wayCount++] = (x+1, y);
        if (checkway(x, y - 1)) walkableWay[wayCount++] = (x, y - 1);
        if (checkway(x-1, y)) walkableWay[wayCount++] = (x-1, y);
        if (wayCount != 0)
        {
            //Debug.Log(wayCount);
            //for (int i = 0; i < wayCount; i++) Debug.Log(walkableWay[i].Item1.ToString() + " " + walkableWay[i].Item2.ToString());
            //Debug.Log("wayout");
            int seed = (int)Random.Range(0, 10);
            int nowWay = checkRange(seed, wayCount);
            //Debug.Log(nowWay);
            for (int i = 0; i < wayCount; i++)
            {
                tmpwayCount = wayCount;
                int tempx = walkableWay[nowWay].Item1, tempy = walkableWay[nowWay].Item2;
                for (int j = 0; j < wayCount; j++)
                    tempwalkableWay[j] = walkableWay[j];
                if (checkway(tempx, tempy)) dfs(tempx,tempy);
                //Debug.Log("nowWay=" + nowWay.ToString());
                wayCount = tmpwayCount;
                for (int j = 0; j < wayCount; j++)
                    walkableWay[j] = tempwalkableWay[j];
                nowWay = (nowWay + 1) % wayCount;
            }
        }
        //dfscount++;
        //if (dfscount >= 200) return;
        
    }
    public void createway()
    {
        bool flag = false;
        while(flag==false)
        {
            int x = 0, y = 0;
            flag = true;
            for (int i = 0; i < levelH; i++)
                if (flag == true)
                    for (int j = 0; j < levelW; j++)
                        if (checkway(i, j) == true)
                        {
                            flag = false;
                            x = i; y = j;
                            break;
                        }
            if (flag == true) break;
            dfs(x, y);
        }
    }
    void createRoom()
    {
        for(int i=0;i<createRoomTime;i++)
        {
            bool flag = false;
            int roomW=Random.Range(3, 8);
            int roomH = Random.Range(3, 8);
            Vector2 pos = new Vector2(Random.Range(1, levelW)-1, Random.Range(1,levelH)-1);
            if (pos.x + roomH - 1 >= levelH || pos.y + roomW - 1 >= levelW) continue;
            for (int j = (int)pos.x; j < (int)pos.x + roomH ; j++)
                if (flag == false)
                    for (int k = (int)pos.y; k < (int)pos.y + roomW ; k++)
                    {
                        if (checkmap(j,k)==false)
                        {
                            flag = true;
                            break;
                        }
                    }
            
            if (flag==false)
            {
                for (int j = (int)pos.x; j < (int)pos.x + roomH; j++)
                    for (int k = (int)pos.y; k < (int)pos.y + roomW; k++)
                    {
                        setground(j, k);
                        vis[j, k] = 1;
                        id[j, k] = roomcount;
                        fa[roomcount] = roomcount;
                    }
                Room newroom = new Room();
                newroom.set(roomcount++, roomW, roomH, pos);
                rooms.Add(newroom);
            }
        }
        InitData();
    }
    void InitTile()
    {
        AddTile("ground", "Image/ground");
        AddTile("wall", "Image/wall");
        AddTile("way", "Image/ground");
    }
    void InitData()
    {
        for (int i = 0; i < levelH; i++)
        {
            for (int j = 0; j < levelW; j++)
            {
                map.SetTile(new Vector3Int(j, i, 0), arrTiles[TileType[i * levelW + j]]);
            }
        }
        Debug.Log("Initiaon is finished");
    }
    void AddTile(string labelName,string spritePath)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        Sprite tmp = Resources.Load<Sprite>(spritePath);
        Debug.Log(spritePath);
        tile.sprite = tmp;
        arrTiles.Add(labelName, tile);
        TilesName.Add(labelName);
    }
}
