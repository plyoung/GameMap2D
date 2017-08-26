using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A layer consists of a series of values in a grid.
/// Each GameMap can have one or more layers.
/// </summary>
[System.Serializable]
public class GameMapLayer
{
	/// <summary> The layer's grid of tile values. -1 is an empty tile, else a value related to GameMapTile.id will be present </summary>
	public int[] grid = new int[0];


	// ----------------------------------------------------------------------------------------------------------------
}
