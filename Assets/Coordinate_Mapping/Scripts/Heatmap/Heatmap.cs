using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace CoordinateMapper {
    public class Heatmap : MonoBehaviour {
        [SerializeField] private bool drawGrid = false; //TODO: remove before submission

        [SerializeField] private float mPlanetRadius = 6371000f; //Planet's radius in meters
        [SerializeField] private float kmRange = 200f; //Range of effect for each point in kilometers
        [SerializeField] [Range(0, 100)] private int startValue = 60;
        [SerializeField] [Range(0, 100)] private int endValue = 0;
        [SerializeField] private Vector2 heatmapSize = new Vector2(2048, 1024);

        [SerializeField] private Gradient colors = null;
        [SerializeField] private Renderer hmRenderer = null;

        public async void GenerateHeatmapGrid(IEnumerable<ICoordinatePoint> points) {
            //var overlay = await HeatmapGenerator.GenerateHeatmapAsync(heatmapSize, mPlanetRadius, kmRange, startValue, endValue, colors, points);
            //var overlay = HeatmapGenerator.GenerateHeatmap(heatmapSize, mPlanetRadius, kmRange, startValue, endValue, colors, points);
            Debug.Log("Generate heatmap!");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var grid = await Task.Run(() => HeatmapGenerator.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, mPlanetRadius, kmRange, startValue, endValue, points));
            var colorBytes = await Task.Run(() => HeatmapGenerator.CreateColorMap(grid, colors));
            var overlay = HeatmapGenerator.CreateHeatmapTexture((int)heatmapSize.x, (int)heatmapSize.y, colorBytes);
            hmRenderer.material.SetTexture("_OverlayTex", overlay);
            sw.Stop();
            Debug.Log("Heatmap generation time: " + sw.ElapsedMilliseconds / 1000f);

            DrawHeatmapGrid(grid);
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
    }
}