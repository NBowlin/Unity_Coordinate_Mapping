using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenerateGrid : MonoBehaviour {
    private enum DrawingTechnique {
        LineRenderer,
        MeshTopologyLines,
        CustomMesh
    };

    [SerializeField] private GameObject graticulePrefab;
    [SerializeField] private Material lineMat;

    [SerializeField] private int segments;
    [SerializeField] private int degreesBetweenLines;

    [SerializeField] private DrawingTechnique drawType;

    [SerializeField] private bool drawParallels = true;
    [SerializeField] private bool drawMeridians = true;

    // Start is called before the first frame update
    void Start() {

        switch (drawType) {
            case DrawingTechnique.LineRenderer:
                DrawUsingLinesRenderer();
                break;
            case DrawingTechnique.MeshTopologyLines:
                DrawUsingMeshTopologyLines();
                break;
            case DrawingTechnique.CustomMesh:
                DrawUsingCustomMesh();
                break;
        }
    }

    void DrawUsingLinesRenderer() {

        var graticuleContainer = new GameObject("Graticule");
        graticuleContainer.transform.SetParent(transform, false);

        var parallelsContainer = new GameObject("Parallels");
        parallelsContainer.transform.SetParent(graticuleContainer.transform, false);

        var meridiansContainer = new GameObject("Meridians");
        meridiansContainer.transform.SetParent(graticuleContainer.transform, false);

        for (int i = -90; i <= 90; i += degreesBetweenLines) {
            if (drawParallels) {
                var latGraticuleGO = Instantiate(graticulePrefab, parallelsContainer.transform);
                latGraticuleGO.name = "" + i;
                var latGraticule = latGraticuleGO.GetComponent<DrawGraticule>();
                latGraticule.segments = segments;
                latGraticule.angle = i;
                latGraticule.isLatitude = true;
            }

            if (drawMeridians) {
                var lngGraticuleGO = Instantiate(graticulePrefab, meridiansContainer.transform);
                lngGraticuleGO.name = "" + i;
                var lngGraticule = lngGraticuleGO.GetComponent<DrawGraticule>();
                lngGraticule.segments = segments;
                lngGraticule.angle = i;
                lngGraticule.isLatitude = false;
            }
        }
    }

    void DrawUsingMeshTopologyLines() {
        Mesh parallelsMesh = new Mesh();

        var graticuleContainer = new GameObject("Graticule");
        graticuleContainer.transform.SetParent(transform, false);
        graticuleContainer.transform.localScale = new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z);

        var parallelsContainer = new GameObject("Parallels");
        parallelsContainer.transform.SetParent(graticuleContainer.transform, false);
        var pFilter = parallelsContainer.AddComponent<MeshFilter>();
        pFilter.mesh = parallelsMesh;
        var pMeshRenderer = parallelsContainer.AddComponent<MeshRenderer>();
        pMeshRenderer.material = lineMat;


        var meridiansContainer = new GameObject("Meridians");
        meridiansContainer.transform.SetParent(graticuleContainer.transform, false);
        meridiansContainer.AddComponent<MeshFilter>();
        meridiansContainer.AddComponent<MeshRenderer>();

        int segmentIncrement = 360 / segments;
        var parallelsPoints = new List<List<Vector3>>();

        for (int i = -90; i <= 90; i += degreesBetweenLines) {
            var currParallelPoints = new List<Vector3>();
            for (int j = 0; j < 360; j += segmentIncrement) {

                var line = Quaternion.Euler(0.0f, j, i) * Vector3.right;

                //Need to reverse the ray direction because collisions don't work from the inside of a collider
                //So take a point some distance along the line as origin, then reverse the direction
                var ray = new Ray(transform.position, line * 200.0f);
                ray.origin = ray.GetPoint(200.0f);
                ray.direction = -ray.direction;

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)) {
                    if (hit.collider.gameObject.tag == "Earth") {
                        currParallelPoints.Add(hit.point);
                    }
                }
                else {
                    Debug.Log("Raycast missed Earth");
                    Debug.DrawRay(ray.origin, ray.direction * 200.0f, Color.yellow, 1000.0f);
                }
            }

            if (currParallelPoints.Count > 1) { parallelsPoints.Add(currParallelPoints); }
        }

        var allPoints = parallelsPoints.SelectMany(i => i).ToArray<Vector3>();

        var parallelsIndices = new int[(allPoints.Length * 2)];
        var currIndex = 0;

        for (int i = 0; i < parallelsPoints.Count; i++) {
            var currPoints = parallelsPoints[i];
            for (int j = 1; j < currPoints.Count; j++) {
                int adjustedIndex = currIndex - 2 * i;
                parallelsIndices[currIndex] = Mathf.CeilToInt(adjustedIndex / 2f) + i;
                parallelsIndices[currIndex + 1] = Mathf.CeilToInt((adjustedIndex + 1) / 2f) + i;
                currIndex += 2;
            }
            parallelsIndices[currIndex] = i * segments + currPoints.Count - 1;
            parallelsIndices[currIndex + 1] = i * segments;
            currIndex += 2;
        }

        parallelsMesh.vertices = allPoints;
        parallelsMesh.SetIndices(parallelsIndices, MeshTopology.Lines, 0, true);

        //mesh.vertices = points;
        //mesh.SetIndices(new int[] { 0, 1, 1, 2, 3, 4 }, MeshTopology.Lines, 0, true);
    }

    void DrawUsingCustomMesh() {

    }
}
