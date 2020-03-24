using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Trap))]
public class TrapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Trap trap = (Trap)target;

        if (GUILayout.Button("Start"))
        {
            trap.StartTarget();
        }

        if (GUILayout.Button("Reset"))
        {
            trap.Reset();
        }
    }
}