using UnityEditor;

namespace NativeSensors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(GyroscopeMotion))]
    public class GyroscopeMotionEditor : Editor
    {
        SerializedProperty handler;
        SerializedProperty frequency;

        void OnEnable()
        {
            handler = serializedObject.FindProperty("handler");
            frequency = serializedObject.FindProperty("_frequency");
            GyroscopeMotion.frequency = frequency.intValue;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(handler);
            GyroscopeMotion.frequency = EditorGUILayout.IntSlider("Update Frequency", GyroscopeMotion.frequency, 1, 100);
            frequency.intValue = GyroscopeMotion.frequency;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
