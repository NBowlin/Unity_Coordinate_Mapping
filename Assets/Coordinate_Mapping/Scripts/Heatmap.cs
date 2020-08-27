using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace CoordinateMapper {
    public class Heatmap : MonoBehaviour {
        [SerializeField] private float mPlanetRadius = 6371000f; //Planet's radius in meters
        [SerializeField] private float kmRange = 200f; //Range of effect for each point in kilometers
        [SerializeField] [Range(0, 100)] private int startValue = 60;
        [SerializeField] [Range(0, 100)] private int endValue = 0;
        [SerializeField] private Vector2 heatmapSize = new Vector2(2048, 1024);

        [SerializeField] private Gradient colors = null;
        [SerializeField] private Renderer hmRenderer = null;

        public async void GenerateHeatmapGrid(IEnumerable<ICoordinatePoint> points) {
            var grid = await Task.Run(() => HeatmapGenerator.GenerateValues((int)heatmapSize.x, (int)heatmapSize.y, mPlanetRadius, kmRange, startValue, endValue, points));
            var colorBytes = await Task.Run(() => HeatmapGenerator.CreateColorMap(grid, colors));
            var overlay = HeatmapGenerator.CreateHeatmapTexture((int)heatmapSize.x, (int)heatmapSize.y, colorBytes);
            hmRenderer.material.SetTexture("_OverlayTex", overlay);
        }
    }
}