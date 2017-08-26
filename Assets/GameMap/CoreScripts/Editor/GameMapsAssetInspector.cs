using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GameMapsAsset))]
public class GameMapsAssetInspector : Editor
{

	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Open Maps Editor"))
		{
			GameMapEditor.Open_GameMapEditor((GameMapsAsset)target);
		}
	}

	// ----------------------------------------------------------------------------------------------------------------
}
