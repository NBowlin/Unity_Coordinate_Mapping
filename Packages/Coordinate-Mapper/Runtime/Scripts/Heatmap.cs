using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CoordinateMapper.Coordinates;
using CoordinateMapper.Extensions;

public class Heatmap : MonoBehaviour
{
    [SerializeField] private bool drawGrid;

    [SerializeField] private int kmRange; //Range of effect for each point in kilometers
    [SerializeField] [Range(0, 100)] private int startValue;
    [SerializeField] [Range(0, 100)] private int endValue;
    [SerializeField] private Vector2 heatmapSize;

    [SerializeField] private Gradient colors;
    [SerializeField] private Renderer hmRenderer;

    private IEnumerable<CoordinatePoint> points;

    public void GenerateHeatmapGrid(IEnumerable<CoordinatePoint> points) {

        /*var p = new CoordinatePoint_Basic();
        var l = new Location();
        l.latitude = 90f;
        l.longitude = 0f;
        l.name = "North Pole";
        p.location = l;
        points = new List<CoordinatePoint>() { p };*/

        

        //TODO: Do this elsewhere?
        LookupTable.GenerateTables();
        this.points = points;
        Invoke("TestProfile", 2f);
    }

    void TestProfile() {
        int[,] heatmapGrid;

        var sw = System.Diagnostics.Stopwatch.StartNew();

        UnityEngine.Profiling.Profiler.BeginSample("HeatmapGenerate");
        heatmapGrid = Heatmap.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, kmRange, startValue, endValue, colors, points);
        UnityEngine.Profiling.Profiler.EndSample();

        sw.Stop();
        Debug.Log("Heatmap generation time: " + sw.ElapsedMilliseconds / 1000f);
        //sw.Reset();

        DrawHeatmapGrid(heatmapGrid);

        //sw.Start();
        UnityEngine.Profiling.Profiler.BeginSample("HeatmapDraw");
        Texture2D overlay = Texture2D_Extensions.DrawHeatmap(heatmapGrid, colors);
        hmRenderer.material.SetTexture("_OverlayTex", overlay);
        UnityEngine.Profiling.Profiler.EndSample();
        //sw.Stop();
        //Debug.Log("Heatmap draw time: " + sw.ElapsedMilliseconds / 1000f);
    }

    void DrawHeatmapGrid(int[,] heatmapGrid) {
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

        //TODO: Check vs lat/lng of center grid space for x/y (currently bot left of grid space)
        //Or maybe even find min and max lat/lng and check if distance threshold is inside that?

        //float eC = 40075.017f;
        //float kmPerLng = eC / 360f;
        //float degPerLng = 360f / w;
        //float cellWidthKm = kmPerLng * degPerLng;
        //int widthCells = Mathf.RoundToInt(range / cellWidthKm);

        //Latitude lines are constant distance - so pre-calculate the km between latitudes and use that to restrict
        //the y coords as we check location distance below
        float kmPerLat = 111f; //111km per 1 degree of latitude
        float degPerLat = 180f / h;
        float cellHeightKm = kmPerLat * degPerLat;
        int cellRangeY = Mathf.RoundToInt(range / cellHeightKm);

        //var sw = new System.Diagnostics.Stopwatch();

        foreach (CoordinatePoint p in points) {
            float texLat = 90f + p.location.latitude;
            float texLng = 180f + p.location.longitude;

            float latRatio = texLat / 180f;
            float lngRatio = texLng / 360f;

            float xCenter = Mathf.Round(lngRatio * w);
            float yCenter = Mathf.Round(latRatio * h);

            xCenter = (int)Mathf.Clamp(xCenter, 0f, w - 1);
            yCenter = (int)Mathf.Clamp(yCenter, 0f, h - 1);

            int cellRangeX = w;
            float yLat = (float)yCenter / (float)h * 180f - 90f;

            int gridX = 1;
            var movingEast = xCenter <= w / 2; //If in western hemisphere - move east so we have more space to check range
            while(true) {
                var currX = xCenter + (movingEast ? gridX : -gridX);
                if(currX > w || currX < 0) { break; }

                float lng = currX / (float)w * 360f - 180f;
                float d = p.location.kmBetweenLocations(yLat, lng);

                if(d > range) {
                    cellRangeX = gridX - 1;
                    break;
                }

                gridX += 1;
            }

            //for (int x = 0; x < w; x++) {
            for (int x = (int)xCenter - cellRangeX; x <= xCenter + cellRangeX; x++) {
                var currX = x;
                if (x < 0 || x >= w) {
                    if(cellRangeX < w / 2) { currX = nfmod(x, w); }
                    else { continue; }
                }

                for (int y = (int)yCenter - cellRangeY; y <= yCenter + cellRangeY; y++) {
                    if (y < 0 || y >= h) { continue; }

                    float lng = (float)currX / (float)w * 360f - 180f;
                    float lat = (float)y / (float)h * 180f - 90f;
                    //sw.Start();
                    float d = p.location.kmBetweenLocations(lat, lng);
                    //sw.Stop();

                    if (d < range) {
                        float dRatio = d / (float)range;
                        int fallOffRange = startValue - endValue;
                        int fallOffVal = (int)(startValue - (fallOffRange * dRatio));
                        heatmapGrid[currX, y] += fallOffVal;
                    }
                }
            }
        }

        //Debug.Log("Heatmap trig time: " + sw.ElapsedMilliseconds / 1000f + " | ACos time: " + Location.sw.ElapsedMilliseconds / 1000f + " | Sin Lookup: " + LookupTable.sinSw.ElapsedMilliseconds / 1000f + " | Cos Lookup: " + LookupTable.cosSw.ElapsedMilliseconds / 1000f
        //     + " | Abs time: " + LookupTable.absSw.ElapsedMilliseconds / 1000f);

        return heatmapGrid;
    }

    public static int nfmod(int a, int b) {
        return a - b * Mathf.FloorToInt((float)a / (float)b);
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