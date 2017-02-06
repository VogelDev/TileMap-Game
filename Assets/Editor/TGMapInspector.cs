using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(TGMap))]
public class TGMapInspector : Editor {

    //float v = .5f;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //EditorGUILayout.BeginVertical();
        //v = EditorGUILayout.Slider(v, 0, 2.0f);
        //EditorGUILayout.EndVertical();

        if (GUILayout.Button("Regenerate"))
        {
            TGMap tileMap = (TGMap)target;
            tileMap.BuildMesh();
        }
    }
}
