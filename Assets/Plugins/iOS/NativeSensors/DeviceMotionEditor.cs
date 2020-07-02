using UnityEditor;

namespace NativeSensors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(DeviceMotion))]
    public class DeviceMotionEditor : Editor
    {
        SerializedProperty handler;
        SerializedProperty frequency;

        void OnEnable()
        {
            handler = serializedObject.FindProperty("handler");
            frequency = serializedObject.FindProperty("_frequency");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(handler);
            DeviceMotion.frequency = EditorGUILayout.IntSlider("Update Frequency", DeviceMotion.frequency, 1, 100);
            frequency.intValue = DeviceMotion.frequency;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
