using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using CoordinateMapper.Coordinates;
using CoordinateMapper.Data;

public class StaticHeatmap : ScriptableWizard
{
    [MenuItem("HeatMap/Generate Heat Map Texture")]
    static void CreateStaticHeatmap() {

        DisplayWizard<StaticHeatmap>("Create a heat map");
    }

    [SerializeField] private int range = 6;
    [SerializeField] [Range(0, 100)] private int startValue = 60;
    [SerializeField] [Range(0, 100)] private int endValue = 0;
    [SerializeField] private Vector2 heatmapSize = new Vector2(2048, 1024);

    [SerializeField] private Gradient colors;

    private void OnWizardCreate() {
        string path = EditorUtility.SaveFilePanelInProject("Save Heatmap Texture", "Heatmap", "png", "Specify where to save the heatmap.");
        if (path.Length > 0) {
            var hm = GenerateStaticHeatMap();
            System.IO.File.WriteAllBytes(path, hm.EncodeToPNG());
        }
    }

    private Texture2D GenerateStaticHeatMap() {
        int w = (int)heatmapSize.x;
        int h = (int)heatmapSize.y;

        int[,] heatmapGrid = new int[w, h];

        TextAsset json = (TextAsset)Resources.Load("magnitude_point_data", typeof(TextAsset));
        Debug.Log(json.text);
        var points = JsonDataLoader<CoordinatePoint_Basic>.ParseJson(json);

        foreach (CoordinatePoint p in points) {
            float texLat = 90f + p.location.latitude;
            float texLng = 180f + p.location.longitude;

            float latRatio = texLat / 180f;
            float lngRatio = texLng / 360f;

            float xStart = Mathf.Round(lngRatio * w);
            float yStart = Mathf.Round(latRatio * h);

            xStart = Mathf.Clamp(xStart, 0f, w - 1);
            yStart = Mathf.Clamp(yStart, 0f, h - 1);

            //Square Pattern - Not fully updated
            /*for (int x = (int)xStart - range; x <= xStart + range; x++) {
                if (x < 0 || x >= w) { continue; }
                for (int y = (int)yStart - range; y <= yStart + range; y++) {
                    if (y < 0 || y >= h) { continue; }
                    heatmapGrid[x, y] += 20;
                }
            }*/


            //Diamond Pattern
            /*for(int x = 0; x < range; x++) {
                if (x + xStart >= w) { continue; }
                for (int y = 0; y < range - x; y++) {
                    if (y + yStart >= h) { continue; }
                    int fallOff = Mathf.Max(x, y);
                    float fallOffPercent = (float)fallOff / (float)range;
                    int fallOffRange = startValue - endValue;
                    int fallOffVal = (int)(startValue - (fallOffRange * fallOffPercent));
                    heatmapGrid[(int)xStart + x, (int)yStart + y] += fallOffVal;

                    if(x != 0 && xStart - x > 0) { heatmapGrid[(int)xStart - x, (int)yStart + y] += fallOffVal; }
                    if(y != 0 && yStart - y > 0) {
                        heatmapGrid[(int)xStart + x, (int)yStart - y] += fallOffVal;
                        if (x != 0 && xStart - x > 0) { heatmapGrid[(int)xStart - x, (int)yStart - y] += fallOffVal; }
                    }
                }
            }*/

            //Circle Pattern
            float rSquared = range * range;
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    float radVal = (xStart - x) * (xStart - x) + (yStart - y) * (yStart - y);
                    if (radVal < rSquared) {
                        int fallOff = Mathf.Max(x, y);
                        float fallOffPercent = radVal / rSquared;
                        int fallOffRange = startValue - endValue;
                        int fallOffVal = (int)(startValue - (fallOffRange * fallOffPercent));

                        heatmapGrid[x, y] += fallOffVal;

                    }
                }
            }
        }

        return DrawHeatmapTexture(heatmapGrid);
    }

    private Texture2D DrawHeatmapTexture(int[,] heatmap) {
        int w = heatmap.GetLength(0);
        int h = heatmap.GetLength(1);

        Texture2D overlay = new Texture2D(w, h);

        Color[] clearColors = new Color[w * h];
        for (int i = 0; i < clearColors.Length; i++) { clearColors[i] = Color.clear; }
        overlay.SetPixels(clearColors);

        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                if (heatmap[x, y] > 0) {
                    //Color using gradient
                    var c = colors.Evaluate(heatmap[x, y] / 100f);
                    overlay.SetPixel(x, y, c);

                    //Red color using alpha
                    //overlay.SetPixel(x, y, new Color(1f, 0f, 0f, heatmap[x, y] / 100f));
                }
            }
        }

        overlay.Apply();
        return overlay;
    }
}
