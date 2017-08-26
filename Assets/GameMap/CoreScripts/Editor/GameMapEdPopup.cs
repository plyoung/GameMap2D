using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


public class GameMapEdPopup : PopupWindowContent
{
	public GameMapsAsset Asset { get; set; }
	public System.Action<int> OnMapSelected { get; set; }

	private ReorderableList list;
	private Rect contentRect;
	private Vector2 scroll = Vector2.zero;

	private static readonly Vector2 popupSz = new Vector2(200f, 200f);
	private static readonly GUIContent GC_Head = new GUIContent("Maps");
	private static readonly GUIContent GC_Add = new GUIContent("+");
	private static readonly GUIContent GC_Rem = new GUIContent("-");

	public override void OnOpen()
	{
		list = new ReorderableList(Asset.maps, typeof(GameMap), true, false, false, false)
		{
			elementHeight = EditorGUIUtility.singleLineHeight,
			headerHeight = 0,
			drawElementCallback = DrawElement,
			onSelectCallback = SelectMap
		};

		contentRect = new Rect(0f, 18f, popupSz.x - 20f, Mathf.Max(popupSz.y - 18, EditorGUIUtility.singleLineHeight * Asset.maps.Count + 5));
	}

	public override void OnClose()
	{
		list = null;
		Asset = null;
		OnMapSelected = null;
	}

	public override Vector2 GetWindowSize()
	{
		return popupSz;
	}

	public override void OnGUI(Rect r)
	{
		DrawHeader(new Rect(0, 0, r.width, 18));
		r.y += 18; r.height -= 18;
		scroll = GUI.BeginScrollView(r, scroll, contentRect, false, true);
		list.DoList(r);
		GUI.EndScrollView();

	}

	private void DrawHeader(Rect r)
	{
		if (ReorderableList.defaultBehaviours != null)
		{
			ReorderableList.defaultBehaviours.DrawHeaderBackground(r);
		}

		GUI.Label(r, GC_Head, EditorStyles.boldLabel);
		r.x = r.xMax - 25; r.width = 25;
		GUI.enabled = list.index >= 0;
		if (GUI.Button(r, GC_Rem, EditorStyles.miniButtonRight))
		{
			if (list.index >= 0 && list.index < Asset.maps.Count)
			{
				Undo.RecordObject(Asset, "Remove Game Map");
				Asset.RemoveMapAtIndex(list.index);
				contentRect = new Rect(0f, 18f, popupSz.x - 20f, Mathf.Max(popupSz.y - 18, EditorGUIUtility.singleLineHeight * Asset.maps.Count + 5));
				OnMapSelected(-1);
			}
		}

		GUI.enabled = true;
		r.x -= 25;
		if (GUI.Button(r, GC_Add, EditorStyles.miniButtonLeft))
		{
			Undo.RecordObject(Asset, "Add Game Map");
			Asset.AddMap();
			contentRect = new Rect(0f, 18f, popupSz.x - 20f, Mathf.Max(popupSz.y - 18, EditorGUIUtility.singleLineHeight * Asset.maps.Count + 5));
			OnMapSelected(Asset.maps.Count - 1);
		}
	}

	private void DrawElement(Rect r, int index, bool isActive, bool isFocused)
	{
		GUI.Label(r, Asset.maps[index].ToString());
	}

	private void SelectMap(ReorderableList list)
	{
		OnMapSelected(list.index);
	}

	// ----------------------------------------------------------------------------------------------------------------
}
