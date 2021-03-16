using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Exercise))]
public class ExerciseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Exercise ex = (Exercise)target;

        if (GUILayout.Button("Show attempt"))
        {
            ex.ShowAttempt();
        }

        if (GUILayout.Button("Begin"))
        {
            ex.Begin();
        }

        if (GUILayout.Button("End"))
        {
            ex.End();
        }
    }
}