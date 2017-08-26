using System.Collections.Generic;
using UnityEngine;


/// <summary> Container for all defined maps. It also holds a reference the defined tiles. </summary>
[System.Serializable]
public class GameMapsAsset : ScriptableObject
{
	/// <summary> All maps created via MapEditor </summary>	
	[SerializeField] public List<GameMap> maps = new List<GameMap>();

	/// <summary> The tiles asset associated with the maps </summary>	
	[SerializeField] public GameMapTilesAsset tileAsset;

	[SerializeField] private int nextMapId = 1;

	// *** extra properties related to this maps asset
	// add any addition serializable types here and they will appear
	// for editing in asset properties section of the map editor

	// public string someProperty; // example

	// ----------------------------------------------------------------------------------------------------------------

	/// <summary> Add a new map. Mainly for use by the map editor. </summary>
	public void AddMap()
	{
		GameMap m = new GameMap() { id = nextMapId, ident = "Map " + nextMapId };
		m.SetSize(10, 10);
		nextMapId++;
		maps.Add(m);
	}

	/// <summary> Remove a map. Mainly for use by the map editor. </summary>
	public void RemoveMapAtIndex(int idx)
	{
		maps.RemoveAt(idx);
		if (maps.Count == 0) nextMapId = 1;
	}

	// ----------------------------------------------------------------------------------------------------------------
}
