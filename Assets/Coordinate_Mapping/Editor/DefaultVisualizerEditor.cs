using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoordinateMapper {

    [CustomEditor(typeof(DefaultVisualizer))]
    public class DefaultVisualizerEditor : Editor {
        SerializedProperty latitudeKey;
        SerializedProperty longitudeKey;
        SerializedProperty magnitudeKey;
        SerializedProperty dataFile;
        SerializedProperty pointPrefab;
        SerializedProperty keyFormat;
        SerializedProperty loadComplete;

        private void OnEnable() {
            latitudeKey = serializedObject.FindProperty("latitudeKey");
            longitudeKey = serializedObject.FindProperty("longitudeKey");
            magnitudeKey = serializedObject.FindProperty("magnitudeKey");
            dataFile = serializedObject.FindProperty("_dataFile");
            pointPrefab = serializedObject.FindProperty("pointPrefab");
            keyFormat = serializedObject.FindProperty("keyFormat");
            loadComplete = serializedObject.FindProperty("_loadComplete");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.LabelField(new GUIContent("Data Information"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(dataFile);
            EditorGUILayout.PropertyField(pointPrefab);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Key Information"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(keyFormat);

            var format = keyFormat.enumValueIndex;
            switch(format) {
                case (int)DataKeyFormat.JsonLatLngArrays:
                    EditorGUILayout.PropertyField(latitudeKey, new GUIContent("Latitude Array Key"));
                    EditorGUILayout.PropertyField(longitudeKey, new GUIContent("Longitude Array Key"));
                    break;
                case (int)DataKeyFormat.JsonLatAndLngKeys:
                case (int)DataKeyFormat.Csv:
                    EditorGUILayout.PropertyField(latitudeKey);
                    EditorGUILayout.PropertyField(longitudeKey);
                    break;
                case (int)DataKeyFormat.JsonSingleLatLngArray:
                    EditorGUILayout.PropertyField(latitudeKey, new GUIContent("Coordinates Array Key"));
                    break;
            }

            EditorGUILayout.PropertyField(magnitudeKey);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Load Complete Event"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(loadComplete);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
