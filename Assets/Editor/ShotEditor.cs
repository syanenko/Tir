using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Shot))]
public class ShotEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Shot shot = (Shot)target;

        if (GUILayout.Button("Save Settings"))
        {
            shot.SaveSettings();
        }

        if (GUILayout.Button("Load Settings"))
        {
            shot.LoadSettings();
        }

        if (GUILayout.Button("Reset to Default"))
        {
            shot.ResetToDefault();
        }
    }
}