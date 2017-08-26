using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GameMapAutoTileEd : EditorWindow
{
	private GameMapTilesAsset asset;
	private GameMapTilesAsset.AutoTile autoTile;
	private System.Action<int> onChange;
	private bool is16Tile;

	private static readonly GUIContent GC_Close = new GUIContent("Close");
	private static readonly GUIContent GC_Info = new GUIContent("Add sprites in spaces below");

	public class StyleDefs
	{
		public GUIStyle WhiteBackground;

		public StyleDefs()
		{
			WhiteBackground = new GUIStyle() { normal = { background = EditorGUIUtility.whiteTexture } };
		}
	}

	private static StyleDefs _styles;
	private static StyleDefs Styles { get { return _styles ?? (_styles = new StyleDefs()); } }

	int[,] map46 =
	{ //  T   R   B   L
		{ 1,0,1,0,1,0,1,0 }, // 0*
		{ 0,0,1,0,1,0,1,0 }, // 1*
		{ 1,0,0,0,1,0,1,0 }, // 4
		{ 1,0,1,0,0,0,1,0 }, // 16
		{ 1,0,1,0,1,0,0,0 }, // 64
		{ 0,1,0,0,1,0,1,0 }, // 5*
		{ 1,0,0,1,0,0,1,0 }, // 20
		{ 1,0,1,0,0,1,0,0 }, // 80
		{ 0,0,1,0,1,0,0,1 }, // 65
		{ 0,0,0,0,1,0,1,0 }, // 7*
		{ 1,0,0,0,0,0,1,0 }, // 28
		{ 1,0,1,0,0,0,0,0 }, // 112
		{ 0,0,1,0,1,0,0,0 }, // 193
		{ 0,0,1,0,0,0,1,0 }, // 17*
		{ 1,0,0,0,1,0,0,0 }, // 68
		{ 0,1,0,1,0,0,1,0 }, // 21*
		{ 1,0,0,1,0,1,0,0 }, // 84
		{ 0,0,1,0,0,1,0,1 }, // 81
		{ 0,1,0,0,1,0,0,1 }, // 69
		{ 0,0,0,1,0,0,1,0 }, // 23*
		{ 1,0,0,0,0,1,0,0 }, // 92
		{ 0,0,1,0,0,0,0,1 }, // 113
		{ 0,1,0,0,1,0,0,0 }, // 197
		{ 0,1,0,0,0,0,1,0 }, // 29*
		{ 1,0,0,1,0,0,0,0 }, // 116
		{ 0,0,1,0,0,1,0,0 }, // 209
		{ 0,0,0,0,1,0,0,1 }, // 71
		{ 0,0,0,0,0,0,1,0 }, // 31*
		{ 1,0,0,0,0,0,0,0 }, // 124
		{ 0,0,1,0,0,0,0,0 }, // 214
		{ 0,0,0,0,1,0,0,0 }, // 199
		{ 0,1,0,1,0,1,0,1 }, // 85*
		{ 0,0,0,1,0,1,0,1 }, // 87*
		{ 0,1,0,0,0,1,0,1 }, // 93
		{ 0,1,0,1,0,0,0,1 }, // 117
		{ 0,1,0,1,0,1,0,0 }, // 213
		{ 0,0,0,0,0,1,0,1 }, // 95*
		{ 0,1,0,0,0,0,0,1 }, // 125
		{ 0,1,0,1,0,0,0,0 }, // 245
		{ 0,0,0,1,0,1,0,0 }, // 215
		{ 0,0,0,1,0,0,0,1 }, // 119*
		{ 0,1,0,0,0,1,0,0 }, // 221
		{ 0,0,0,0,0,0,0,1 }, // 127*
		{ 0,1,0,0,0,0,0,0 }, // 253
		{ 0,0,0,1,0,0,0,0 }, // 247
		{ 0,0,0,0,0,1,0,0 }, // 223
		{ 0,0,0,0,0,0,0,0 }, // 255*
	};

	// ----------------------------------------------------------------------------------------------------------------

	public static void Show_GameMapAutoTileEd(GameMapTilesAsset asset, GameMapTilesAsset.AutoTile autoTile, System.Action<int> onChange)
	{
		GameMapAutoTileEd win = GetWindow<GameMapAutoTileEd>(true, "AutoTile Ed", true);
		win.asset = asset;
		win.autoTile = autoTile;
		win.is16Tile = autoTile.tiles.Length == 16;
		win.onChange = onChange;
		win.minSize = win.maxSize = new Vector2(win.is16Tile ? 175f : 495f, 220f);
		win.ShowUtility();
	}

	private void OnFocus()
	{
		wantsMouseMove = true;
	}

	private void OnLostFocus()
	{
		wantsMouseMove = false;
	}

	private void OnGUI()
	{
		if (asset == null || autoTile == null)
		{
			Close();
			GUIUtility.ExitGUI();
			return;
		}

		if (GUILayout.Button(GC_Close))
		{
			Close();
		}

		GUILayout.Label(GC_Info);
		EditorGUILayout.Space();
		Rect rect = GUILayoutUtility.GetRect(1f, 1f, 1f, 1f);
		Rect r = new Rect(rect.x + 10, rect.y + 10, 35, 35);

		if (Event.current.type == EventType.Repaint)
		{
			GUI.color = Color.white;
			Styles.WhiteBackground.Draw(new Rect(rect.x, rect.y, position.width, position.height), false, false, false, false);
		}

		for (int i = 0; i < autoTile.tiles.Length; i++)
		{
			EditorGUI.BeginChangeCheck();
			autoTile.tiles[i].sprite = (Sprite)EditorGUI.ObjectField(r, autoTile.tiles[i].sprite, typeof(Sprite), false);
			if (EditorGUI.EndChangeCheck() && onChange != null) onChange(i);

			if (Event.current.type == EventType.Repaint)
			{
				if (autoTile.tiles[i].sprite != null)
				{
					GUI.color = Color.black;
					Styles.WhiteBackground.Draw(r, false, false, false, false);
					GUI.color = Color.white;
					Sprite sp = autoTile.tiles[i].sprite;
					Rect r2= new Rect(sp.rect.x / sp.texture.width, sp.rect.y / sp.texture.height, sp.rect.width / sp.texture.width, sp.rect.height / sp.texture.height);
					GUI.DrawTextureWithTexCoords(r, sp.texture, r2);
				}

				if (autoTile.tiles[i].sprite == null || r.Contains(Event.current.mousePosition))
				{
					if (is16Tile) Draw16TileSample(r, i);
					else Draw46TileSample(r, i);
				}
			}

			r.x += 40;
			if ((is16Tile && r.x > 150) || r.x > 480) { r.x = rect.x + 10; r.y += 40; }
		}

		if (Event.current.type == EventType.MouseMove)
		{
			Repaint();
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(asset);
			GUI.changed = false;
			Repaint();
		}
	}

	// I did not want to make project messy with sample textures so will just manually draw them with these functions

	private void Draw16TileSample(Rect r, int idx)
	{
		switch (idx)
		{
			case 0: DrawSample(r, true, true, true, true); break;
			case 1: DrawSample(r, false, true, true, true); break;
			case 2: DrawSample(r, true, true, false, true); break;
			case 3: DrawSample(r, false, true, false, true); break;
			case 4: DrawSample(r, true, true, true, false); break;
			case 5: DrawSample(r, false, true, true, false); break;
			case 6: DrawSample(r, true, true, false, false); break;
			case 7: DrawSample(r, false, true, false, false); break;
			case 8: DrawSample(r, true, false, true, true); break;
			case 9: DrawSample(r, false, false, true, true); break;
			case 10: DrawSample(r, true, false, false, true); break;
			case 11: DrawSample(r, false, false, false, true); break;
			case 12: DrawSample(r, true, false, true, false); break;
			case 13: DrawSample(r, false, false, true, false); break;
			case 14: DrawSample(r, true, false, false, false); break;
			case 15: DrawSample(r, false, false, false, false); break;
		}
	}

	private void Draw46TileSample(Rect r, int idx)
	{
		DrawSample(r, map46[idx, 0], map46[idx, 1], map46[idx, 2], map46[idx, 3], map46[idx, 4], map46[idx, 5], map46[idx, 6], map46[idx, 7]);
	}

	private void DrawSample(Rect rect, int t, int tr, int r, int br, int b, int bl, int l, int tl)
	{
		GUI.color = Color.black;
		Styles.WhiteBackground.Draw(rect, false, false, false, false);
		GUI.color = Color.green;
		if (t == 1) DrawTopLine(rect);
		if (b == 1) DrawBottomLine(rect);
		if (l == 1) DrawLeftLine(rect);
		if (r == 1) DrawRightLine(rect);
		if (tl == 1) DrawTopLeftCorner(rect);
		if (tr == 1) DrawTopRightCorner(rect);
		if (bl == 1) DrawBottomLeftCorner(rect);
		if (br == 1) DrawBottomRightCorner(rect);
		GUI.color = Color.white;
	}

	private void DrawSample(Rect rect, bool t, bool b, bool l, bool r)
	{
		GUI.color = Color.black;
		Styles.WhiteBackground.Draw(rect, false, false, false, false);
		GUI.color = Color.green;
		if (t) DrawTopLine(rect);
		if (b) DrawBottomLine(rect);
		if (l) DrawLeftLine(rect);
		if (r) DrawRightLine(rect);
		GUI.color = Color.white;
	}

	private void DrawTopLine(Rect r)
	{
		r.y += 2f; r.height = 2f;
		r.x += 2f; r.width -= 4f;
		Styles.WhiteBackground.Draw(r, false, false, false, false);
	}

	private void DrawBottomLine(Rect r)
	{
		r.y = r.yMax - 4f; r.height = 2f;
		r.x += 2f; r.width -= 4f;
		Styles.WhiteBackground.Draw(r, false, false, false, false);
	}

	private void DrawLeftLine(Rect r)
	{
		r.y += 2f; r.height -= 4f;
		r.x += 2f; r.width = 2f;
		Styles.WhiteBackground.Draw(r, false, false, false, false);
	}

	private void DrawRightLine(Rect r)
	{
		r.y += 2f; r.height -= 4f;
		r.x = r.xMax - 4f; r.width = 2f;
		Styles.WhiteBackground.Draw(r, false, false, false, false);
	}

	private void DrawTopLeftCorner(Rect r)
	{
		DrawTopLine(new Rect(r.x, r.y + 5f, 10f, r.height));
		DrawLeftLine(new Rect(r.x + 5f, r.y, r.width, 10f));
	}

	private void DrawTopRightCorner(Rect r)
	{
		DrawTopLine(new Rect(r.xMax - 10f, r.y + 5f, 10f, r.height));
		DrawRightLine(new Rect(r.x - 5f, r.y, r.width, 10f));
	}

	private void DrawBottomRightCorner(Rect r)
	{
		DrawBottomLine(new Rect(r.xMax - 10f, r.y - 5f, 10f, r.height));
		DrawRightLine(new Rect(r.x - 5, r.yMax - 10f, r.width, 10f));
	}

	private void DrawBottomLeftCorner(Rect r)
	{
		DrawBottomLine(new Rect(r.x, r.y - 5f, 10f, r.height));
		DrawLeftLine(new Rect(r.x + 5f, r.yMax - 10f, r.width, 10f));
	}

	// ----------------------------------------------------------------------------------------------------------------
}
