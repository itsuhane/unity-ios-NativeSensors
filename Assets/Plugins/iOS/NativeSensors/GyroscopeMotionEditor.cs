using UnityEditor;

namespace NativeSensors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(AccelerometerMotion))]
    public class AccelerometerMotionEditor : Editor
    {
        SerializedProperty handler;
        SerializedProperty frequency;

        void OnEnable()
        {
            handler = serializedObject.FindProperty("handler");
            frequency = serializedObject.FindProperty("_frequency");
            AccelerometerMotion.frequency = frequency.intValue;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(handler);
            AccelerometerMotion.frequency = EditorGUILayout.IntSlider("Update Frequency", AccelerometerMotion.frequency, 1, 100);
            frequency.intValue = AccelerometerMotion.frequency;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
