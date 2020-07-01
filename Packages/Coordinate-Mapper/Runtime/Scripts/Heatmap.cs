using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CoordinateMapper.Coordinates;
using CoordinateMapper.Extensions;

public class Heatmap : MonoBehaviour
{
    [SerializeField] private bool drawGrid;


    //TODO: Remove old drawing method once done comparing
    [SerializeField] private bool useOldDrawMethod;
    [SerializeField] private int rangeOld;


    [SerializeField] private int range;
    [SerializeField] [Range(0, 100)] private int startValue;
    [SerializeField] [Range(0, 100)] private int endValue;
    [SerializeField] private Vector2 heatmapSize;

    [SerializeField] private Gradient colors;
    [SerializeField] private Renderer hmRenderer;

    public void GenerateHeatMapGrid(IEnumerable<CoordinatePoint> points) {

        /*var p = new CoordinatePoint_Basic();
        var l = new Location();
        l.latitude = 90f;
        l.longitude = 0f;
        l.name = "North Pole";
        p.location = l;
        points = new List<CoordinatePoint>() { p };*/

        int[,] heatmapGrid;

        if (useOldDrawMethod) {
            System.Diagnostics.Stopwatch sw1 = System.Diagnostics.Stopwatch.StartNew();
            heatmapGrid = Heatmap.GenerateValuesOld((int)heatmapSize.x, (int)heatmapSize.y, rangeOld, startValue, endValue, colors, points);
            sw1.Stop();
            Debug.Log("Old generate time: " + (float)sw1.Elapsed.Milliseconds / 1000f);
        } else {
            System.Diagnostics.Stopwatch sw2 = System.Diagnostics.Stopwatch.StartNew();
            heatmapGrid = Heatmap.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, range, startValue, endValue, colors, points);
            sw2.Stop();
            Debug.Log("New generate time: " + (float)sw2.Elapsed.Milliseconds / 1000f);
        }

        DrawHeatMapGrid(heatmapGrid);

        Texture2D overlay = Texture2D_Extensions.DrawHeatmap(heatmapGrid, colors);
        hmRenderer.material.SetTexture("_OverlayTex", overlay);
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

        //float eC = 40075.017f;
        //float kmPerLng = eC / 360f;
        //float degPerLng = 360f / w;
        //float cellWidthKm = kmPerLng * degPerLng;
        //int widthCells = Mathf.RoundToInt(range / cellWidthKm);

        float pC = 39940.653f;
        float kmPerLat = pC / 180f;
        float degPerLat = 180f / h;
        float cellHeightKm = kmPerLat * degPerLat;
        int cellRangeY = Mathf.RoundToInt(range / cellHeightKm) + 4;

        foreach (CoordinatePoint p in points) {
            float texLat = 90f + p.location.latitude;
            float texLng = 180f + p.location.longitude;

            float latRatio = texLat / 180f;
            float lngRatio = texLng / 360f;

            float xStart = Mathf.Round(lngRatio * w);
            float yStart = Mathf.Round(latRatio * h);

            xStart = Mathf.Clamp(xStart, 0f, w - 1);
            yStart = Mathf.Clamp(yStart, 0f, h - 1);

            for(int x = 0; x < w; x++) {
                if (x < 0 || x >= w) { continue; }
                for (int y = (int)yStart - cellRangeY; y <= yStart + cellRangeY; y++) {
                    if (y < 0 || y >= h) { continue; }

                    float lng = (float)x / (float)w * 360f - 180f;
                    float lat = (float)y / (float)h * 180f - 90f;
                    float d = p.location.kmBetweenLocations(lat, lng);
                    if (d < range) {
                        float dRatio = d / (float)range;
                        int fallOffRange = startValue - endValue;
                        int fallOffVal = (int)(startValue - (fallOffRange * dRatio));
                        heatmapGrid[x, y] += fallOffVal;
                    }
                }
            }
        }

        //TODO: Check vs lat/lng of center grid space for x/y (currently bot left of grid space)
        //Or maybe even find min and max lat/lng and check if distance threshold is inside that?
        /*for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                float lng = (float)x / (float)w * 360f - 180f;
                float lat = (float)y / (float)h * 180f - 90f;

                foreach (CoordinatePoint p in points) {
                    float d = p.location.kmBetweenLocations(lat, lng);
                    if (d < range) {
                        float dRatio = d / (float)range;
                        int fallOffRange = startValue - endValue;
                        int fallOffVal = (int)(startValue - (fallOffRange * dRatio));
                        heatmapGrid[x, y] += fallOffVal;
                    }
                }
            }
        }*/

        return heatmapGrid;
    }

    public static int[,] GenerateValuesOld(int w, int h, int range, int startValue, int endValue, Gradient colors, IEnumerable<CoordinatePoint> points) {
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

            //Debug.Log(p.location.name + " X/Y Value is: " + xStart + "/" + yStart);

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