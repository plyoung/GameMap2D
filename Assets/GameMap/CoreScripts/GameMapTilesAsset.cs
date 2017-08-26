using System.Collections.Generic;
using UnityEngine;


/// <summary> Container for all defined tiles. </summary>
[System.Serializable]
public class GameMapTilesAsset : ScriptableObject
{
	[SerializeField] public List<GameMapTile> tiles = new List<GameMapTile>();
	[SerializeField] public List<AutoTile> autoTiles = new List<AutoTile>();
	[SerializeField] private int nextTileId = 1;
	[SerializeField] private int nextAutoTileId = 1;

	[System.Serializable]
	public class AutoTile
	{
		public int id;
		public GameMapTile[] tiles; // 16 or 47 depending on system using including corners or not
	}

	private Dictionary<int, GameMapTile> _cache = null; // <Tile ID, Tile Definition>

	// ----------------------------------------------------------------------------------------------------------------

	/// <summary> Get the Tile by its unique ID. Remember that the ID is the value stored in 
	/// GameMap.grid, not the tile's index in the GameMapTilesAsset.tiles list. </summary>
	public GameMapTile GetTile(int id)
	{
		if (id < 0) return null;

		if (_cache == null)
		{
			_cache = new Dictionary<int, GameMapTile>();
			foreach (GameMapTile t in tiles)
			{
				_cache.Add(t.id, t);
			}

			foreach (AutoTile at in autoTiles)
			{
				_cache.Add(at.tiles[0].id, at.tiles[0]);

				// copy the mainTile info into all auto-tile pieces, except for the sprite
				// note; skip [0] on purpose since it is the main tile
				for (int i = 1; i < at.tiles.Length; i++)
				{
					Sprite _sp = at.tiles[i].sprite;
					int _id = at.tiles[i].id;
					at.tiles[0].CopyTo(at.tiles[i]);
					at.tiles[i].id = _id;
					at.tiles[i].sprite = _sp;
					_cache.Add(at.tiles[i].id, at.tiles[i]);
				}
			}
		}

		GameMapTile tile = null;
		if (_cache.TryGetValue(id, out tile)) return tile;
		return null;
	}

	// ----------------------------------------------------------------------------------------------------------------

	/// <summary> Add tile definition. Mainly for editor use. </summary>
	public void AddTile()
	{
		GameMapTile t = new GameMapTile() { id = nextTileId++ };

		tiles.Add(t);
		_cache = null;
	}

	/// <summary> Add auto-tile definition. Mainly for editor use. </summary>
	public void AddAutoTile(bool withCorners)
	{
		AutoTile at = new AutoTile() { id = nextAutoTileId++, tiles = new GameMapTile[withCorners ? 47 : 16] };
		for (int i = 0; i < at.tiles.Length; i++)
		{
			at.tiles[i] = new GameMapTile() { id = nextTileId++, _aid = at.id };
		}

		autoTiles.Add(at);
		_cache = null;
	}

	/// <summary> Remove tile definition. Mainly for editor use. </summary>
	public void RemoveTileAtIndex(int idx)
	{
		tiles.RemoveAt(idx);
		if (tiles.Count == 0 && autoTiles.Count == 0) nextTileId = 1;
		_cache = null;
	}

	/// <summary> Remove auto-tile definition. Mainly for editor use. </summary>
	public void RemoveAutoTileAtIndex(int idx)
	{
		autoTiles.RemoveAt(idx);
		if (tiles.Count == 0 && autoTiles.Count == 0) nextTileId = 1;
		if (autoTiles.Count == 0) nextAutoTileId = 1;
		_cache = null;
	}

	// ----------------------------------------------------------------------------------------------------------------
}
