using System.Collections.Generic;
using UnityEngine;

public class BSPMap
{

    // This is a Binary Space Partitioning (BSP) map generator strategy,
    // tailored to make rogue-like square room dungeons.
    // https://en.wikipedia.org/wiki/Binary_space_partitioning
    // This could be improved by adding different types of rooms other than
    // rectangle spaces, plus the connecting hallways are naive at best.

    int cols;
    int rows;
    int x;
    int y;
    int w;
    int h;
    bool allowDiagonals;
    float percentWalls;
    BSPTree mainTree;
    TDTile[,] grid;
    List<Vector2> path;
    Vector2 start;
    Vector2 end;

    bool DISCARD_BY_RATIO = true;
    float H_RATIO = 0.45f;
    float W_RATIO = 0.45f;

    int N_ITERATIONS = 4;


    public BSPMap(int cols, int rows, int x, int y, int w, int h, Vector2 start, Vector2 end, bool allowDiagonals, float percentWalls)
    {
        Debug.Log("Starting map");
        this.cols = cols;
        this.rows = rows;
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        this.allowDiagonals = allowDiagonals;

        this.DISCARD_BY_RATIO = true;
        this.H_RATIO = 0.45f;
        this.W_RATIO = 0.45f;

        this.N_ITERATIONS = 1;

        this.grid = new TDTile[rows, cols];
        this.path = new List<Vector2>();
        this.start = start;
        this.end = end;
    }
    // Initialize the grid
    public TDTile[,] Build()
    {

        Debug.Log("Building map");
        for (var i = 0; i < this.cols; i++)
        {
            for (var j = 0; j < this.rows; j++)
            {
                this.grid[i, j] = new TDTile(i, j, (int)this.w / this.cols, (int)this.h / this.rows, true, this.grid);
            }
        }

        var mainContainer = new BSPContainer(1, 1, this.cols - 2, this.rows - 2);
        this.mainTree = this.SplitContainer(mainContainer, this.N_ITERATIONS);

        // Write the rooms into the grid
        var leafs = this.mainTree.GetLeafs();
        for (var i = 0; i < leafs.Count; i++)
        {
            var room = new BSPRoom(leafs[i]);
            room.RemoveWallsFromGrid(this.grid);
        }
        // Write the halls into the grid
        this.CarvePath(this.mainTree, this.grid);
        this.CarveEntranceAndExit(this.grid, this.start, this.end);

        return grid;
    }


    // Carve out the hallways which connect rooms
    private void CarvePath(BSPTree tree, TDTile[,] grid)
    {
        if (tree.lchild == null || tree.rchild == null)
        {
            return;
        }
        int leftX = (int)Mathf.Floor(tree.lchild.leaf.center.x);
        int leftY = (int)Mathf.Floor(tree.lchild.leaf.center.y);
        int rightX = (int)Mathf.Floor(tree.rchild.leaf.center.x);
        int rightY = (int)Mathf.Floor(tree.rchild.leaf.center.y);

        int x = leftX;
        int y = leftY;
        while (x != rightX)
        {
            grid[x, y].wall = false;
            x += rightX - leftX > 0 ? 1 : -1;
        }
        while (y != rightY)
        {
            grid[x, y].wall = false;
            y += rightY - leftY > 0 ? 1 : -1;
        }

        this.CarvePath(tree.lchild, grid);
        this.CarvePath(tree.rchild, grid);
    }

    // Make sure the starting end ending points connect to the rest of the map.
    private void CarveEntranceAndExit(TDTile[,] grid, Vector2 startPoint, Vector2 endPoint)
    {
        // Entrance
        int x = (int)startPoint.x;
        int y = (int)startPoint.y;
        while (grid[x, y].wall)
        {
            grid[x++, y].wall = false;
            grid[x, y++].wall = false;
        }
        x = (int)endPoint.x;
        y = (int)endPoint.y;
        while (grid[x, y].wall)
        {
            grid[x--, y].wall = false;
            grid[x, y--].wall = false;
        }
    }

    // Recursively divide the region until iteration count is met, or the
    // child regions are the right size.
    private BSPTree SplitContainer(BSPContainer container, int iteration)
    {
        BSPTree root = new BSPTree(container);
        if (iteration > 0)
        {
            var sr = this.RandomSplit(container);
            root.lchild = this.SplitContainer(sr[0], iteration - 1);
            root.rchild = this.SplitContainer(sr[1], iteration - 1);
        }
        return root;
    }
    // Divide a container region into two child regions
    private BSPContainer[] RandomSplit(BSPContainer container)
    {
        BSPContainer region1, region2;
        if (Random.Range(0, 2) == 0)
        {
            // Vertical
            region1 = new BSPContainer(
                container.x, container.y,             // region1.x, region1.y
                Random.Range(1, container.w), container.h   // region1.w, region1.h
            );
            region2 = new BSPContainer(
                container.x + region1.w, container.y,      // region2.x, region2.y
                container.w - region1.w, container.h       // region2.w, region2.h
            );

            if (this.DISCARD_BY_RATIO)
            {
                float region1_w_ratio = region1.w / region1.h;
                float region2_w_ratio = region2.w / region2.h;
                if (region1_w_ratio < this.W_RATIO || region2_w_ratio < this.W_RATIO)
                {
                    return this.RandomSplit(container);
                }
            }
        }
        else {
            // Horizontal
            region1 = new BSPContainer(
                container.x, container.y,             // region1.x, region1.y
                container.w, Random.Range(1, container.h + 1)   // region1.w, region1.h
            );
            region2 = new BSPContainer(
                container.x, container.y + region1.h,      // region2.x, region2.y
                container.w, container.h - region1.h       // region2.w, region2.h
            );

            if (this.DISCARD_BY_RATIO)
            {
                var region1_h_ratio = region1.h / region1.w;
                var region2_h_ratio = region2.h / region2.w;
                if (region1_h_ratio < this.H_RATIO || region2_h_ratio < this.H_RATIO)
                {
                    return this.RandomSplit(container);
                    };
            }
        }
        return new BSPContainer[] { region1, region2 };
    }
    // TODO -- Figure out where to build
    //this.build();
}

// The tree tracks the leaf node (container) and it's partitioned children
public class BSPTree
{
    public BSPContainer leaf;
    public BSPTree lchild;
    public BSPTree rchild;
    public BSPTree(BSPContainer leaf)
    {
        this.leaf = leaf;
        this.lchild = null;
        this.rchild = null;
    }

    // returns only leafs which don't have children, as a flat array.
    public List<BSPContainer> GetLeafs()
    {
        if (this.lchild == null && this.rchild == null)
        {
            List<BSPContainer> leaves = new List<BSPContainer>();
            leaves.Add(this.leaf);
            return leaves;
        }
        else {
            List<BSPContainer> leaves = new List<BSPContainer>();
            leaves.AddRange(this.lchild.GetLeafs());
            leaves.AddRange(this.rchild.GetLeafs());
            return leaves;
        }
    }
}

// The container is the partitioned region to put a room into
public class BSPContainer
{
    public int x;
    public int y;
    public int w;
    public int h;
    public Vector2 center;

    public BSPContainer(int x, int y, int w, int h)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        this.center = new Vector2(this.x + (int)(this.w / 2), (int)(this.y + this.h / 2));
    }
}

// Used to create the actual room within the container space.
public class BSPRoom
{
    int x;
    int y;
    int w;
    int h;

    public BSPRoom(BSPContainer container)
    {
        this.x = container.x + (int)Mathf.Floor(Random.Range(0, Mathf.Floor(container.w / 3 + 1)));
        this.y = container.y + (int)Mathf.Floor(Random.Range(0, Mathf.Floor(container.h / 3 + 1)));
        this.w = container.w - (this.x - container.x);
        this.h = container.h - (this.y - container.y);
        this.w -= (int)Mathf.Floor(Random.Range(0, this.w / 3 + 1));
        this.h -= (int)Mathf.Floor(Random.Range(0, this.w / 3 + 1));
    }

    // We mark the empty spaces for the room
    public void RemoveWallsFromGrid(TDTile[,] grid)
    {
        for (var x = this.x; x < this.x + this.w; x++)
        {
            for (var y = this.y; y < this.y + this.h; y++)
            {
                grid[x, y].wall = false;
            }
        }
        //Decorate(grid);
    }
    // This decorator randomly applies obsticles to the room,
    // giving them more personality
    private void Decorate(TDTile[,] grid)
    {
        switch ((int)Mathf.Floor(Random.Range(0, 11)))
        {
            case 0:
            case 1:
                DecorateColumns(grid);
                break;
            case 2:
            case 3:
                DecorateCircle(grid);
                break;
            default:
                return;
        }
    }
    // Evenly spaced out columns
    private void DecorateColumns(TDTile[,] grid)
    {
        var spacing = (int)Mathf.Floor(Random.Range(3, 6));
        var cols = this.w / spacing;
        var rows = this.h / spacing;
        for (var x = this.x + 1; x < this.x + this.w - 1; x += spacing)
        {
            for (var y = this.y + 1; y < this.y + this.h - 1; y += spacing)
            {
                grid[x, y].wall = true;
            }
        }
    }
    // Hollow circle of random size.  At small resolutions,
    // usually diamond shaped.
    private void DecorateCircle(TDTile[,] grid)
    {
        int radius = (int)Mathf.Floor(Random.Range(2, Mathf.Min(this.w / 2, this.h / 2, 6) + 1));
        int x0 = (int)Mathf.Floor(this.x + this.w / 2) + (int)Mathf.Floor(Random.Range(-1, 3));
        int y0 = (int)Mathf.Floor(this.y + this.h / 2) + (int)Mathf.Floor(Random.Range(-1, 3)); ;

        int x = radius;
        int y = 0;
        int err = 0;

        while (x >= y)
        {
            grid[x0 + x, y0 + y].wall = true;
            grid[x0 + y, y0 + x].wall = true;
            grid[x0 - y, y0 + x].wall = true;
            grid[x0 - x, y0 + y].wall = true;
            grid[x0 - x, y0 - y].wall = true;
            grid[x0 - y, y0 - x].wall = true;
            grid[x0 + y, y0 - x].wall = true;
            grid[x0 + x, y0 - y].wall = true;

            if (err <= 0)
            {
                y += 1;
                err += 2 * y + 1;
            }
            if (err > 0)
            {
                x -= 1;
                err -= 2 * x + 1;
            }
        }
    }
}