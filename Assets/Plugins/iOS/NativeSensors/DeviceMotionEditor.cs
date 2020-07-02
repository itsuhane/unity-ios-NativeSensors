using UnityEditor;

namespace NativeSensors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(DeviceMotion))]
    public class DeviceMotionEditor : Editor
    {
        SerializedProperty handler;
        SerializedProperty frequency;
        SerializedProperty referenceFrame;

        void OnEnable()
        {
            handler = serializedObject.FindProperty("handler");
            frequency = serializedObject.FindProperty("_frequency");
            referenceFrame = serializedObject.FindProperty("_attitudeReferenceX");
            DeviceMotion.frequency = frequency.intValue;
            DeviceMotion.attitudeReferenceX = (AttitudeReferenceFrameX)referenceFrame.enumValueIndex;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(handler);

            DeviceMotion.frequency = EditorGUILayout.IntSlider("Update Frequency", DeviceMotion.frequency, 1, 100);
            frequency.intValue = DeviceMotion.frequency;

            DeviceMotion.attitudeReferenceX = (AttitudeReferenceFrameX)EditorGUILayout.EnumPopup("Attitude Reference X", DeviceMotion.attitudeReferenceX);
            referenceFrame.enumValueIndex = (int)DeviceMotion.attitudeReferenceX;

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
