using UnityEngine;
using System.Collections.Generic;

public class DTileMap
{

    protected class DRoom
    {
        public int left;
        public int top;
        public int width;
        public int height;

        public bool isConnected = false;

        public int right
        {
            get { return left + width - 1; }
        }

        public int bottom
        {
            get { return top + height - 1; }
        }

        public int center_x
        {
            get { return left + width / 2; }
        }

        public int center_y
        {
            get { return top + height / 2; }
        }

        public bool CollidesWith(DRoom other)
        {
            return !(right < other.left - 1 || left > other.right + 1 || top > other.bottom + 1 || bottom < other.top - 1);
        }

    }

    TDTile[,] map_data;
    int size_x;
    int size_y;
    List<DRoom> rooms;

    /*
        0 = unknown
        1 = floor
        2 = wall
        3 = stone
     */

    /*
    public DTileMap(int size_x, int size_y)
    {
        this.size_x = size_x;
        this.size_y = size_y;

        map_data = new int[this.size_x, this.size_y];
        for (int x = 0; x < size_x; x++)
        {
            for (int y = 0; y < size_y; y++)
            {
                map_data[x, y] = 3;
            }
        }

        rooms = new List<DRoom>();

        int maxFails = 10;

        while (rooms.Count < 10)
        {
            int rsx = Random.Range(10, size_x / 4 + 1);
            int rsy = Random.Range(10, size_y / 4 + 1);

            DRoom r = new DRoom();
            r.left = Random.Range(1, size_x - rsx);
            r.top = Random.Range(1, size_y - rsy);
            r.width = rsx;
            r.height = rsy;

            if (!RoomCollides(r))
            {
                rooms.Add(r);
                MakeRoom(r, 1);
            }
            else
            {
                if (--maxFails <= 0)
                {
                    break;
                }
            }
        }

        

        for (int i = 0; i < rooms.Count; i++)
        {
            if (!rooms[i].isConnected)
            {
                int j = Random.Range(1, rooms.Count);

                BuildCorridor(rooms[i], rooms[(i + j) % rooms.Count]);
            }
        }

        MakeWalls();

        MakeRoom(rooms[0], 0);
    }
    */

    public DTileMap(int size_x, int size_y)
    {
        this.size_x = size_x;
        this.size_y = size_y;

        BSPMap map = new BSPMap(size_x, size_y, 0,0,1, 1, new Vector2(), new Vector2(size_x, size_y), true, .3f);

        map_data = map.Build();
    }
    
    void MakeWalls()
    {
        for (int x = 0; x < size_x; x++)
        {
            for (int y = 0; y < size_y; y++)
            {
                if (map_data[x, y].type == 3 && HasAdjacentFloor(x, y))
                {
                    map_data[x, y].type = 2;
                }
            }
        }
    }

    bool HasAdjacentFloor(int x, int y)
    {
        if (x > 0 && map_data[x - 1, y].type == 1)
        {
            return true;
        }
        if (x < size_x - 1 && map_data[x + 1, y].type == 1)
        {
            return true;
        }
        if (y > 0 && map_data[x, y - 1].type == 1)
        {
            return true;
        }
        if (y < size_y - 1 && map_data[x, y + 1].type == 1)
        {
            return true;
        }

        if (x > 0 && y > 0 && map_data[x - 1, y - 1].type == 1)
        {
            return true;
        }
        if (x < size_x - 1 && y < size_y - 1 && map_data[x + 1, y + 1].type == 1)
        {
            return true;
        }

        if (x > 0 && y < size_y - 1 && map_data[x - 1, y + 1].type == 1)
        {
            return true;
        }

        if (x < size_x - 1 && y > 0 && map_data[x + 1, y - 1].type == 1)
        {
            return true;
        }

        return false;
    }

    bool RoomCollides(DRoom r)
    {
        foreach (DRoom r2 in rooms)
        {
            if (r.CollidesWith(r2))
            {
                return true;
            }
        }
        return false;
    }


    public int GetTileAt(int x, int y)
    {
        return map_data[x, y].type;
    }

    void MakeRoom(DRoom r, int tileType)
    {
        for (int x = 0; x < r.width; x++)
        {
            for (int y = 0; y < r.height; y++)
            {
                map_data[r.left + x, r.top + y].type = tileType;
            }
        }
    }

    void BuildCorridor(DRoom r1, DRoom r2)
    {
        int x = r1.center_x;
        int y = r1.center_y;

        while (x != r2.center_x)
        {
            map_data[x, y].type = 1;
            x += x < r2.center_x ? 1 : -1;
        }

        while (y != r2.center_y)
        {
            map_data[x, y].type = 1;
            y += y < r2.center_y ? 1 : -1;
        }

        r1.isConnected = true;
        r2.isConnected = true;
    }

    public Vector2 GetStart()
    {
        return new Vector2(rooms[0].center_x, rooms[0].center_y);
    }

}
