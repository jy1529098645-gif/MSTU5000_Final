using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundSystem))]
public class SoundSystemEditor : Editor
{
    SerializedProperty audibleRangeProp;
    SerializedProperty falloffProp;

    void OnEnable()
    {
        audibleRangeProp = serializedObject.FindProperty("audibleRange");
        falloffProp = serializedObject.FindProperty("falloffExponent");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default fields but keep serialized properties in sync
        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        SoundSystem ss = (SoundSystem)target;
        Transform t = ss.transform;

        serializedObject.Update();
        float radius = audibleRangeProp.floatValue;

        // Draw three wire discs to suggest a sphere
        Handles.color = new Color(0f, 0.6f, 1f, 0.4f);
        Handles.DrawWireDisc(t.position, Vector3.up, radius);
        Handles.DrawWireDisc(t.position, Vector3.right, radius);
        Handles.DrawWireDisc(t.position, Vector3.forward, radius);

        // Radius handle for interactive editing
        EditorGUI.BeginChangeCheck();
        float newRadius = Handles.RadiusHandle(Quaternion.identity, t.position, radius);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(ss, "Change Audible Range");
            audibleRangeProp.floatValue = Mathf.Max(0.01f, newRadius);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(ss);
        }

        // Label showing current value
        Handles.Label(t.position + Vector3.up * radius, $"Audible Range: {radius:F1}");
    }
}
