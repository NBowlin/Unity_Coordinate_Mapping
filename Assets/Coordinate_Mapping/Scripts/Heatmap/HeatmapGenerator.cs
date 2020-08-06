using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;
using System.Threading.Tasks;

namespace CoordinateMapper {
    public static class HeatmapGenerator {

        public static Texture2D GenerateHeatmap(Vector2 heatmapSize, float mPlanetRadius, float kmRange, int startValue, int endValue, Gradient colors, IEnumerable<ICoordinatePoint> points) {
            var heatmapGrid = GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, mPlanetRadius, kmRange, startValue, endValue, points);
            var colorBytes = CreateColorMap(heatmapGrid, colors);

            return CreateHeatmapTexture(heatmapGrid.GetLength(0), heatmapGrid.GetLength(1), colorBytes);
        }

        public async static Task<Texture2D> GenerateHeatmapAsync(Vector2 heatmapSize, float mPlanetRadius, float kmRange, int startValue, int endValue, Gradient colors, IEnumerable<ICoordinatePoint> points) {
            var heatmapGrid = await Task.Run(() => GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, mPlanetRadius, kmRange, startValue, endValue, points));
            var colorBytes = await Task.Run(() => CreateColorMap(heatmapGrid, colors));

            return CreateHeatmapTexture(heatmapGrid.GetLength(0), heatmapGrid.GetLength(1), colorBytes);
        }

        //NOTE: Texture operations must be done on the main thread - so don't call this asynchronously
        public static Texture2D CreateHeatmapTexture(int width, int height, byte[] colorBytes) {
            Texture2D heatmap = new Texture2D(width, height, TextureFormat.RGBA32, false);
            heatmap.LoadRawTextureData(colorBytes);
            heatmap.Apply();
            return heatmap;
        }

        //TODO: Make this more efficient
        public static int[,] GenerateValues(int w, int h, float radius, float range, int startValue, int endValue, IEnumerable<ICoordinatePoint> points) {
            int[,] heatmapGrid = new int[w, h];

            //Latitude lines are constant distance - so pre-calculate the km between latitudes and use that to restrict
            //the y coords as we check location distance below
            float kmPerLat = Mathf.PI * radius / 180.0f / 1000f; //km per 1 degree of latitude (Earth is ~111km)
            float degPerLat = 180f / h;
            float cellHeightKm = kmPerLat * degPerLat;
            int cellRangeY = Mathf.RoundToInt(range / cellHeightKm);

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
                        float d = p.location.kmBetweenLocations(lat, lng, radius);

                        if (d < range) {
                            float dRatio = d / range;
                            int fallOffRange = startValue - endValue;
                            int fallOffVal = (int)(startValue - (fallOffRange * dRatio));
                            heatmapGrid[currX, y] += fallOffVal;
                        }
                    }
                }
            }

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

        public static byte[] CreateColorMap(int[,] heatmapValues, Gradient colors) {
            int w = heatmapValues.GetLength(0);
            int h = heatmapValues.GetLength(1);

            var texColors = new Color32[w * h];
            for (int i = 0; i < texColors.Length; i++) { texColors[i] = Color.clear; }

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    if (heatmapValues[x, y] > 0) {
                        texColors[y * w + x] = colors.Evaluate(heatmapValues[x, y] / 100f);
                    }
                }
            }

            return ColorsToBytes(texColors);
        }

        public static byte[] ColorsToBytes(Color32[] colors) {
            var byteColors = new byte[colors.Length * 4];
            for (int i = 0; i < colors.Length; i++) {
                Color32 c = colors[i];
                byteColors[i * 4] = c.r;
                byteColors[i * 4 + 1] = c.g;
                byteColors[i * 4 + 2] = c.b;
                byteColors[i * 4 + 3] = c.a;
            }
            return byteColors;
        }
    }
}
