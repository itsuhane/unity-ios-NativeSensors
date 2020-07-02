using UnityEditor;

namespace NativeSensors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(HeadphoneMotion))]
    public class HeadphoneMotionEditor : Editor
    {
        SerializedProperty handler;

        void OnEnable()
        {
            handler = serializedObject.FindProperty("handler");

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(handler);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
