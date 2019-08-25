using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor
{

	public override void OnInspectorGUI() {
		MapGenerator mapGen = (MapGenerator) target;

		if (DrawDefaultInspector()) {
			if(mapGen.autoUpdate) {
				mapGen.DrawMapInEditor();
			}
		}

		if(GUILayout.Button("Generate")) {
            Debug.Log("Generating terrain");
            LogWriter.Log("Generating terrain");
            mapGen.DrawMapInEditor();
		}
	}

}
