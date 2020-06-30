using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CoordinateMapper.Coordinates;
using CoordinateMapper.Extensions;

public class Heatmap : MonoBehaviour
{
    [SerializeField] private bool drawGrid;

    [SerializeField] private int range;
    [SerializeField] [Range(0, 100)] private int startValue;
    [SerializeField] [Range(0, 100)] private int endValue;
    [SerializeField] private Vector2 heatmapSize;

    [SerializeField] private Gradient colors;
    [SerializeField] private Renderer renderer;

    public void GenerateHeatMapGrid(IEnumerable<CoordinatePoint> points) {
        int[,] heatmapGrid = Heatmap.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, range, startValue, endValue, colors, points);

        for (int x = 0; x < heatmapGrid.GetLength(0); x++) {
            string line = "";
            for (int y = 0; y < heatmapGrid.GetLength(1); y++) {
                line = line + heatmapGrid[x, y] + ", ";
            }
            //Debug.Log(line);
        }

        Texture2D overlay = Texture2D_Extensions.DrawHeatmap(heatmapGrid, colors);
        renderer.material.SetTexture("_OverlayTex", overlay);
    }

    void DrawHeatMapGrid(int[,] heatmapGrid) {
        if (!drawGrid) { return; }

        float left = transform.position.x - transform.localScale.x / 2f;
        float right = transform.position.x + transform.localScale.x / 2f;
        float bot = transform.position.z - transform.localScale.y / 2f;
        float top = transform.position.z + transform.localScale.y / 2f;
        float xDist = Mathf.Abs(right - left);
        float yDist = Mathf.Abs(top - bot);

        for (int x = 0; x < heatmapGrid.GetLength(0); x++) {
            float ratio = (float)x / (float)heatmapGrid.GetLength(0);
            float lineX = xDist * ratio;

            Debug.DrawLine(new Vector3(left + lineX, transform.position.y, bot), new Vector3(left + lineX, transform.position.y, top), Color.green, 100f);
        }

        for (int y = 0; y < heatmapGrid.GetLength(1); y++) {
            float ratio = (float)y / (float)heatmapGrid.GetLength(1);
            float lineY = yDist * ratio;

            Debug.DrawLine(new Vector3(left, transform.position.y, bot + lineY), new Vector3(right, transform.position.y, bot + lineY), Color.green, 100f);
        }

        Debug.DrawLine(new Vector3(right, transform.position.y, bot), new Vector3(right, transform.position.y, top), Color.green, 100f);
        Debug.DrawLine(new Vector3(left, transform.position.y, top), new Vector3(right, transform.position.y, top), Color.green, 100f);
    }

    //TODO: Make this more efficient
    public static int[,] GenerateValues(int w, int h, int range, int startValue, int endValue, Gradient colors, IEnumerable<CoordinatePoint> points) {
        int[,] heatmapGrid = new int[w, h];

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

        return heatmapGrid;
    }
}