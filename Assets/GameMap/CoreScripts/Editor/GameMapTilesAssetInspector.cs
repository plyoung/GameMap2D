using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GameMapTilesAsset))]
public class GameMapTilesAssetInspector : Editor
{

	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Open Maps Editor"))
		{
			GameMapEditor.Open_GameMapEditor();
		}
	}

	// ----------------------------------------------------------------------------------------------------------------
}
