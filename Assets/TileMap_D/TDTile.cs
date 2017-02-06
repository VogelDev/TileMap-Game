
public class TDTile {

    public const int TILE_UNKNOWN = 0;
    public const int TILE_FLOOR = 1;
    public const int TILE_WALL = 2;
    public const int TILE_STONE = 3;

    public int type = TILE_UNKNOWN;
    int x;
    int y;
    int w;
    int h;
    public bool wall;
    TDTile[,] grid;

    public TDTile(int x, int y, int w, int h, bool wall, TDTile[,] grid)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        this.wall = wall;
        this.grid = grid;
    }

}
