using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CoordinateMapper.Coordinates;

public class Heatmap : MonoBehaviour
{
    [SerializeField] private bool drawGrid;

    [SerializeField] private int range;
    [SerializeField] [Range(0, 100)] private int startValue;
    [SerializeField] [Range(0, 100)] private int endValue;

    [SerializeField] private Gradient colors;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void DoStuff(IEnumerable<CoordinatePoint> points) {
        /*var p = new CoordinatePoint_Basic();
        var l = new Location();
        l.latitude = 40.7128f;
        l.longitude = -74.0060f;
        p.location = l;
        points = new List<CoordinatePoint>() { p };*/

        GenerateHeatMapGrid(points);
    }

    void GenerateHeatMapGrid(IEnumerable<CoordinatePoint> points) {
        Material mat = GetComponent<Renderer>().material;
        Texture2D overlay = new Texture2D(mat.mainTexture.width, mat.mainTexture.height); //mat.GetTexture("_OverlayTex") as Texture2D;
        int w = 1600;
        int h = 800;

        int[,] heatmapGrid = new int[w, h];
        DrawHeatMapGrid(heatmapGrid);

        foreach (CoordinatePoint p in points) {
            float texLat = 90f + p.location.latitude;
            float texLng = 180f + p.location.longitude;

            float latRatio = texLat / 180f;
            float lngRatio = texLng / 360f;

            float xStart = Mathf.Round(lngRatio * w);
            float yStart = Mathf.Round(latRatio * h);

            xStart = Mathf.Clamp(xStart, 0f, w - 1);
            yStart = Mathf.Clamp(yStart, 0f, h - 1);

            //Square Pattern
            /*for (int x = (int)xStart - range; x <= xStart + range; x++) {
                if (x < 0 || x >= w) { continue; }
                for (int y = (int)yStart - range; y <= yStart + range; y++) {
                    if (y < 0 || y >= h) { continue; }
                    heatmapGrid[x, y] += 20;
                }
            }*/


            //Diamond Pattern
            for(int x = 0; x < range; x++) {
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
            }
        }

        for (int x = 0; x < heatmapGrid.GetLength(0); x++) {
            string line = "";
            for (int y = 0; y < heatmapGrid.GetLength(1); y++) {
                line = line + heatmapGrid[x, y] + ", ";
            }
            Debug.Log(line);
        }

        DrawHeatmapTexture(heatmapGrid);
    }

    void DrawHeatmapTexture(int[,] heatmap) {
        Material mat = GetComponent<Renderer>().material;
        Texture2D overlay = new Texture2D(heatmap.GetLength(0), heatmap.GetLength(1)); //mat.GetTexture("_OverlayTex") as Texture2D;
        int w = heatmap.GetLength(0);
        int h = heatmap.GetLength(1);

        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                overlay.SetPixel(x, y, Color.clear);
            }
        }

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
        mat.SetTexture("_OverlayTex", overlay);
    }

    void DrawHeatMapGrid(int[,] heatmapGrid) {
        if (!drawGrid) { return; }

        for (int x = 0; x < heatmapGrid.GetLength(0); x++) {
            for (int y = 0; y < heatmapGrid.GetLength(1); y++) {
                float ratioX = (float)x / (float)heatmapGrid.GetLength(0);
                float endRatioX = (float)(x + 1) / (float)heatmapGrid.GetLength(0);

                float ratioY = (float)y / (float)heatmapGrid.GetLength(1);
                float endRatioY = (float)(y + 1) / (float)heatmapGrid.GetLength(1);

                Vector2 botLeft = new Vector2(transform.position.x - transform.localScale.x / 2f, transform.position.z - transform.localScale.y / 2f);

                Debug.DrawLine(new Vector3(botLeft.x + ratioX * transform.localScale.x, transform.position.y, botLeft.y + ratioY * transform.localScale.y),
                    new Vector3(botLeft.x + endRatioX * transform.localScale.x, transform.position.y, botLeft.y + ratioY * transform.localScale.y), Color.green, 100f);

                Debug.DrawLine(new Vector3(botLeft.x + ratioX * transform.localScale.x, transform.position.y, botLeft.y + ratioY * transform.localScale.y),
                    new Vector3(botLeft.x + ratioX * transform.localScale.x, transform.position.y, botLeft.y + endRatioY * transform.localScale.y), Color.green, 100f);
            }
        }
    }
}

