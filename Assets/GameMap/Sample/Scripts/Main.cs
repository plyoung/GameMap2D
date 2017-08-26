using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Main : MonoBehaviour
{
	// ----------------------------------------------------------------------------------------------------------------
	#region properties

	[SerializeField] public float tileSize = 32f;
	[SerializeField] public float ppu = 100f;

	[Space]
	[SerializeField] public GameMapsAsset mapsAsset;
	[SerializeField] public GameObject playerFab;
	[SerializeField] public GameObject floorFab;
	[SerializeField] public GameObject[] npcFabs;
	[SerializeField] public GameObject[] trapFabs;

	#endregion
	// ----------------------------------------------------------------------------------------------------------------
	#region system

	private void Start()
	{
		LoadMap(0);
	}

	// an example of how map data could be loaded
	private void LoadMap(int mapIdx)
	{
		GameMap map = mapsAsset.maps[mapIdx];

		float sz = tileSize / ppu;
		float offsX = -((map.width / 2f * sz) - (sz / 2f));
		float offsY = -((map.height / 2f * sz) - (sz / 2f));

		// create containers for the various map objects
		Transform floorContainer = new GameObject("Tiles").transform;
		Transform npcContainer = new GameObject("NPCs").transform;
		Transform trapContainer = new GameObject("Traps").transform;

		// place tiles and objects. GameMap supports laters but if you choose not to use them then you only need to read from map.grid[]
		// if you do use layers then you should also read data from map.layers[].grid[] while also still using map.grid[] as layer-0
		// to make it easier to read from these two sources of data you can simply use map.LayerCount and map.GetLayerData

		for (int i = 0; i < map.LayerCount; i++)
		{
			int[] grid = map.GetlayerData(i);
			int idx = 0;
			for (int y = 0; y < map.height; y++)
			{
				for (int x = 0; x < map.width; x++)
				{
					GameMapTile t = mapsAsset.tileAsset.GetTile(grid[idx++]);
					if (t == null) continue;

					if (t.type == GameMapTile.Type.Floor)
					{   // place a floor tile
						if (t.sprite == null) continue;
						SpriteRenderer ren = Instantiate(floorFab).GetComponent<SpriteRenderer>();
						ren.sprite = t.sprite;
						ren.transform.SetParent(floorContainer, false);
						ren.transform.localScale = new Vector3(tileSize / ren.sprite.rect.width, tileSize / ren.sprite.rect.height, 1f);
						ren.transform.localPosition = new Vector3(x * sz + offsX, y * sz + offsY, 0f);
						ren.GetComponent<BoxCollider2D>().size = new Vector2(ren.sprite.rect.width / ren.sprite.pixelsPerUnit, ren.sprite.rect.height / ren.sprite.pixelsPerUnit);
					}

					else if (t.type == GameMapTile.Type.NPC)
					{   // place an NPC
						SpriteRenderer ren = Instantiate(npcFabs[t.opt1]).GetComponent<SpriteRenderer>();
						ren.transform.SetParent(npcContainer, false);
						ren.transform.localScale = new Vector3(tileSize / ren.sprite.rect.width, tileSize / ren.sprite.rect.height, 1f);
						ren.transform.localPosition = new Vector3(x * sz + offsX, y * sz + offsY, 0f);
					}

					else if (t.type == GameMapTile.Type.Trap)
					{   // place a Trap
						SpriteRenderer ren = Instantiate(trapFabs[t.opt1]).GetComponent<SpriteRenderer>();
						ren.transform.SetParent(trapContainer, false);
						ren.transform.localScale = new Vector3(tileSize / ren.sprite.rect.width, tileSize / ren.sprite.rect.height, 1f);
						ren.transform.localPosition = new Vector3(x * sz + offsX, y * sz + offsY, 0f);
					}

					else if (t.type == GameMapTile.Type.Start)
					{   // place player object
						SpriteRenderer ren = Instantiate(playerFab).GetComponent<SpriteRenderer>();
						ren.transform.localScale = new Vector3(tileSize / ren.sprite.rect.width, tileSize / ren.sprite.rect.height, 1f);
						ren.transform.localPosition = new Vector3(x * sz + offsX, y * sz + offsY, 0f);
					}

				}
			}
		}
	}

	#endregion
	// ----------------------------------------------------------------------------------------------------------------
}
