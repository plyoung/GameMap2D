using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameMapEditor : EditorWindow
{
	// ----------------------------------------------------------------------------------------------------------------
	#region vars

	private static Color backColor = new Color(0f, 0f, 0f, 1f);
	private static Color gridColor = new Color(1f, 1f, 1f, 0.3f);
	private static bool outsideMapIsSolid = true; // if true then the areas outside map counts as solid of an auto-tile when doing auto-tile calculations
	private static float refTileSz = 0f;
	private static float panelWidth = 250f;
	private static bool showEditorSettings = true;
	private static bool showAssetProperties = true;
	private static bool showMapProperties = true;
	private static bool showTileProperties = true;
    private static bool showLayers = true;
    private static GameMapEdPopup mapsPopup = new GameMapEdPopup();
	private static Vector2[] scroll = { Vector2.zero, Vector2.zero };

	private bool dragSplitter = false;
	private bool doRepaint = false;

	private GameMapsAsset asset = null;
	private GameMapsAsset _setAsset = null;
	private int mapSize_w = 0;
	private int mapSize_h = 0;
	private int mapIdx = -1;
	private int tileIdx = -1;
	private bool autoTileSelected = false;
	private float tileDrawSz = 32f;
	private int dragDropTarget = -1;
    private int currLayer = -1;
    private bool[] layerHidden = new bool[0];

	[NonSerialized] private float tileListH = 1f;
	[NonSerialized] private bool marking = false;
	[NonSerialized] private bool pasting = false;
	[NonSerialized] private bool clearMarked = false;

	[NonSerialized] private SerializedObject assetObj;
	[NonSerialized] private Editor assetEd = null;
	[NonSerialized] private Editor tilesEd = null;
	[NonSerialized] private Dictionary<int, TileDef> tileCache = new Dictionary<int, TileDef>();
	[NonSerialized] private GUIContent TileContent = new GUIContent();
	[NonSerialized] private List<int> markedTiles = new List<int>();
	[NonSerialized] private List<TileDef> copyBuffer = new List<TileDef>();

	[NonSerialized] private FieldInfo colField = null;
	[NonSerialized] private FieldInfo sprField = null;
	[NonSerialized] private FieldInfo intField = null;
	[NonSerialized] private FieldInfo strField = null;
	[NonSerialized] private FieldInfo flpField = null;

	private static int EditorCanvasHash = "GameMapEditorCanvasHash".GetHashCode();

	#endregion
	// ----------------------------------------------------------------------------------------------------------------
	#region defs

	private class TileDef
	{
		public string text;
		public Sprite sprite;
		public Rect rect;
		public Color color = Color.white;
		public bool flipped;
		public bool isAuto;

		// these are used by copy/paste system
		public int id;
		public int x, y;

		public TileDef Copy()
		{
			return new TileDef()
			{
				text = text,
				sprite = sprite,
				rect = rect,
				color = color,
				flipped = flipped,
				isAuto = isAuto,
				id = id,
				x = x,
				y = y
			};
		}
	}

	private class StyleDefs
	{
		public GUIStyle Panel;
		public GUIStyle Tile;
		public GUIStyle SolidWhite;

		public StyleDefs()
		{
			GUISkin skin = GUI.skin;
			Panel = new GUIStyle(skin.FindStyle("PreferencesSectionBox")) { padding = new RectOffset(0, 0, 0, 3), margin = new RectOffset(0, 0, 0, 0), stretchHeight = true, stretchWidth = false };
			Tile = new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 12, fontStyle = FontStyle.Bold, normal = { textColor = Color.black, background = null } };
			SolidWhite = new GUIStyle() { normal = { background = EditorGUIUtility.whiteTexture } };
		}
	}

	private StyleDefs _styles = null;
	private StyleDefs Styles { get { return _styles ?? (_styles = new StyleDefs()); } }

	private static readonly GUIContent GC_MapSelect = new GUIContent("-select-");
	private static readonly GUIContent GC_EditorHead = new GUIContent("Editor");
	private static readonly GUIContent GC_AssetHead = new GUIContent("Asset");
	private static readonly GUIContent GC_MapHead = new GUIContent("Map");
    private static readonly GUIContent GC_LayersHead = new GUIContent("Layers");
    private static readonly GUIContent GC_TilesHead = new GUIContent("Tiles");
	private static readonly GUIContent GC_new = new GUIContent("new");
	private static readonly GUIContent GC_rename = new GUIContent("ren", "Rename");
	private static readonly GUIContent GC_apply = new GUIContent("apply");
	private static readonly GUIContent GC_clear = new GUIContent("x", "Clear tile selection. Draw empty tiles.");
	private static readonly GUIContent GC_add = new GUIContent("+", "Add");
	private static readonly GUIContent GC_rem = new GUIContent("-", "Remove selected");
	private static readonly GUIContent GC_movL = new GUIContent("<", "Move selected");
	private static readonly GUIContent GC_movR = new GUIContent(">", "Move selected");
	private static readonly GUIContent GC_BackCol = new GUIContent("BackColor");
	private static readonly GUIContent GC_GridCol = new GUIContent("GridColor");
	private static readonly GUIContent GC_Solid = new GUIContent("Outside is Solid");
	private static readonly GUIContent GC_AutoSz = new GUIContent("Auto-tile Size");
	private static readonly GUIContent GC_EditAuto = new GUIContent("Setup Auto-tile");
    private static readonly GUIContent GC_Viz = new GUIContent("*", "Toggle layer visblity in editor");

    private GenericMenu addTileMenu = null;

	private static readonly Dictionary<int, int> map64 = new Dictionary<int, int>()
	{
		{ 0, 0 },
		{ 1, 1 }, { 4, 2 }, { 16, 3 }, { 64, 4 },
		{ 5, 5 }, { 20, 6 }, { 80, 7 }, {65, 8 },
		{ 7, 9 }, { 28, 10 }, { 112, 11 }, { 193, 12 },
		{ 17, 13 }, { 68, 14 },
		{ 21, 15 }, { 84, 16 }, { 81, 17 }, { 69, 18 },
		{ 23, 19 }, { 92, 20 }, { 113, 21 }, { 197, 22 },
		{ 29, 23 }, { 116, 24 }, { 209, 25 }, { 71, 26 },
		{ 31, 27 }, { 124, 28 }, { 241, 29 }, { 199, 30 },
		{ 85, 31 },
		{ 87, 32 }, { 93, 33 }, { 117, 34 }, { 213, 35 },
		{ 95, 36 }, { 125, 37 }, { 245, 38 }, { 215, 39 },
		{ 119, 40 }, { 221, 41 },
		{ 127, 42 }, { 253, 43 }, { 247, 44 }, { 223, 45 },
		{ 255, 46 }
	};

	#endregion
	// ----------------------------------------------------------------------------------------------------------------
	#region system

	[MenuItem("Window/GameMap Editor")]
	public static void Open_GameMapEditor()
	{
		GetWindow<GameMapEditor>("GameMap", true);
	}

	public static void Open_GameMapEditor(GameMapsAsset openAsset)
	{
		GameMapEditor win = GetWindow<GameMapEditor>("GameMap", true);
		win._setAsset = openAsset;
	}

	private void OnEnable()
	{
		backColor = EditorPrefs_GetColor("plyGameMapEd.backColor", backColor);
		gridColor = EditorPrefs_GetColor("plyGameMapEd.gridColor", gridColor);
		outsideMapIsSolid = EditorPrefs.GetBool("plyGameMapEd.outsideMapIsSolid", outsideMapIsSolid);
		refTileSz = EditorPrefs.GetFloat("plyGameMapEd.refTileSz", refTileSz);
		panelWidth = EditorPrefs.GetFloat("plyGameMapEd.panelWidth", panelWidth);
		showEditorSettings = EditorPrefs.GetBool("plyGameMapEd.showEditorSettings", showEditorSettings);
		showAssetProperties = EditorPrefs.GetBool("plyGameMapEd.showAssetProps", showAssetProperties);
		showMapProperties = EditorPrefs.GetBool("plyGameMapEd.showMapProps", showMapProperties);
		showTileProperties = EditorPrefs.GetBool("plyGameMapEd.showTileProps", showTileProperties);
        showLayers = EditorPrefs.GetBool("plyGameMapEd.showLayers", showLayers);

        // auto load 1st found asset
        if (_setAsset == null)
		{
			GameMapsAsset[] assets = Resources.FindObjectsOfTypeAll<GameMapsAsset>();
			if (assets.Length > 0) _setAsset = assets[0];
		}

		Undo.undoRedoPerformed -= OnUnRedo;
		Undo.undoRedoPerformed += OnUnRedo;
	}

	private void OnDisable()
	{
		Undo.undoRedoPerformed -= OnUnRedo;
	}

	private void OnFocus()
	{
		wantsMouseMove = true;
		dragSplitter = false;
		marking = false;
		pasting = false;
		clearMarked = false;
	}

	private void OnLostFocus()
	{
		wantsMouseMove = false;
		marking = false;
		pasting = false;
		clearMarked = false;
	}

	private void OnGUI()
	{
		if (asset == null && assetObj != null)
		{   // catch case where asset was manually deleted while active in editor
			assetEd = null;
			tilesEd = null;
			assetObj = null;
			tileCache.Clear();
			GUIUtility.ExitGUI();
			return;
		}

		Event ev = Event.current;
		EditorGUILayout.BeginHorizontal();
		{
			DrawSideBar(ev);
			DrawCanvas(ev);
		}
		EditorGUILayout.EndHorizontal();

		if (doRepaint)
		{
			doRepaint = false;
			Repaint();
		}
	}

	private void OnUnRedo()
	{
		UpdateTileDefCache();
		Repaint();
	}

	#endregion
	// ----------------------------------------------------------------------------------------------------------------
	#region side panel

	private void DrawSideBar(Event ev)
	{
		Rect r = EditorGUILayout.BeginVertical(Styles.Panel, GUILayout.Width(panelWidth));
		scroll[0] = EditorGUILayout.BeginScrollView(scroll[0], GUIStyle.none, GUI.skin.verticalScrollbar);
		{
			EditorGUIUtility.labelWidth = 100;

			if (assetObj != null) assetObj.Update();
			DrawEditorSettings();
			DrawMapAssetProperties();
			DrawMapProperties();
            DrawLayers();
            if (assetObj != null) assetObj.ApplyModifiedProperties();

			DrawTileProperties(ev);
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();

		// splitter
		r.x = r.xMax; r.width = 5;

		if (ev.type == EventType.Repaint)
		{
			GUI.skin.box.Draw(r, false, false, false, false);
		}

		EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeHorizontal);
		if (ev.type == EventType.MouseDown && ev.button == 0 && r.Contains(ev.mousePosition))
		{
			ev.Use();
			dragSplitter = true;
		}

		if (dragSplitter && ev.button == 0)
		{
			if (ev.type == EventType.MouseUp)
			{
				ev.Use();
				dragSplitter = false;
				doRepaint = true;
				EditorPrefs.SetFloat("plyGameMapEd.panelWidth", panelWidth);
			}

			if (ev.type == EventType.MouseDrag)
			{
				ev.Use();
				doRepaint = true;
				panelWidth += ev.delta.x;
				float f = position.width / 2f;
				panelWidth = Mathf.Clamp(panelWidth, 150, f < 150 ? 150 : f);
				EditorPrefs.SetFloat("plyGameMapEd.panelWidth", panelWidth);
			}
		}

		if (ev.type == EventType.Repaint)
		{
			if (asset != _setAsset)
			{
				asset = _setAsset;
				assetObj = null;
				mapIdx = -1;
				doRepaint = true;
			}

			if (asset != null)
			{
				if (assetEd == null || assetEd.target != asset)
				{
					if (assetEd != null) DestroyImmediate(assetEd);
					assetEd = Editor.CreateEditor(asset);
					doRepaint = true;
				}

				if (asset.tileAsset != null)
				{
					if (tilesEd == null || tilesEd.target != asset.tileAsset)
					{
						if (tilesEd != null) DestroyImmediate(tilesEd);
						tilesEd = Editor.CreateEditor(asset.tileAsset);
						CollectTilePreviewFields();
						UpdateTileDefCache();
						doRepaint = true;
					}
				}
				else if (tilesEd != null)
				{
					DestroyImmediate(tilesEd);
					tilesEd = null;
					tileCache.Clear();
					doRepaint = true;
				}
			}
			else
			{
				if (assetEd != null)
				{
					DestroyImmediate(assetEd);
					assetEd = null;
					assetObj = null;
					doRepaint = true;
				}

				if (tilesEd != null)
				{
					DestroyImmediate(tilesEd);
					tilesEd = null;
					tileCache.Clear();
					doRepaint = true;
				}
			}

			if (assetEd != null && assetObj == null)
			{
				assetObj = assetEd.serializedObject;
				doRepaint = true;
			}

			if (asset != null && asset.maps.Count > 0 && mapIdx < 0)
			{   // auto select 1st map
				OnMapSelected(0);
			}
		}

	}

	private void CollectTilePreviewFields()
	{
		colField = null;
		sprField = null;
		intField = null;
		strField = null;
		flpField = null;

		System.Type t = asset.tileAsset.tiles.GetType().GetGenericArguments()[0];
		System.Type attribT = typeof(GameMapTilePreviewAttribute);
		FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo f in fields)
		{
			object[] attribs = f.GetCustomAttributes(attribT, false);
			if (attribs.Length > 0)
			{
				if (colField == null && f.FieldType == typeof(Color)) { colField = f; continue; }
				if (sprField == null && f.FieldType == typeof(Sprite)) { sprField = f; continue; }
				if (intField == null && f.FieldType == typeof(int)) { intField = f; continue; }
				if (strField == null && f.FieldType == typeof(string)) { strField = f; continue; }
				if (flpField == null && f.FieldType == typeof(bool)) { flpField = f; continue; }
			}
		}
	}

	private void UpdateTileDefCache()
	{
		if (asset == null || asset.tileAsset == null) return;

		tileCache.Clear();

		foreach (GameMapTilesAsset.AutoTile at in asset.tileAsset.autoTiles)
		{
			foreach (GameMapTile tile in at.tiles)
			{
				TileDef def = new TileDef() { id = tile.id };
				UpdateCachedValues(def, tile, true);
				tileCache.Add(tile.id, def);
			}
		}

		foreach (GameMapTile tile in asset.tileAsset.tiles)
		{
			TileDef def = new TileDef() { id = tile.id };
			UpdateCachedValues(def, tile, false);
			tileCache.Add(tile.id, def);
		}
	}

	private void UpdateCachedValues(TileDef def, GameMapTile tile, bool isAuto)
	{
		def.color = Color.white;
		def.sprite = null;
		def.text = null;
		def.flipped = false;
		def.isAuto = isAuto;

		if (colField != null)
		{
			def.color = (Color)colField.GetValue(tile);
		}

		if (sprField != null)
		{
			Sprite sp = (Sprite)sprField.GetValue(tile);
			if (sp != null)
			{
				def.sprite = sp;
				def.rect = new Rect(sp.rect.x / sp.texture.width, sp.rect.y / sp.texture.height, sp.rect.width / sp.texture.width, sp.rect.height / sp.texture.height);
			}
		}

		if (strField != null)
		{
			string s = (string)strField.GetValue(tile);
			if (!string.IsNullOrEmpty(s)) def.text = s;
		}

		if (intField != null && def.text == null)
		{
			int a = (int)intField.GetValue(tile);
			if (a >= 0) def.text = a.ToString();
		}

		if (flpField != null)
		{
			def.flipped = (bool)flpField.GetValue(tile);
		}
	}

	private void DrawEditorSettings()
	{
		EditorGUILayout.Space();
		EditorGUI.BeginChangeCheck();
		showEditorSettings = EditorGUILayout.Foldout(showEditorSettings, GC_EditorHead, true);
		if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool("plyGameMapEd.showEditorSettings", showEditorSettings);
		if (showEditorSettings)
		{
			EditorGUI.BeginChangeCheck();
			backColor = EditorGUILayout.ColorField(GC_BackCol, backColor);
			if (EditorGUI.EndChangeCheck()) EditorPrefs_SetColor("plyGameMapEd.backColor", backColor);

			EditorGUI.BeginChangeCheck();
			gridColor = EditorGUILayout.ColorField(GC_GridCol, gridColor);
			if (EditorGUI.EndChangeCheck()) EditorPrefs_SetColor("plyGameMapEd.gridColor", gridColor);

			EditorGUI.BeginChangeCheck();
			outsideMapIsSolid = EditorGUILayout.Toggle(GC_Solid, outsideMapIsSolid);
			if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool("plyGameMapEd.outsideMapIsSolid", outsideMapIsSolid);

			EditorGUI.BeginChangeCheck();
			refTileSz = EditorGUILayout.FloatField(GC_AutoSz, refTileSz);
			if (EditorGUI.EndChangeCheck()) EditorPrefs.SetFloat("plyGameMapEd.refTileSz", refTileSz);

			EditorGUILayout.Space();
		}
	}

	private void DrawMapAssetProperties()
	{
		GUILayout.Box(GUIContent.none, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(3f));
		EditorGUI.BeginChangeCheck();
		showAssetProperties = EditorGUILayout.Foldout(showAssetProperties, GC_AssetHead, true);
		if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool("plyGameMapEd.showAssetProps", showAssetProperties);
		if (showAssetProperties)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUI.BeginChangeCheck();
				_setAsset = (GameMapsAsset)EditorGUILayout.ObjectField(_setAsset, typeof(GameMapsAsset), false, GUILayout.ExpandWidth(true));
				if (EditorGUI.EndChangeCheck()) doRepaint = true;
				if (GUILayout.Button(GC_new, EditorStyles.miniButtonRight, GUILayout.Width(50)))
				{
					string fn = EditorUtility.SaveFilePanel("Maps Asset", Application.dataPath, "maps", "asset");
					if (!string.IsNullOrEmpty(fn))
					{
						GameMapsAsset a = LoadOrCreateAsset<GameMapsAsset>(fn);
						if (a == null) EditorUtility.DisplayDialog("Error", "Could not create map asset", "OK");
						else _setAsset = a as GameMapsAsset;
						doRepaint = true;
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			if (assetObj != null)
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUI.BeginChangeCheck();
					//EditorGUILayout.PropertyField(assetObj.FindProperty("tileAsset"), GUIContent.none);
					GameMapTilesAsset tileAsset = (GameMapTilesAsset)EditorGUILayout.ObjectField(asset.tileAsset, typeof(GameMapTilesAsset), false, GUILayout.ExpandWidth(true));
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(asset, "Set tiles asset");
						asset.tileAsset = tileAsset;
						doRepaint = true;
					}
					if (GUILayout.Button(GC_new, EditorStyles.miniButtonRight, GUILayout.Width(50)))
					{
						string fn = EditorUtility.SaveFilePanel("Tile Asset", Application.dataPath, "tiles", "asset");
						if (!string.IsNullOrEmpty(fn))
						{
							tileAsset = LoadOrCreateAsset<GameMapTilesAsset>(fn);
							if (tileAsset == null) EditorUtility.DisplayDialog("Error", "Could not create tiles asset", "OK");
							else
							{
								Undo.RecordObject(asset, "Set tiles asset");
								asset.tileAsset = tileAsset;
							}
							doRepaint = true;
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();

				SerializedProperty iterator = assetObj.GetIterator();
				iterator.NextVisible(true); // skip the "m_Script" property
				bool enterChildren = true;
				bool hadElems = false;
				while (iterator.NextVisible(enterChildren))
				{
					enterChildren = false;
					if (iterator.name == "maps") continue;
					if (iterator.name == "tileAsset") continue;
					if (iterator.name == "nextMapId") continue;
					EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
					hadElems = true;
				}

				if (hadElems) EditorGUILayout.Space();
			}
		}
	}

	private void DrawMapProperties()
	{
		if (assetObj == null) return;
		GUILayout.Box(GUIContent.none, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(3f));
		Rect r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
		{
			// map selection button
			r.height = EditorGUIUtility.singleLineHeight;
			r.x = r.xMax - 103f; r.width = 100f; r.height = 15f;
			if (GUI.Button(r, GC_MapSelect))
			{
				mapsPopup.Asset = asset;
				mapsPopup.OnMapSelected = OnMapSelected;
				PopupWindow.Show(r, mapsPopup);
			}

			// the active map's properties
			EditorGUI.BeginChangeCheck();
			showMapProperties = EditorGUILayout.Foldout(showMapProperties, GC_MapHead, true);
			if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool("plyGameMapEd.showMapProps", showMapProperties);

			if (showMapProperties)
			{
				EditorGUILayout.Space();

				if (mapIdx >= 0 && mapIdx < asset.maps.Count)
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.PrefixLabel("Ident");
						GUILayout.Label(asset.maps[mapIdx].id.ToString() + " => " + asset.maps[mapIdx].ident);
						if (GUILayout.Button(GC_rename, EditorStyles.miniButtonRight)) GameMapTextEd.ShowEd("Rename map", "Enter a unique name", asset.maps[mapIdx].ident, OnRenameMap);
						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.PrefixLabel("Size");
						mapSize_w = EditorGUILayout.IntField(mapSize_w, GUILayout.Width(35));
						GUILayout.Label("x");
						mapSize_h = EditorGUILayout.IntField(mapSize_h, GUILayout.Width(35));
						if (GUILayout.Button(GC_apply, EditorStyles.miniButtonRight))
						{
							Undo.RecordObject(asset, "Resize map");
							asset.maps[mapIdx].Resize(mapSize_w, mapSize_h);
							mapSize_w = asset.maps[mapIdx].width;
							mapSize_h = asset.maps[mapIdx].height;
						}
						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();

					SerializedProperty maps = assetObj.FindProperty("maps");
					SerializedProperty iterator = maps.GetArrayElementAtIndex(mapIdx);
					SerializedProperty endprop = iterator.GetEndProperty();
					bool enterChildren = true;
					while (iterator.NextVisible(enterChildren))
					{
						enterChildren = false;
						if (SerializedProperty.EqualContents(iterator, endprop)) break;
						if (iterator.name == "id") continue;
						if (iterator.name == "ident") continue;
						if (iterator.name == "width") continue;
						if (iterator.name == "height") continue;
						if (iterator.name == "grid") continue;
                        if (iterator.name == "layers") continue;
                        EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
					}

					EditorGUILayout.Space();
				}
			}
		}
		EditorGUILayout.EndVertical();
	}

	private void OnMapSelected(int idx)
	{
		if (mapIdx == idx) return;

		mapIdx = idx;
		if (mapIdx >= asset.maps.Count) mapIdx = -1;

		if (mapIdx >= 0)
		{
			mapSize_w = asset.maps[mapIdx].width;
			mapSize_h = asset.maps[mapIdx].height;
		}

		pasting = false;
		marking = false;
		clearMarked = false;
		markedTiles.Clear();

		Repaint();
	}

	private void OnRenameMap(GameMapTextEd wiz)
	{
		string s = wiz.text;
		wiz.Close();

		if (!string.IsNullOrEmpty(s) && s != asset.maps[mapIdx].ident)
		{
			if (StringIsUnique(asset.maps, s))
			{
				Undo.RecordObject(asset, "Rename map");
				asset.maps[mapIdx].ident = s;
			}
			else
			{
				EditorUtility.DisplayDialog("Error", "The map name must be unique.", "OK");
			}
		}

		Repaint();
	}

    private void DrawLayers()
    {
        if (asset == null) return;
        GUILayout.Box(GUIContent.none, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(3f));
        Rect r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        {
            if (showLayers)
            {
                // buttons
                r.height = EditorGUIUtility.singleLineHeight;
                r.x = r.xMax - 28f; r.width = 25f; r.height = 15f;

                GUI.enabled = currLayer >= 0;
                if (GUI.Button(r, GC_rem, EditorStyles.miniButtonRight))
                {
                    Undo.RecordObject(asset, "Remove Layer");
                    ArrayUtility.RemoveAt(ref asset.maps[mapIdx].layers, currLayer);
                    EditorUtility.SetDirty(asset);
                    currLayer--;
                    doRepaint = true;
                }
                r.x -= 25f;
                GUI.enabled = true;
                if (GUI.Button(r, GC_add, EditorStyles.miniButtonLeft))
                {
                    Undo.RecordObject(asset, "Add Layer");
                    ArrayUtility.Add(ref asset.maps[mapIdx].layers, new GameMapLayer());
                    asset.maps[mapIdx].InitLayer(asset.maps[mapIdx].layers.Length); // not -1 since special handling of grid[] vs layers[].grid
                    EditorUtility.SetDirty(asset);
                    doRepaint = true;
                }
            }

            EditorGUI.BeginChangeCheck();
            showLayers = EditorGUILayout.Foldout(showLayers, GC_LayersHead, true);
            if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool("plyGameMapEd.showLayers", showLayers);
            if (showLayers)
            {
                if (layerHidden.Length != asset.maps[mapIdx].layers.Length + 1)
                {
                    layerHidden = new bool[asset.maps[mapIdx].layers.Length + 1];
                }

                EditorGUILayout.Space();
                for (int i = -1; i < asset.maps[mapIdx].layers.Length; i++)
                {
                    DrawLayerEntry(i);
                }
                EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawLayerEntry(int idx)
    {
        EditorGUILayout.BeginHorizontal();
        {
            layerHidden[idx + 1] = !GUILayout.Toggle(!layerHidden[idx + 1], GC_Viz, EditorStyles.miniButton, GUILayout.Width(25));
            if (GUILayout.Toggle((idx == currLayer), "Layer " + (idx + 1), EditorStyles.miniButton)) currLayer = idx;
        }
        EditorGUILayout.EndHorizontal();
    }

	private void DrawTileProperties(Event ev)
	{
		if (asset == null || asset.tileAsset == null || tilesEd == null) return;

		GUILayout.Box(GUIContent.none, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(3f));
		Rect r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
		{
            if (showTileProperties)
            {
                // tile manipulation buttons
                r.height = EditorGUIUtility.singleLineHeight;
                r.x = r.xMax - 28f; r.width = 25f; r.height = 15f;

                GUI.enabled = tileIdx >= 0;
                if (GUI.Button(r, GC_rem, EditorStyles.miniButtonRight))
                {
                    if (autoTileSelected)
                    {
                        if (tileCache.ContainsKey(asset.tileAsset.autoTiles[tileIdx].tiles[0].id)) tileCache.Remove(asset.tileAsset.autoTiles[tileIdx].tiles[0].id);
                        Undo.RecordObject(asset.tileAsset, "Remove Auto-Tile Definition");
                        asset.tileAsset.RemoveAutoTileAtIndex(tileIdx);
                        EditorUtility.SetDirty(asset.tileAsset);
                        tileIdx--; if (tileIdx < 0 && asset.tileAsset.autoTiles.Count > 0) tileIdx = 0;
                        if (tileIdx < 0 && asset.tileAsset.tiles.Count > 0) { tileIdx = 0; autoTileSelected = false; }
                    }
                    else
                    {
                        if (tileCache.ContainsKey(asset.tileAsset.tiles[tileIdx].id)) tileCache.Remove(asset.tileAsset.tiles[tileIdx].id);
                        Undo.RecordObject(asset.tileAsset, "Remove Tile Definition");
                        asset.tileAsset.RemoveTileAtIndex(tileIdx);
                        EditorUtility.SetDirty(asset.tileAsset);
                        tileIdx--; if (tileIdx < 0 && asset.tileAsset.tiles.Count > 0) tileIdx = 0;
                        if (tileIdx < 0 && asset.tileAsset.autoTiles.Count > 0) { tileIdx = 0; autoTileSelected = true; }
                    }
                    doRepaint = true;
                }
                r.x -= 25f;
                GUI.enabled = true;
                if (GUI.Button(r, GC_add, EditorStyles.miniButtonLeft))
                {
                    if (addTileMenu == null)
                    {
                        addTileMenu = new GenericMenu();
                        addTileMenu.AddItem(new GUIContent("Tile"), false, OnAddTile, 0);
                        addTileMenu.AddItem(new GUIContent("Auto-16Tile"), false, OnAddTile, 1);
                        addTileMenu.AddItem(new GUIContent("Auto-46Tile"), false, OnAddTile, 2);
                    }

                    addTileMenu.ShowAsContext();
                }

                r.x -= 28f;
                GUI.enabled = !autoTileSelected && tileIdx < asset.tileAsset.tiles.Count - 1;
                if (GUI.Button(r, GC_movR, EditorStyles.miniButtonRight))
                {
                    Undo.RecordObject(asset.tileAsset, "Move Tile Definition");
                    GameMapTile t = asset.tileAsset.tiles[tileIdx];
                    asset.tileAsset.tiles.RemoveAt(tileIdx);
                    asset.tileAsset.tiles.Insert(++tileIdx, t);
                    EditorUtility.SetDirty(asset.tileAsset);
                    doRepaint = true;
                }
                r.x -= 25f;
                GUI.enabled = !autoTileSelected && tileIdx > 0;
                if (GUI.Button(r, GC_movL, EditorStyles.miniButtonLeft))
                {
                    Undo.RecordObject(asset.tileAsset, "Move Tile Definition");
                    GameMapTile t = asset.tileAsset.tiles[tileIdx];
                    asset.tileAsset.tiles.RemoveAt(tileIdx);
                    asset.tileAsset.tiles.Insert(--tileIdx, t);
                    EditorUtility.SetDirty(asset.tileAsset);
                    doRepaint = true;
                }
                GUI.enabled = true;

                r.x -= 28f;
                if (GUI.Button(r, GC_clear, EditorStyles.miniButton))
                {
                    pasting = false;
                    marking = false;
                    clearMarked = false;
                    markedTiles.Clear();
                    tileIdx = -1;
                    autoTileSelected = false;
                    doRepaint = true;
                }
            }

			EditorGUI.BeginChangeCheck();
			showTileProperties = EditorGUILayout.Foldout(showTileProperties, GC_TilesHead, true);
			if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool("plyGameMapEd.showTileProps", showTileProperties);

			if (showTileProperties && tileIdx >= 0 && ((!autoTileSelected && tileIdx < asset.tileAsset.tiles.Count) || (autoTileSelected && tileIdx < asset.tileAsset.autoTiles.Count)))
			{
				SerializedObject obj = tilesEd.serializedObject;
				obj.Update();

				SerializedProperty tileProp = null;
				SerializedProperty endprop = null;

				if (autoTileSelected)
				{
					SerializedProperty tilesProp = obj.FindProperty("autoTiles");
					tileProp = tilesProp.GetArrayElementAtIndex(tileIdx);
					tilesProp = tileProp.FindPropertyRelative("tiles");
					tileProp = tilesProp.GetArrayElementAtIndex(0);
					endprop = tileProp.GetEndProperty();

					if (GUILayout.Button(GC_EditAuto, EditorStyles.miniButton, GUILayout.Width(EditorGUIUtility.labelWidth)))
					{
						GameMapAutoTileEd.Show_GameMapAutoTileEd(asset.tileAsset, asset.tileAsset.autoTiles[tileIdx], OnAutoTileChange);
					}
				}
				else
				{
					SerializedProperty tilesProp = obj.FindProperty("tiles");
					tileProp = tilesProp.GetArrayElementAtIndex(tileIdx);
					endprop = tileProp.GetEndProperty();
					EditorGUILayout.Space();
				}

				bool enterChildren = true;
				bool didChange = false;
				while (tileProp.NextVisible(enterChildren))
				{
					enterChildren = false;
					if (SerializedProperty.EqualContents(tileProp, endprop)) break;
					if (tileProp.name == "id") continue;
					if (tileProp.name == "_aid") continue;
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(tileProp, true, new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck()) didChange = true;
				}

				obj.ApplyModifiedProperties();
				EditorGUILayout.Space();
				if (didChange)
				{
					TileDef def = null;
					GameMapTile t = autoTileSelected ? asset.tileAsset.autoTiles[tileIdx].tiles[0] : asset.tileAsset.tiles[tileIdx];
					if (tileCache.TryGetValue(t.id, out def))
					{
						UpdateCachedValues(def, t, def.isAuto);
					}
				}
			}
		}
		EditorGUILayout.EndVertical();

		// draw the tile list
		DrawTileList(ev);
	}

	private void OnAutoTileChange(int idx)
	{
		if (autoTileSelected && tileIdx >= 0 && tileIdx < asset.tileAsset.autoTiles.Count)
		{
			TileDef def = null;
			GameMapTile t = asset.tileAsset.autoTiles[tileIdx].tiles[idx];
			if (tileCache.TryGetValue(t.id, out def)) UpdateCachedValues(def, t, def.isAuto);
			Repaint();
		}
	}

	private void OnAddTile(object arg)
	{
		int opt = (int)arg;

		if (opt == 0)
		{
			Undo.RecordObject(asset.tileAsset, "Add Tile Definition");
			asset.tileAsset.AddTile();
			EditorUtility.SetDirty(asset.tileAsset);
			tileIdx = asset.tileAsset.tiles.Count - 1;
			autoTileSelected = false;

			TileDef def = new TileDef() { id = asset.tileAsset.tiles[tileIdx].id };
			UpdateCachedValues(def, asset.tileAsset.tiles[tileIdx], false);
			tileCache.Add(def.id, def);
		}

		else if (opt == 1 || opt == 2)
		{
			Undo.RecordObject(asset.tileAsset, "Add Auto-Tile Definition");
			asset.tileAsset.AddAutoTile(opt == 2);
			EditorUtility.SetDirty(asset.tileAsset);
			tileIdx = asset.tileAsset.autoTiles.Count - 1;
			autoTileSelected = true;

			TileDef def = new TileDef() { id = asset.tileAsset.autoTiles[tileIdx].tiles[0].id };
			UpdateCachedValues(def, asset.tileAsset.autoTiles[tileIdx].tiles[0], true);
			tileCache.Add(def.id, def);
		}

		doRepaint = true;
	}

	private void DrawTileList(Event ev)
	{
		GUILayout.Box(GUIContent.none, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(3f));

		Rect rr = GUILayoutUtility.GetLastRect();
		if (ev.type == EventType.Repaint)
		{
			tileListH = Mathf.Ceil(asset.tileAsset.tiles.Count / ((rr.width - 10f) / (tileDrawSz + 5f)));
			tileListH += 1f;
			tileListH = tileListH * (tileDrawSz + 5f);
		}

		rr = GUILayoutUtility.GetRect(1f, tileListH, GUILayout.ExpandWidth(true));
		Rect r = new Rect(5f, rr.y, tileDrawSz, tileDrawSz);

		GUI.color = Color.white;
		GUI.contentColor = Color.white;
		GUI.backgroundColor = Color.white;

		// *** auto-tiles
		for (int i = 0; i < asset.tileAsset.autoTiles.Count; i++)
		{
			GameMapTile tile = asset.tileAsset.autoTiles[i].tiles[0];
			TileDef def = null;
			if (!tileCache.TryGetValue(tile.id, out def))
			{
				Debug.LogError("This should not happen.");
				continue;
			}

			if (autoTileSelected && i == tileIdx && ev.type == EventType.Repaint)
			{
				GUI.skin.box.Draw(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), false, false, false, false);
			}

			GUI.color = def.color;

			if (def.sprite != null)
			{
				Rect r2 = r;
				if (def.flipped) { r2.x += r.width; r2.width = -r.width; }
				GUI.DrawTextureWithTexCoords(r2, def.sprite.texture, def.rect, true);
				Styles.Tile.normal.background = null;
			}
			else
			{
				Styles.Tile.normal.background = EditorGUIUtility.whiteTexture;
			}

			TileContent.text = def.text;
			if (GUI.Button(r, TileContent, Styles.Tile))
			{
				tileIdx = i;
				autoTileSelected = true;
				doRepaint = true;
			}

			GUI.color = Color.white;

			r.x += tileDrawSz + 5f;
			if (r.x > rr.xMax - (tileDrawSz + 5f)) { r.x = 5f; r.y += tileDrawSz + 5f; }
		}

		// *** normal tiles
		for (int i = 0; i < asset.tileAsset.tiles.Count; i++)
		{
			GameMapTile tile = asset.tileAsset.tiles[i];
			TileDef def = null;
			if (!tileCache.TryGetValue(tile.id, out def))
			{
				Debug.LogError("This should not happen.");
				continue;
			}

			if (!autoTileSelected && i == tileIdx && ev.type == EventType.Repaint)
			{
				GUI.skin.box.Draw(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), false, false, false, false);
			}

			GUI.color = def.color;

			if (def.sprite != null)
			{
				Rect r2 = r;
				if (def.flipped) { r2.x += r.width; r2.width = -r.width; }
				GUI.DrawTextureWithTexCoords(r2, def.sprite.texture, def.rect, true);
				Styles.Tile.normal.background = null;
			}
			else
			{
				Styles.Tile.normal.background = EditorGUIUtility.whiteTexture;
			}

			TileContent.text = def.text;
			if (GUI.Button(r, TileContent, Styles.Tile))
			{
				tileIdx = i;
				autoTileSelected = false;
				doRepaint = true;
			}

			GUI.color = Color.white;

			// handle drag&drop of sprite onto tile list
			if (sprField != null)
			{
				if (ev.type == EventType.DragUpdated && r.Contains(ev.mousePosition))
				{
					dragDropTarget = i;
					DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
					ev.Use();
				}

				if (ev.type == EventType.DragExited)
				{
					dragDropTarget = -1;
				}

				if (ev.type == EventType.DragPerform && dragDropTarget == i)
				{
					DragAndDrop.AcceptDrag();
					ev.Use();
					if (DragAndDrop.objectReferences.Length > 0)
					{
						Sprite sp = DragAndDrop.objectReferences[0] as Sprite;
						if (sp == null)
						{
							Texture2D t = DragAndDrop.objectReferences[0] as Texture2D;
							if (t != null)
							{
								UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(t));
								if (objs.Length > 1) sp = objs[1] as Sprite; // [0] is texture/parent so [1] is the 1st sprite in it
							}
						}
						if (sp != null)
						{
							//sprField.SetValue(asset.tileAsset.tiles[i], sp);
							//EditorUtility.SetDirty(asset.tileAsset);

							SerializedObject obj = tilesEd.serializedObject;
							obj.Update();
							SerializedProperty tilesProp = obj.FindProperty("tiles");
							SerializedProperty tileP = tilesProp.GetArrayElementAtIndex(i);
							SerializedProperty spriteP = tileP.FindPropertyRelative(sprField.Name);
							spriteP.objectReferenceValue = sp;
							obj.ApplyModifiedProperties();

							UpdateCachedValues(def, asset.tileAsset.tiles[i], def.isAuto);
							doRepaint = true;
						}
					}
					dragDropTarget = -1;
				}
			}

			// ...
			r.x += tileDrawSz + 5f;
			if (r.x > rr.xMax - (tileDrawSz + 5f)) { r.x = 5f; r.y += tileDrawSz + 5f; }
		}
	}

	#endregion
	// ----------------------------------------------------------------------------------------------------------------
	#region canvas

	private void DrawCanvas(Event ev)
	{
		bool changed = GUI.changed;
		GUI.changed = false;

		GUILayout.Space(7);
		scroll[1] = EditorGUILayout.BeginScrollView(scroll[1]);
		{
			if (asset == null || asset.tileAsset == null || mapIdx < 0)
			{
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndScrollView();
				return;
			}

			GameMap map = asset.maps[mapIdx];
			float w = map.width * tileDrawSz;
			float h = map.height * tileDrawSz;
			Rect mainRect = GUILayoutUtility.GetRect(w + 20f, h + 20f, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
			mainRect.y += 3f; mainRect.width = w; mainRect.height = h;

			int controlID = GUIUtility.GetControlID(EditorCanvasHash, FocusType.Keyboard);
			if (ev.GetTypeForControl(controlID) == EventType.Repaint)
			{
				// black background
				GUI.backgroundColor = backColor;
				Styles.SolidWhite.Draw(mainRect, false, false, false, false);
				GUI.backgroundColor = Color.white;

                if (layerHidden.Length != asset.maps[mapIdx].layers.Length + 1)
                {
                    layerHidden = new bool[asset.maps[mapIdx].layers.Length + 1];
                }

                // draw placed tiles
                Rect r = new Rect(mainRect.x, mainRect.yMax - tileDrawSz, tileDrawSz, tileDrawSz);
                for (int i = map.layers.Length - 1; i >= 0; i--)
                {
                    if (!layerHidden[i + 1]) DrawGridTiles(mainRect, r, map.layers[i].grid);
                }

                if (!layerHidden[0]) DrawGridTiles(mainRect, r, map.grid);

				// draw marked tiles overlay
				if (markedTiles.Count > 0)
				{
					GUI.backgroundColor = new Color(0f, 1f, 0f, 0.5f);
					foreach (int idx in markedTiles)
					{
						int x, y;
						map.IdxToPosition(idx, out x, out y);
						r.x = mainRect.x + (x * tileDrawSz);
						r.y = mainRect.yMax - ((y+1) * tileDrawSz);
						Styles.SolidWhite.Draw(r, false, false, false, false);
					}
					GUI.backgroundColor = Color.white;
				}

				// grid over everything
				DrawGrid(mainRect, map.width, map.height);

				// draw tiles to be pasted
				if (pasting)
				{
					float offsX = mainRect.x + Mathf.Floor(ev.mousePosition.x / tileDrawSz) * tileDrawSz;
					float offsY = mainRect.y + Mathf.Floor(ev.mousePosition.y / tileDrawSz) * tileDrawSz;
					foreach (TileDef tile in copyBuffer)
					{
						r.x = offsX + (tile.x * tileDrawSz);
						r.y = offsY - (tile.y * tileDrawSz);
						if (r.x < mainRect.x || r.y < mainRect.y || r.x >= mainRect.xMax || r.y >= mainRect.yMax) continue;
						if (tile.id >= 0) DrawTile(r, tile);
						GUI.backgroundColor = new Color(0f, 1f, 0f, 0.5f);
						Styles.SolidWhite.Draw(r, false, false, false, false);
					}

					GUI.backgroundColor = Color.white;
				}
			}

			HandleCanvasEvents(mainRect, map, ev, controlID);
		}
		EditorGUILayout.EndScrollView();

		if (GUI.changed)
		{
			EditorUtility.SetDirty(asset);
			GUI.changed = changed;
		}
	}

    private void DrawGridTiles(Rect mainRect, Rect r, int[] g)
    {
        for (int i = 0; i < g.Length; i++)
        {
            TileDef def = null;
            if (tileCache.TryGetValue(g[i], out def)) DrawTile(r, def);

            r.x += tileDrawSz;
            if (r.x >= mainRect.xMax) { r.x = mainRect.x; r.y -= tileDrawSz; }
        }
    }

    private void HandleCanvasEvents(Rect mainRect, GameMap map, Event ev, int controlID)
	{
		switch (ev.GetTypeForControl(controlID))
		{
			case EventType.MouseDown:
			{
				if (mainRect.Contains(ev.mousePosition))
				{
					if (GUIUtility.hotControl == 0)
					{
						GUIUtility.hotControl = controlID;
						GUIUtility.keyboardControl = controlID;

						if (pasting)
						{
							if (ev.button == 0)
							{
								DoPasteTiles(mainRect, ev);
							}
							else if (ev.button == 1)
							{
								pasting = false;
								doRepaint = true;
							}
						}
						else if (ev.button <= 1)
						{
							GridClick(mainRect, map, ev);
							doRepaint = true;
						}

						ev.Use();
					}
				}
				else
				{
					if (GUIUtility.keyboardControl == controlID)
					{
						GUIUtility.keyboardControl = 0;
					}
				}
			} break;

			case EventType.MouseUp:
			{
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
					doRepaint = true;
					ev.Use();
				}
			} break;

			case EventType.MouseMove:
			{
				if (pasting) doRepaint = true;
			} break;

			case EventType.MouseDrag:
			{
				if (!pasting && GUIUtility.hotControl == controlID)
				{
					if (ev.button == 2)
					{
						scroll[1] = scroll[1] - ev.delta;
					}
					else
					{
						GridClick(mainRect, map, ev);
					}

					doRepaint = true;
					ev.Use();
				}
			} break;

			case EventType.KeyDown:
			{
				if (GUIUtility.keyboardControl == controlID)
				{
					if (ev.keyCode == KeyCode.Escape)
					{
						if (pasting)
						{
							pasting = false;
							doRepaint = true;
							ev.Use();
						}
						else if (marking)
						{
							marking = false;
							clearMarked = false;
							markedTiles.Clear();
							doRepaint = true;
							ev.Use();
						}
					}
					else if (ev.keyCode == KeyCode.Delete && markedTiles.Count > 0)
					{
						DeleteMarkedTiles();
						ev.Use();
					}
				}
			} break;

			case EventType.ValidateCommand:
			{
				switch (ev.commandName)
				{
					case "Duplicate": case "Cut": case "Copy": case "Paste": case "Delete": ev.Use(); break;
				}
			} break;

			case EventType.ExecuteCommand:
			{
				switch (ev.commandName)
				{
					case "Duplicate": { if (markedTiles.Count > 0) { CopyMarkedTiles(true); EnterPasteMode(); ev.Use(); } } break;
					case "Cut": { if (markedTiles.Count > 0) { CopyMarkedTiles(false); DeleteMarkedTiles(); ev.Use(); } } break;
					case "Copy": { if (markedTiles.Count > 0) { CopyMarkedTiles(true); ev.Use(); } } break;
					case "Paste": if (copyBuffer.Count > 0) { { EnterPasteMode(); ev.Use(); } } break;
					case "Delete": { if (markedTiles.Count > 0) { DeleteMarkedTiles(); ev.Use(); } } break;
				}
			} break;
		}
	}

	private void DrawTile(Rect r, TileDef def)
	{
		GUI.color = def.color;

		if (def.sprite != null)
		{
			Rect rr = r;

			if (def.isAuto && refTileSz > 0.0f)
			{
				rr.width *= def.sprite.rect.width / refTileSz;
				rr.height *= def.sprite.rect.height / refTileSz;
			}

			if (def.flipped) { rr.x += rr.width; rr.width = -rr.width; }

			GUI.DrawTextureWithTexCoords(rr, def.sprite.texture, def.rect, true);
			Styles.Tile.normal.background = null;
		}
		else
		{
			Styles.Tile.normal.background = EditorGUIUtility.whiteTexture;
		}

		TileContent.text = def.text;
		Styles.Tile.Draw(r, TileContent, false, false, false, false);

		GUI.color = Color.white;
	}

	private void GridClick(Rect mainRect, GameMap map, Event ev)
	{
        int[] grid = (currLayer < 0 ? map.grid : map.layers[currLayer].grid);
        int idx = MousePosToGridIdx(map, ev);

		if (ev.modifiers == EventModifiers.Shift && ev.button == 0)
		{   // mark tiles in grid
			if (idx >= 0 && idx < grid.Length)
			{
				if (clearMarked)
				{
					if (markedTiles.Contains(idx)) markedTiles.Remove(idx);
					else if (ev.type == EventType.MouseDown)
					{
						clearMarked = false;
						markedTiles.Add(idx);
					}
				}
				else 
				{
					if (!markedTiles.Contains(idx)) markedTiles.Add(idx);
					else if (ev.type == EventType.MouseDown)
					{
						clearMarked = true;
						markedTiles.Remove(idx);
					}
					
				}

				marking = true;
				doRepaint = true;
			}
		}

		else if (idx >= 0 && idx < grid.Length)
		{   // place or delete tiles in grid
			if (autoTileSelected && ev.button == 0 && tileIdx >= 0 && tileIdx < asset.tileAsset.autoTiles.Count)
			{
				PaintAutoTile(map, idx);
			}

			else
			{
				int id = -1;
				if (ev.button == 0 && !autoTileSelected) id = (tileIdx >= 0 && tileIdx < asset.tileAsset.tiles.Count ? asset.tileAsset.tiles[tileIdx].id : -1);
				if (grid[idx] != id)
				{
					Undo.RecordObject(asset, id == -1 ? "Clear Tile" : "Place Tile");
					grid[idx] = id;
					doRepaint = true;
					GUI.changed = true;
				}
			}
		}
	}

	private void PaintAutoTile(GameMap map, int idx)
	{
        int[] grid = (currLayer < 0 ? map.grid : map.layers[currLayer].grid);
        int refAutoId = asset.tileAsset.autoTiles[tileIdx].id;
		bool is16Tile = (asset.tileAsset.autoTiles[tileIdx].tiles.Length == 16);

		int mainX, mainY;
		map.IdxToPosition(idx, out mainX, out mainY);
		int id = (is16Tile ? CalcAuto16TileId(map, refAutoId, mainX, mainY) : CalcAuto46TileId(map, refAutoId, mainX, mainY));

		if (grid[idx] != id)
		{
			doRepaint = true;
			GUI.changed = true;
			Undo.RecordObject(asset, "Place Auto-Tile");
			grid[idx] = id;

			// update the neighbouring tiles
			for (int ox = -1; ox <= 1; ox++)
			{
				for (int oy = -1; oy <= 1; oy++)
				{
					if (ox == 0 && oy == 0) continue;

					int x = mainX + ox;
					int y = mainY + oy;
					if (x < 0 || y < 0 || x >= map.width || y >= map.height) continue;

					idx = map.PositionToIdx(x, y);
					if (grid[idx] < 0) continue;

					GameMapTile t = asset.tileAsset.GetTile(grid[idx]);
					if (t == null || t._aid != refAutoId) continue;

					grid[idx] = (is16Tile ? CalcAuto16TileId(map, refAutoId, x, y) : CalcAuto46TileId(map, refAutoId, x, y));
				}
			}
		}
	}

	private int CalcAuto16TileId(GameMap map, int refAutoId, int x, int y)
	{
        int[] grid = (currLayer < 0 ? map.grid : map.layers[currLayer].grid);
        int invalid = outsideMapIsSolid ? -2 : -1;
		int[] neighbour_ids =
		{
			(y < map.height - 1 ? grid[map.PositionToIdx(x, y + 1)] : invalid ),
			(x > 0 ? grid[map.PositionToIdx(x - 1, y)] : invalid ),
			(x < map.width - 1 ? grid[map.PositionToIdx(x + 1, y)] : invalid ),
			(y > 0 ? grid[map.PositionToIdx(x, y - 1)] : invalid )
		};

		for (int i = 0; i < neighbour_ids.Length; i++)
		{
			if (neighbour_ids[i] == -2)
			{
				neighbour_ids[i] = 1;
				continue;
			}

			GameMapTile t = asset.tileAsset.GetTile(neighbour_ids[i]);
			neighbour_ids[i] = (t == null || t._aid != refAutoId ? 0 : 1);
		}

		int at_idx = (neighbour_ids[0] * 1 + neighbour_ids[1] * 2 + neighbour_ids[2] * 4 + neighbour_ids[3] * 8);
		return asset.tileAsset.autoTiles[tileIdx].tiles[at_idx].id;
	}

	private int CalcAuto46TileId(GameMap map, int refAutoId, int x, int y)
	{
        int[] grid = (currLayer < 0 ? map.grid : map.layers[currLayer].grid);
        int invalid = outsideMapIsSolid ? -2 : -1;
		int[] neighbour_ids =
		{
			(y < map.height - 1 ? grid[map.PositionToIdx(x, y + 1)] : invalid ),							// 1: top
			(x < map.width - 1 && y < map.height - 1 ? grid[map.PositionToIdx(x + 1, y + 1)] : invalid ),	// 2: top-right
			(x < map.width - 1 ? grid[map.PositionToIdx(x + 1, y)] : invalid ),								// 4: right
			(x < map.width - 1 && y > 0? grid[map.PositionToIdx(x + 1, y - 1)] : invalid ),					// 8: bottom-right
			(y > 0 ? grid[map.PositionToIdx(x, y - 1)] : invalid ),											// 16: bottom
			(x > 0 && y > 0 ? grid[map.PositionToIdx(x - 1, y - 1)] : invalid ),							// 32: bottom-left
			(x > 0 ? grid[map.PositionToIdx(x - 1, y)] : invalid ),											// 64: left
			(x > 0 && y < map.height - 1 ? grid[map.PositionToIdx(x - 1, y + 1)] : invalid ),				// 128: top-left
		};

		for (int i = 0; i < neighbour_ids.Length; i++)
		{
			if (neighbour_ids[i] == -2)
			{
				neighbour_ids[i] = 1;
				continue;
			}

			GameMapTile t = asset.tileAsset.GetTile(neighbour_ids[i]);
			neighbour_ids[i] = (t == null || t._aid != refAutoId ? 0 : 1);
		}

		int idx = (
			neighbour_ids[0] * 1 + 
			neighbour_ids[1] * 2 * (neighbour_ids[0] * neighbour_ids[2]) +		// corners are only valid if the sides are solid too
			neighbour_ids[2] * 4 + 
			neighbour_ids[3] * 8 * (neighbour_ids[2] * neighbour_ids[4]) +		// corners are only valid if the sides are solid too
			neighbour_ids[4] * 16 + 
			neighbour_ids[5] * 32 * (neighbour_ids[4] * neighbour_ids[6]) +		// corners are only valid if the sides are solid too
			neighbour_ids[6] * 64 +
			neighbour_ids[7] * 128 * (neighbour_ids[6] * neighbour_ids[0])		// corners are only valid if the sides are solid too
			);

		int mapped_idx;
		if (map64.TryGetValue(idx, out mapped_idx))
		{
			return asset.tileAsset.autoTiles[tileIdx].tiles[mapped_idx].id;
		}
		else
		{
			Debug.LogError("Missing mapping: " + idx);
		}

		return -1;
	}

	private void DrawGrid(Rect r, int w, int h)
	{
		GUI.backgroundColor = gridColor;

		Rect rr = r;
		rr.width = 1f;
		for (int i = 0; i <= w; i++)
		{
			rr.x = r.x + (i * tileDrawSz);
			Styles.SolidWhite.Draw(rr, false, false, false, false);
		}

		rr = r;
		rr.height = 1f;
		for (int i = 0; i <= h; i++)
		{
			rr.y = r.y + (i * tileDrawSz);
			Styles.SolidWhite.Draw(rr, false, false, false, false);
		}

		GUI.backgroundColor = Color.white;
	}

	private int MousePosToGridIdx(GameMap map, Event ev)
	{
		int x = Mathf.FloorToInt(ev.mousePosition.x / tileDrawSz);
		int y = Mathf.FloorToInt(ev.mousePosition.y / tileDrawSz);
		y = map.height - (y + 1); // map's Y starts from bottom-up
		if (x < 0 || y < 0 || x >= map.width || y >= map.height) return -1;
		return map.PositionToIdx(x, y);
	}

	private void CopyMarkedTiles(bool clearMarked)
	{
		copyBuffer.Clear();

		GameMap map = asset.maps[mapIdx];
        int[] grid = (currLayer < 0 ? map.grid : map.layers[currLayer].grid);

        int x=0, y=0, offsX=0, offsY=0;
		markedTiles.Sort((a, b) => a.CompareTo(b));
		map.IdxToPosition(markedTiles[markedTiles.Count / 2], out offsX, out offsY);

		foreach (int idx in markedTiles)
		{
			map.IdxToPosition(idx, out x, out y);
			TileDef t = (grid[idx] >= 0 ? tileCache[grid[idx]].Copy() : new TileDef() { id = -1 });
			t.x = x - offsX;
			t.y = y - offsY;
			copyBuffer.Add(t);
		}

		if (clearMarked) markedTiles.Clear();
		doRepaint = true;
	}

	private void DeleteMarkedTiles()
	{
		Undo.RecordObject(asset, "Remove Tiles");

        GameMap map = asset.maps[mapIdx];
        int[] grid = (currLayer < 0 ? map.grid : map.layers[currLayer].grid);
        foreach (int idx in markedTiles)
		{
			grid[idx] = -1;
		}

		markedTiles.Clear();
		GUI.changed = true;
		doRepaint = true;
	}

	private void EnterPasteMode()
	{
		pasting = true;
		markedTiles.Clear();
		doRepaint = true;
	}

	private void DoPasteTiles(Rect mainRect, Event ev)
	{
		Undo.RecordObject(asset, "Paste Tiles");

		GameMap map = asset.maps[mapIdx];
        int[] grid = (currLayer < 0 ? map.grid : map.layers[currLayer].grid);
		int xOffs = Mathf.FloorToInt(ev.mousePosition.x / tileDrawSz);
		int yOffs = Mathf.FloorToInt(ev.mousePosition.y / tileDrawSz);
		yOffs = map.height - (yOffs + 1); // map's Y starts from bottom-up
		foreach (TileDef tile in copyBuffer)
		{
			int x = xOffs + tile.x;
			int y = yOffs + tile.y;
			if (x < 0 || x >= map.width || y < 0 || y >= map.height) continue;

			grid[map.PositionToIdx(x, y)] = tile.id;
		}

		GUI.changed = true;
		doRepaint = true;
	}

	#endregion
	// ----------------------------------------------------------------------------------------------------------------
	#region helpers

	private T LoadOrCreateAsset<T>(string fn, bool createIfNotExist = true)
		where T : ScriptableObject
	{
		if (!fn.StartsWith(Application.dataPath)) return null;
		try
		{
			fn = "Assets" + fn.Replace(Application.dataPath, "");
			ScriptableObject asset = AssetDatabase.LoadAssetAtPath(fn, typeof(T)) as ScriptableObject;
			if (asset == null && createIfNotExist)
			{
				asset = CreateInstance(typeof(T));
				AssetDatabase.CreateAsset(asset, fn);
				AssetDatabase.SaveAssets();
			}
			return (T)asset;
		}
		catch
		{
			return null;
		}
	}

	private ScriptableObject LoadOrCreateAsset(string fn, Type t, bool createIfNotExist = true)
	{
		if (!fn.StartsWith(Application.dataPath)) return null;
		try
		{
			fn = "Assets" + fn.Replace(Application.dataPath, "");
			ScriptableObject asset = AssetDatabase.LoadAssetAtPath(fn, t) as ScriptableObject;
			if (asset == null && createIfNotExist)
			{
				asset = CreateInstance(t);
				AssetDatabase.CreateAsset(asset, fn);
				AssetDatabase.SaveAssets();
			}
			return asset;
		}
		catch
		{
			return null;
		}
	}

	private bool StringIsUnique<T>(List<T> existingStrings, string str)
	{
		if (string.IsNullOrEmpty(str) || existingStrings == null) return false;
		for (int i = 0; i < existingStrings.Count; i++)
		{
			if (existingStrings[i] == null) continue;
			if (str.Equals(existingStrings[i].ToString())) return false;
		}
		return true;
	}

	private static void EditorPrefs_SetColor(string key, Color val)
	{
		EditorPrefs.SetString(key, string.Format("{0},{1},{2},{3}", val.r, val.g, val.b, val.a));
	}

	private static Color EditorPrefs_GetColor(string key, Color defaultValue)
	{
		Color res = defaultValue;
		string val = EditorPrefs.GetString(key, null);
		if (!string.IsNullOrEmpty(val))
		{
			string[] vals = val.Split(',');
			if (vals.Length == 4)
			{
				float.TryParse(vals[0], out res.r);
				float.TryParse(vals[1], out res.g);
				float.TryParse(vals[2], out res.b);
				float.TryParse(vals[3], out res.a);
			}
		}
		return res;
	}

	#endregion
	// ----------------------------------------------------------------------------------------------------------------
}
