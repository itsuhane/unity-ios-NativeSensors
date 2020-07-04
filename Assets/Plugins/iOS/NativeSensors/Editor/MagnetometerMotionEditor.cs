using UnityEditor;

namespace NativeSensors
{
    [CustomEditor(typeof(MagnetometerMotion))]
    public class MagnetometerMotionEditor : Editor
    {
        SerializedProperty handler;
        SerializedProperty frequency;

        void OnEnable()
        {
            handler = serializedObject.FindProperty("handler");
            frequency = serializedObject.FindProperty("_frequency");
            MagnetometerMotion.frequency = frequency.intValue;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(handler);
            MagnetometerMotion.frequency = EditorGUILayout.IntSlider("Update Frequency", MagnetometerMotion.frequency, 1, 100);
            frequency.intValue = MagnetometerMotion.frequency;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
