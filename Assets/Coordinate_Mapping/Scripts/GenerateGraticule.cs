using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CoordinateMapper {
    public class GenerateGraticule : MonoBehaviour {
        [SerializeField] private Material lineMat = null;

        [SerializeField] private int segments = 0;
        [SerializeField] private int degreesBetweenLines = 0;

        [SerializeField] private bool drawParallels = true;
        [SerializeField] private bool drawMeridians = true;

        // Start is called before the first frame update
        void Start() {
            DrawUsingMeshTopologyLines();
        }

        void DrawUsingMeshTopologyLines() {
            var graticuleContainer = new GameObject("Graticule");
            graticuleContainer.transform.SetParent(transform, true);
            graticuleContainer.transform.localScale = new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z);

            if (drawParallels) {
                var parallelsPoints = DeterminePoints(true);
                var parallels = DrawMeshLines(parallelsPoints);
                parallels.name = "Parallels";
                parallels.transform.SetParent(graticuleContainer.transform, false);
            }

            if (drawMeridians) {
                var meridiansPoints = DeterminePoints(false);
                var meridians = DrawMeshLines(meridiansPoints);
                meridians.name = "Meridians";
                meridians.transform.SetParent(graticuleContainer.transform, false);
            }
        }

        List<List<Vector3>> DeterminePoints(bool plottingParallels) {

            int segmentIncrement = 360 / segments;
            var points = new List<List<Vector3>>();

            for (int i = -90; i <= 90; i += degreesBetweenLines) {
                var currPoints = new List<Vector3>();
                for (int j = 0; j < 360; j += segmentIncrement) {

                    var line = PlanetUtility.VectorFromLatLng(plottingParallels ? i : j, plottingParallels ? j : i, Vector3.right);

                    var hitInfo = PlanetUtility.LineFromOriginToSurface(transform, line, LayerMask.GetMask("Planet"));
                    if (hitInfo.HasValue) { currPoints.Add(hitInfo.Value.point); }
                }

                if (currPoints.Count > 1) { points.Add(currPoints); }
            }

            return points;
        }

        GameObject DrawMeshLines(List<List<Vector3>> points) {
            var allPoints = points.SelectMany(i => i).ToArray<Vector3>();

            var lineIndices = new int[(allPoints.Length * 2)];
            var currIndex = 0;

            for (int i = 0; i < points.Count; i++) {
                var currPoints = points[i];
                for (int j = 1; j < currPoints.Count; j++) {
                    int adjustedIndex = currIndex - 2 * i;
                    lineIndices[currIndex] = Mathf.CeilToInt(adjustedIndex / 2f) + i;
                    lineIndices[currIndex + 1] = Mathf.CeilToInt((adjustedIndex + 1) / 2f) + i;
                    currIndex += 2;
                }
                lineIndices[currIndex] = i * segments + currPoints.Count - 1;
                lineIndices[currIndex + 1] = i * segments;
                currIndex += 2;
            }

            var linesMesh = new Mesh();

            var container = new GameObject();
            var filter = container.AddComponent<MeshFilter>();
            filter.mesh = linesMesh;
            var pMeshRenderer = container.AddComponent<MeshRenderer>();
            pMeshRenderer.material = lineMat;

            linesMesh.vertices = allPoints;
            linesMesh.SetIndices(lineIndices, MeshTopology.Lines, 0, true);

            return container;
        }
    }
}
