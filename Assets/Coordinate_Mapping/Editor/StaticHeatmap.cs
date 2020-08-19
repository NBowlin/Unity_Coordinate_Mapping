using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;

namespace CoordinateMapper {
    public class StaticHeatmap : ScriptableWizard {
        [MenuItem("Coordinate Mapper/Generate Heatmap Texture")]
        static void CreateStaticHeatmap() {
            DisplayWizard<StaticHeatmap>("Create a heat map");
        }

        [SerializeField] private TextAsset dataFile = null;

        [SerializeField] private DataKeyFormat keyFormat = DataKeyFormat.JsonLatAndLngKeys;

        [SerializeField] private string latitudeKey = null;
        [SerializeField] private string longitudeKey = null;
        [SerializeField] private string magnitudeKey = null;

        [SerializeField] private float mPlanetRadius = 6371000f; //Planet's radius in meters
        [SerializeField] private float kmRange = 200f;
        [SerializeField] [Range(0, 100)] private int startValue = 60;
        [SerializeField] [Range(0, 100)] private int endValue = 0;
        [SerializeField] private Vector2 heatmapSize = new Vector2(2048, 1024);

        [SerializeField] private Gradient colors = new Gradient();

        private async void OnWizardCreate() {
            string path = EditorUtility.SaveFilePanelInProject("Save Heatmap Texture", "Static_Heatmap", "png", "Specify where to save the heatmap.");
            if (path.Length > 0) {
                DateTime before = DateTime.Now;
                var hm = await GenerateStaticHeatmap();
                DateTime after = DateTime.Now;
                TimeSpan duration = after.Subtract(before);
                Debug.Log("Heatmap generation time in seconds: " + duration.TotalSeconds);
                System.IO.File.WriteAllBytes(path, hm.EncodeToPNG());
            }
        }

        private async Task<Texture2D> GenerateStaticHeatmap() {
            var parser = new DefaultParser(dataFile.text, keyFormat, latitudeKey, longitudeKey, magnitudeKey);
            var points = await parser.HandleDefaultParsing();

            var grid = await Task.Run(() => HeatmapGenerator.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, mPlanetRadius, kmRange, startValue, endValue, points));
            var colorBytes = await Task.Run(() => HeatmapGenerator.CreateColorMap(grid, colors));
            var overlay = HeatmapGenerator.CreateHeatmapTexture((int)heatmapSize.x, (int)heatmapSize.y, colorBytes);

            return overlay;
        }

        private void OnGUI() {
            EditorGUILayout.LabelField(new GUIContent("Data Information"), EditorStyles.boldLabel);
            dataFile = (TextAsset)EditorGUILayout.ObjectField("Data File", dataFile, typeof(TextAsset), false);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Key Information"), EditorStyles.boldLabel);
            keyFormat = (DataKeyFormat)EditorGUILayout.EnumPopup(keyFormat);

            switch (keyFormat) {
                case DataKeyFormat.JsonLatLngArrays:
                    latitudeKey = EditorGUILayout.TextField("Latitude Array Key", latitudeKey);
                    longitudeKey = EditorGUILayout.TextField("Longitude Array Key", longitudeKey);
                    break;
                case DataKeyFormat.JsonLatAndLngKeys:
                case DataKeyFormat.Csv:
                    latitudeKey = EditorGUILayout.TextField("Latitude Key", latitudeKey);
                    longitudeKey = EditorGUILayout.TextField("Longitude Key", longitudeKey);
                    break;
                case DataKeyFormat.JsonSingleLatLngArray:
                    latitudeKey = EditorGUILayout.TextField("Coordinates Array Key", latitudeKey);
                    break;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Heatmap Information"), EditorStyles.boldLabel);
            mPlanetRadius = EditorGUILayout.FloatField("Planet Radius (meters)", mPlanetRadius);
            kmRange = EditorGUILayout.FloatField("Range (kilometers)", kmRange);
            startValue = EditorGUILayout.IntSlider("Start Value", startValue, 0, 100);
            endValue = EditorGUILayout.IntSlider("End Value", endValue, 0, 100);
            heatmapSize = EditorGUILayout.Vector2Field("Heatmap Size", heatmapSize);

            EditorGUILayout.Space();

            colors = EditorGUILayout.GradientField("Colors", colors);

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Generate Heatmap")) { OnWizardCreate(); }
        }
    }
}
