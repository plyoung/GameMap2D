using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A map/grid of tiles. The GameMapsAsset contains a list of these maps. 
/// The map's 0x0 point is considered to be at the bottom-left and WxH at top-right
/// </summary>
[System.Serializable]
public class GameMap
{
	/// <summary> a unique identifier for the map </summary>
	public int id;

	/// <summary> a unique name by which map can be identified </summary>
	public string ident;

	/// <summary> width of the map (determines how big grid is) </summary>
	public int width;

	/// <summary> height of map (determines how big grid is) </summary>
	public int height;

	/// <summary> The grid/map. -1 is an empty tile, else a value related to GameMapTile.id will be present 
    /// This is also known as layer-0 or the default layer when there are movethan one layer in the map.
    /// </summary>
	public int[] grid = new int[0];

    /// <summary> The GameMap can have additional layers. In that case the grid[] field above will be the default, or layer-0. </summary>
    public GameMapLayer[] layers = new GameMapLayer[0];

	// *** extra properties related to this map
	// add any addition serializable types here and they will appear
	// for editing in map properties section of the map editor

	//public Color backgroundColor = Color.black;

	// ----------------------------------------------------------------------------------------------------------------

	/// <summary> Return the grid index of a tile at position (x,y). No error checking, x or y value is out of bounds, to improve performance. </summary>
	public int PositionToIdx(int x, int y)
	{
		return (y * width + x);
	}

	/// <summary> Get an x and y position, given specific index into grid[]. No error checking, idx out of bounds, to improve performance. </summary>
	public void IdxToPosition(int idx, out int x, out int y)
	{
		y = idx / width;
		x = idx - (y * width);
	}

    /// <summary> This returns layers.Length + 1 since grid[] is layer-0. </summary>
    public int LayerCount { get { return layers.Length + 1;  } }

    /// <summary> Returns data from a layer. layerIdx=0 is the same as reading GameMap.grid[] while any higher value (1+) will be data from GameMap.layers[layerIdx-1].grid </summary>
    public int[] GetlayerData(int layerIdx)
    {
        return (layerIdx == 0 ? grid : layers[layerIdx - 1].grid);
    }

	/// <summary> Return an array of GameMapIdxIdPair with the ID and Index of tiles neighbouring the one at x,y.
	/// Array is always length 4 else 8 if includeDiagonal=true.
	/// idx or id = -1 represents no tile or areas outside of map grid.
	/// Result starts at tile above (north) and go around clockwise. </summary>
	public GameMapIdxIdPair[] GetNeighbours(int x, int y, bool includeDiagonal = false, int layerIdx = 0)
	{
        int[] g = (layerIdx == 0 ? grid : layers[layerIdx - 1].grid);
		if (includeDiagonal)
		{
			return new GameMapIdxIdPair[]
				{
					(y < height - 1 ?                   new GameMapIdxIdPair(g[((y + 1) * width + (x + 0))], ((y + 1) * width + (x + 0))) : new GameMapIdxIdPair(-1, -1)),
					(x < width - 1 && y < height - 1 ?  new GameMapIdxIdPair(g[((y + 1) * width + (x + 1))], ((y + 1) * width + (x + 1))) : new GameMapIdxIdPair(-1, -1)),
					(x < width - 1 ?                    new GameMapIdxIdPair(g[((y + 0) * width + (x + 1))], ((y + 0) * width + (x + 1))) : new GameMapIdxIdPair(-1, -1)),
					(x < width - 1 && y > 0 ?           new GameMapIdxIdPair(g[((y - 1) * width + (x + 1))], ((y - 1) * width + (x + 1))) : new GameMapIdxIdPair(-1, -1)),
					(y > 0 ?                            new GameMapIdxIdPair(g[((y - 1) * width + (x + 0))], ((y - 1) * width + (x + 0))) : new GameMapIdxIdPair(-1, -1)),
					(x > 0 && y > 0 ?                   new GameMapIdxIdPair(g[((y - 1) * width + (x - 1))], ((y - 1) * width + (x - 1))) : new GameMapIdxIdPair(-1, -1)),
					(x > 0 ?                            new GameMapIdxIdPair(g[((y + 0) * width + (x - 1))], ((y + 0) * width + (x - 1))) : new GameMapIdxIdPair(-1, -1)),
					(x > 0 && y < height - 1 ?          new GameMapIdxIdPair(g[((y + 1) * width + (x - 1))], ((y + 1) * width + (x - 1))) : new GameMapIdxIdPair(-1, -1))
				};
		}
		else
		{
			return new GameMapIdxIdPair[]
				{
					(y < height - 1 ?   new GameMapIdxIdPair(g[((y + 1) * width + (x + 0))], ((y + 1) * width + (x + 0))) : new GameMapIdxIdPair(-1, -1)),
					(x < width - 1 ?    new GameMapIdxIdPair(g[((y + 0) * width + (x + 1))], ((y + 0) * width + (x + 1))) : new GameMapIdxIdPair(-1, -1)),
					(y > 0 ?            new GameMapIdxIdPair(g[((y - 1) * width + (x + 0))], ((y - 1) * width + (x + 0))) : new GameMapIdxIdPair(-1, -1)),
					(x > 0 ?            new GameMapIdxIdPair(g[((y + 0) * width + (x - 1))], ((y + 0) * width + (x - 1))) : new GameMapIdxIdPair(-1, -1))
				};
		}
	}

	// ----------------------------------------------------------------------------------------------------------------

	/// <summary> This will destroy the exiting map. Use Resize if you want to keep existing tile data. Mainly used by editor. </summary>
	public void SetSize(int w, int h)
	{
		if (w < 1) w = 1;
		if (h < 1) h = 1;

		width = w;
		height = h;
		grid = new int[width * height];
        foreach(GameMapLayer l in layers) l.grid = new int[width * height];
        ClearMap();
	}

    /// <summary> Resizes map, keeping existing tile data. Use SetSize() to create a clean resized map. Mainly used by editor. </summary>
    public void Resize(int w, int h)
	{
		if (w < 1) w = 1;
		if (h < 1) h = 1;

		int[] old = grid;
		grid = new int[w * h];
        ClearLayer(0);

		for (int y = 0; y < h; y++)
		{
			if (y >= height) break;
			for (int x = 0; x < w; x++)
			{
				if (x >= width) break;
				grid[(y * w + x)] = old[(y * width + x)];
			}
		}

        for (int i =0; i < layers.Length; i++)
        {
            old = layers[i].grid;
            layers[i].grid = new int[w * h];
            ClearLayer(i+1);

            for (int y = 0; y < h; y++)
            {
                if (y >= height) break;
                for (int x = 0; x < w; x++)
                {
                    if (x >= width) break;
                    layers[i].grid[(y * w + x)] = old[(y * width + x)];
                }
            }
        }

        width = w;
		height = h;
	}

	/// <summary> Fill grid with empty tiles (-1). Mainly used by editor. </summary>
	public void ClearMap()
	{
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = -1;
        }

        foreach (GameMapLayer l in layers)
        {
            for (int i = 0; i < l.grid.Length; i++)
            {
                l.grid[i] = -1;
            }
        }
    }

    /// <summary> This set the size of the layer. This will destroy any exsiting tile placements in the layer.
    /// idx=0 refers to the grid[] while anything higher will be layers[idx-1].grid </summary>
    public void InitLayer(int idx)
    {
        if (idx == 0)
        {
            grid = new int[width * height];
            ClearLayer(0);
        }
        else
        {
            layers[idx-1].grid = new int[width * height];
            ClearLayer(idx);
        }
    }

    /// <summary> idx=0 refers to the grid[] while anything higher will be layers[idx-1].grid </summary>
    public void ClearLayer(int idx)
    {
        if (idx == 0)
        {
            for (int i = 0; i < grid.Length; i++)
            {
                grid[i] = -1;
            }
        }
        else
        {
            idx--;
            for (int i = 0; i < layers[idx].grid.Length; i++)
            {
                layers[idx].grid[i] = -1;
            }
        }
    }

	public override string ToString()
	{
		return ident;
	}

	// ----------------------------------------------------------------------------------------------------------------
}
