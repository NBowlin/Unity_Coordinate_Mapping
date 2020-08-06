using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace CoordinateMapper {
    public class Heatmap : MonoBehaviour {
        [SerializeField] private bool drawGrid; //TODO: remove before submission

        [SerializeField] private float mPlanetRadius = 6371000f; //Planet's radius in meters
        [SerializeField] private float kmRange = 200f; //Range of effect for each point in kilometers
        [SerializeField] [Range(0, 100)] private int startValue = 60;
        [SerializeField] [Range(0, 100)] private int endValue = 0;
        [SerializeField] private Vector2 heatmapSize = new Vector2(2048, 1024);

        [SerializeField] private Gradient colors;
        [SerializeField] private Renderer hmRenderer;

        //TODO: Updated the class to use ICoordinatePoint - check back after rework
        private IEnumerable<ICoordinatePoint> points;

        public async void GenerateHeatmapGrid(IEnumerable<ICoordinatePoint> points) {
            this.points = points;
            //Invoke("TestProfile", 2.0f);

            int[,] heatmapGrid;
            heatmapGrid = await Task.Run(() => Heatmap.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, mPlanetRadius, kmRange, startValue, endValue, colors, points));

            DrawHeatmapGrid(heatmapGrid);

            var colorBytes = await Task.Run(() => Texture2D_Extensions.CreateColorMap(heatmapGrid, colors));

            Texture2D overlay = new Texture2D(heatmapGrid.GetLength(0), heatmapGrid.GetLength(1), TextureFormat.RGBA32, false);
            overlay.LoadRawTextureData(colorBytes);
            overlay.Apply();

            hmRenderer.material.SetTexture("_OverlayTex", overlay);

            /*int[,] heatmapGrid;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            UnityEngine.Profiling.Profiler.BeginSample("HeatmapGenerate");
            heatmapGrid = await Heatmap.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, mPlanetRadius, kmRange, startValue, endValue, colors, points);
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
            //Debug.Log("Heatmap draw time: " + sw.ElapsedMilliseconds / 1000f);*/
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
        public static int[,] GenerateValues(int w, int h, float radius, float range, int startValue, int endValue, Gradient colors, IEnumerable<ICoordinatePoint> points) {
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
            float kmPerLat = Mathf.PI * radius / 180.0f / 1000f; //km per 1 degree of latitude (Earth is ~111km)
            float degPerLat = 180f / h;
            float cellHeightKm = kmPerLat * degPerLat;
            int cellRangeY = Mathf.RoundToInt(range / cellHeightKm);

            //var sw = new System.Diagnostics.Stopwatch();

            foreach (ICoordinatePoint p in points) {
                float texLat = 90f + p.location.latitude;
                float texLng = 180f + p.location.longitude;

                float latRatio = texLat / 180f;
                float lngRatio = texLng / 360f;

                float xCenter = Mathf.Round(lngRatio * w);
                float yCenter = Mathf.Round(latRatio * h);

                xCenter = (int)Mathf.Clamp(xCenter, 0f, w - 1);
                yCenter = (int)Mathf.Clamp(yCenter, 0f, h - 1);

                //Calculate longitudinal range for given point: 1° Longitude = Cos(lat) * length of 1° (km) at equator
                float yLat = (float)yCenter / (float)h * 180f - 90f;
                float kmPerLng = Mathf.Cos(yLat * Mathf.Deg2Rad) * kmPerLat;
                float degPerLng = 360f / w;
                float cellWidthKm = kmPerLng * degPerLng;
                int cellRangeX = Mathf.RoundToInt(range / cellWidthKm);

                //The idea was to only do distance calculations for 1/4th of the circle, then just mirror those values around the grid
                //However, because bot-left lat/lng of circle is farther from center than, say, bot-right, it doesn't quite work
                //Could adjust where I'm checking lat/lng, but moving on for now
                /*heatmapGrid[(int)xCenter, (int)yCenter] += startValue;
                int[,] currValues = new int[cellRangeX + 1, cellRangeY + 1];
                //Bottom left quadrant
                for (int x = (int)xCenter - cellRangeX; x <= xCenter; x++) {
                    var currX = x;
                    if (x < 0 || x >= w) {
                        if (cellRangeX < w / 2) { currX = Modulo(x, w); }
                        else { continue; }
                    }

                    for (int y = (int)yCenter - cellRangeY; y <= yCenter; y++) {
                        if (y < 0 || y >= h || (currX == (int)xCenter && y == (int)yCenter)) { continue; }

                        float lng = CartesianToSphericalLongitude((float)currX, (float)w);
                        float lat = CartesianToSphericalLatitude((float)y, (float)h);
                        float d = p.location.kmBetweenLocations(lat, lng);

                        if (d < range) {
                            float dRatio = d / (float)range;
                            int fallOffRange = startValue - endValue;
                            int fallOffVal = (int)(startValue - (fallOffRange * dRatio));
                            heatmapGrid[currX, y] += fallOffVal;

                            currValues[(int)xCenter - x, (int)yCenter - y] = fallOffVal;
                        }
                    }
                }

                //Bottom right quadrant
                for (int x = (int)xCenter + 1; x <= xCenter + cellRangeX; x++) {
                    var currX = x;
                    if (x < 0 || x >= w) {
                        if (cellRangeX < w / 2) { currX = Modulo(x, w); }
                        else { continue; }
                    }

                    for (int y = (int)yCenter - cellRangeY; y < yCenter; y++) {
                        if (y < 0 || y >= h || (currX == (int)xCenter && y == (int)yCenter)) { continue; }
                        heatmapGrid[currX, y] += currValues[x - (int)xCenter, (int)yCenter - y];
                    }
                }

                //Top left quadrant
                for (int x = (int)xCenter - cellRangeX; x < xCenter; x++) {
                    var currX = x;
                    if (x < 0 || x >= w) {
                        if (cellRangeX < w / 2) { currX = Modulo(x, w); }
                        else { continue; }
                    }

                    for (int y = (int)yCenter + 1; y <= yCenter + cellRangeY; y++) {
                        if (y < 0 || y >= h || (currX == (int)xCenter && y == (int)yCenter)) { continue; }
                    }
                }

                //Top right quadrant
                for (int x = (int)xCenter; x <= xCenter + cellRangeX; x++) {
                    var currX = x;
                    if (x < 0 || x >= w) {
                        if (cellRangeX < w / 2) { currX = Modulo(x, w); }
                        else { continue; }
                    }

                    for (int y = (int)yCenter; y <= yCenter + cellRangeY; y++) {
                        if (y < 0 || y >= h || (currX == (int)xCenter && y == (int)yCenter)) { continue; }
                    }
                }*/



                for (int x = (int)xCenter - cellRangeX; x <= xCenter + cellRangeX; x++) {
                    var currX = x;
                    if (x < 0 || x >= w) {
                        if (cellRangeX < w / 2) { currX = Modulo(x, w); }
                        else { continue; }
                    }

                    for (int y = (int)yCenter - cellRangeY; y <= yCenter + cellRangeY; y++) {
                        if (y < 0 || y >= h) { continue; }

                        float lng = CartesianToSphericalLongitude((float)currX, (float)w);
                        float lat = CartesianToSphericalLatitude((float)y, (float)h);
                        //sw.Start();
                        float d = p.location.kmBetweenLocations(lat, lng, radius);
                        //sw.Stop();

                        if (d < range) {
                            float dRatio = d / range;
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

        public static (float latitude, float longitude) CartesionToSphericalCoords(float x, float y, float gridWidth, float gridHeight) {
            float lng = CartesianToSphericalLongitude(x, gridWidth);
            float lat = CartesianToSphericalLatitude(y, gridHeight);
            return (lat, lng);
        }

        public static float CartesianToSphericalLatitude(float y, float gridHeight) {
            //var centerY = y + (y >= gridHeight / 2  ? 0.5f :-0.5f); //Test vs the center of the grid point, rather than the origin
            return y / gridHeight * 180f - 90f;
        }

        public static float CartesianToSphericalLongitude(float x, float gridWidth) {
            //var centerX = x + (x >= gridWidth / 2 ? 0.5f : -0.5f); //Test vs the center of the grid point, rather than the origin
            return x / gridWidth * 360f - 180f;
        }

        //Apparently C# % operator is actually just remainder. So negative numbers don't mod properly, below is proper modulo
        public static int Modulo(int a, int b) {
            return a - b * Mathf.FloorToInt((float)a / (float)b);
        }
    }
}