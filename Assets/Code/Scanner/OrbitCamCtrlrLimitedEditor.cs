#if UNITY_EDITOR
namespace Scanner.Editors {
    using UnityEditor;
    using K3.Editors;

    [CustomEditor(typeof(OrbitingCameraControllerShipView))]
    class OrbitCamCtrlrLimitedEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();            
            var limitPhi = serializedObject.FindProperty("limitPhi");
            var limitTheta = serializedObject.FindProperty("limitTheta");
            EditorGUILayout.PropertyField(limitPhi);
            if (limitPhi.boolValue)
                    this.DoMinMaxField("Phi", serializedObject.FindProperty("minPhi"), serializedObject.FindProperty("maxPhi"), -180, 180);

            EditorGUILayout.PropertyField(limitTheta);
            if (limitTheta.boolValue)
                    this.DoMinMaxField("Theta", serializedObject.FindProperty("minTheta"), serializedObject.FindProperty("maxTheta"), -180, 180);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("orbitDistanceWheelFactor"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("panMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mousePhiMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mouseThetaMultiplier"));

            serializedObject.ApplyModifiedProperties();
        }
    }

}
#endif