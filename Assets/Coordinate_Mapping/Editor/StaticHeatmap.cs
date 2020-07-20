using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace CoordinateMapper {
    public class StaticHeatmap : ScriptableWizard {
        [MenuItem("Coordinate Mapper/Generate Heatmap Texture")]
        static void CreateStaticHeatmap() {
            DisplayWizard<StaticHeatmap>("Create a heat map");
        }

        [SerializeField] private TextAsset json;

        [SerializeField] private float mPlanetRadius = 6371000f; //Planet's radius in meters
        [SerializeField] private float kmRange = 200f;
        [SerializeField] [Range(0, 100)] private int startValue = 60;
        [SerializeField] [Range(0, 100)] private int endValue = 0;
        [SerializeField] private Vector2 heatmapSize = new Vector2(2048, 1024);

        [SerializeField] private Gradient colors = new Gradient();

        private void OnEnable() {
            //TODO: Remove this for production
            json = (TextAsset)Resources.Load("magnitude_point_data", typeof(TextAsset));
        }

        private void OnWizardCreate() {
            string path = EditorUtility.SaveFilePanelInProject("Save Heatmap Texture", "Static_Heatmap", "png", "Specify where to save the heatmap.");
            if (path.Length > 0) {
                DateTime before = DateTime.Now;
                var hm = GenerateStaticHeatmap();
                DateTime after = DateTime.Now;
                TimeSpan duration = after.Subtract(before);
                Debug.Log("Heatmap generation time in seconds: " + duration.TotalSeconds);
                System.IO.File.WriteAllBytes(path, hm.EncodeToPNG());
            }
        }

        //TODO: Update this to use proper parser when rework is done
        private Texture2D GenerateStaticHeatmap() {
            /*Debug.Log(json.text);
            var points = JsonDataLoader<CoordinatePoint_Basic>.ParseJson(json);

            int[,] heatmapGrid = Heatmap.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, mPlanetRadius, kmRange, startValue, endValue, colors, points);
            return Texture2D_Extensions.DrawHeatmap(heatmapGrid, colors);*/
            return null;
        }
    }
}
