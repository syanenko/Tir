using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SportingExercise))]
public class SportingExerciseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SportingExercise ex = (SportingExercise)target;

        if (GUILayout.Button("Start"))
        {
            ex.StartExercise();
        }

        if (GUILayout.Button("Reset"))
        {
            ex.Reset();
        }

        if (GUILayout.Button("Stop"))
        {
            ex.StopExercise();
        }
    }
}