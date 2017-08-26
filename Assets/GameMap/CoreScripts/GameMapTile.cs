using UnityEngine;


/// <summary> A tile definition. The map/grid consist of an array of these tiles. </summary>
[System.Serializable]
public class GameMapTile
{
	// the tile needs a way to be identifiable in the editor. this can be done with either a sprite, colour, or value.
	// simply add the [GameMapTilePreview] attribute above any Sprite, Color, String, or Int properties below.
	// for String an empty value will be ignored. For Int negative numbers will be ignored.

	/// <summary> Unique identifier for tile. This is the value stored in GameMap.grid[] (-1 is used for empty tiles in the grid) </summary>
	public int id;

	/// <summary> Sprite representing the tile. </summary>
	[GameMapTilePreview] public Sprite sprite; // You may choose not to use this but do not remove it since it is required by auto-tiles system.

	public int _aid = -1; // helper for auto-tiles; do not remove.

	// *** extra properties related to this tile definition
	// add any addition serializable types here and they will appear
	// for editing in asset properties section of the map editor

	// Note: 
	//	Any of the fields following may be removed if you do not need/use them. They are here as an example. 
	//	(of course the demo script makes use of them so update it too)
	//	Remember to add/remove fields to CopyTo() function too

	public enum Type { Start=0, End=1, Key=2, Coin=3, Block=4, Floor=20, Platform=21, NPC=30, Trap=40, Text=50 }
	public Type type = Type.Floor;	// the runtime might use something like this to identify what the placed tile means
	public int opt1 = 0;			// this value could depend on the chosen type. For example, if NPC then this could indicate which NPC prefab to spawn from an array of NPC prefabs.
	
	[GameMapTilePreview] public Color color = Color.white;
	[GameMapTilePreview] public string strVal = "";

	// ----------------------------------------------------------------------------------------------------------------

	/// <summary> Copies this tile's data into target. </summary>
	public void CopyTo(GameMapTile t)
	{
		t.id = id;
		t.sprite = sprite;
		t.type = type;
		t.opt1 = opt1;
		t.color = color;
		t.strVal = strVal;
	}

	// ----------------------------------------------------------------------------------------------------------------
}
