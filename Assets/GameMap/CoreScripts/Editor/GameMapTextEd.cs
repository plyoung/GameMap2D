using UnityEngine;
using UnityEditor;


public class GameMapTextEd : EditorWindow
{
	public string text { get; private set; }

	private string label;
	private System.Action<GameMapTextEd> callback;
	private bool accepted = false;
	private bool lostFocus = false;

	private static GUIStyle BottomBarStyle = null;

	public static void ShowEd(string title, string label, string currText, System.Action<GameMapTextEd> callback)
	{
		GameMapTextEd wiz = GetWindow<GameMapTextEd>(true, title, true);
		wiz.label = label;
		wiz.text = currText;
		wiz.callback = callback;
		wiz.minSize = wiz.maxSize = new Vector2(250, 100);
		wiz.ShowUtility();
	}

	void OnFocus() { lostFocus = false; }
	void OnLostFocus() { lostFocus = true; }

	void Update()
	{
		if (lostFocus) Close();
		if (accepted && callback != null) callback(this);
	}

	void OnGUI()
	{
		if (BottomBarStyle == null)
		{
			BottomBarStyle = new GUIStyle(GUI.skin.FindStyle("ProjectBrowserBottomBarBg")) { padding = new RectOffset(3, 3, 8, 8), stretchHeight = false, stretchWidth = true, fixedHeight = 0, fixedWidth = 0 };
		}

		EditorGUILayout.Space();
		GUILayout.Label(label);
		text = EditorGUILayout.TextField(text);
		GUILayout.FlexibleSpace();
		EditorGUILayout.BeginHorizontal(BottomBarStyle);
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Accept", GUILayout.Width(80))) accepted = true;
			GUILayout.Space(5);
			if (GUILayout.Button("Cancel", GUILayout.Width(80))) Close();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
	}

	// ----------------------------------------------------------------------------------------------------------------
}
