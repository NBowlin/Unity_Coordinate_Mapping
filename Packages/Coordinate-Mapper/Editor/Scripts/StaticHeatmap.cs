using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using CoordinateMapper.Coordinates;
using CoordinateMapper.Data;
using CoordinateMapper.Extensions;
using System;

public class StaticHeatmap : ScriptableWizard
{
    [MenuItem("HeatMap/Generate Heat Map Texture")]
    static void CreateStaticHeatmap() {

        DisplayWizard<StaticHeatmap>("Create a heat map");
    }

    [SerializeField] private TextAsset json;

    [SerializeField] private int range = 6;
    [SerializeField] [Range(0, 100)] private int startValue = 60;
    [SerializeField] [Range(0, 100)] private int endValue = 0;
    [SerializeField] private Vector2 heatmapSize = new Vector2(2048, 1024);

    [SerializeField] private Gradient colors = new Gradient();

    private void OnEnable() {
        json = (TextAsset)Resources.Load("magnitude_point_data", typeof(TextAsset));
    }

    private void OnWizardCreate() {
        string path = EditorUtility.SaveFilePanelInProject("Save Heatmap Texture", "Heatmap", "png", "Specify where to save the heatmap.");
        if (path.Length > 0) {
            DateTime before = DateTime.Now;
            var hm = GenerateStaticHeatMap();
            DateTime after = DateTime.Now;
            TimeSpan duration = after.Subtract(before);
            Debug.Log("Heatmap generation time in seconds: " + duration.Seconds);
            System.IO.File.WriteAllBytes(path, hm.EncodeToPNG());
        }
    }

    private Texture2D GenerateStaticHeatMap() {
        Debug.Log(json.text);
        var points = JsonDataLoader<CoordinatePoint_Basic>.ParseJson(json);

        int[,] heatmapGrid = Heatmap.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, range, startValue, endValue, colors, points);
        return Texture2D_Extensions.DrawHeatmap(heatmapGrid, colors);
        //return DrawHeatmapTexture(heatmapGrid);
    }

    //Draw texture using SetPixel
    //private Texture2D DrawHeatmapTexture(int[,] heatmap) {
    //    DateTime before = DateTime.Now;
    //    int w = heatmap.GetLength(0);
    //    int h = heatmap.GetLength(1);

    //    Texture2D overlay = new Texture2D(w, h);

    //    Color[] clearColors = new Color[w * h];
    //    for (int i = 0; i < clearColors.Length; i++) { clearColors[i] = Color.clear; }
    //    overlay.SetPixels(clearColors);

    //    for (int x = 0; x < w; x++) {
    //        for (int y = 0; y < h; y++) {
    //            if (heatmap[x, y] > 0) {
    //                //Color using gradient
    //                var c = colors.Evaluate(heatmap[x, y] / 100f);
    //                overlay.SetPixel(x, y, c);

    //                //Red color using alpha
    //                //overlay.SetPixel(x, y, new Color(1f, 0f, 0f, heatmap[x, y] / 100f));
    //            }
    //        }
    //    }

    //    overlay.Apply();

    //    DateTime after = DateTime.Now;
    //    TimeSpan duration = after.Subtract(before);
    //    Debug.Log("Draw time in seconds: " + duration.Seconds);

    //    return overlay;
    //}
}
