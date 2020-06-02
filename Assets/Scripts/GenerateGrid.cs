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
        //TODO: Clean this up so there isn't so much duplication
        Mesh parallelsMesh = new Mesh();
        Mesh meridiansMesh = new Mesh();

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
        var mFilter = meridiansContainer.AddComponent<MeshFilter>();
        mFilter.mesh = meridiansMesh;
        var mMeshRenderer = meridiansContainer.AddComponent<MeshRenderer>();
        mMeshRenderer.material = lineMat;

        int segmentIncrement = 360 / segments;
        var parallelsPoints = new List<List<Vector3>>();
        var meridianPoints = new List<List<Vector3>>();

        for (int i = -90; i <= 90; i += degreesBetweenLines) {
            var currParallelPoints = new List<Vector3>();
            var curMeridianPoints = new List<Vector3>();
            for (int j = 0; j < 360; j += segmentIncrement) {

                var latLine = Quaternion.Euler(0.0f, j, i) * Vector3.right;
                var lngLine = Quaternion.Euler(0.0f, i, j) * Vector3.right;

                //Need to reverse the ray direction because collisions don't work from the inside of a collider
                //So take a point some distance along the line as origin, then reverse the direction
                var latRay = new Ray(transform.position, latLine * 200.0f);
                latRay.origin = latRay.GetPoint(200.0f);
                latRay.direction = -latRay.direction;

                RaycastHit latHit;

                if (Physics.Raycast(latRay, out latHit)) {
                    if (latHit.collider.gameObject.tag == "Earth") {
                        currParallelPoints.Add(latHit.point);
                    }
                }
                else {
                    Debug.Log("Raycast missed Earth");
                    Debug.DrawRay(latRay.origin, latRay.direction * 200.0f, Color.yellow, 1000.0f);
                }

                var lngRay = new Ray(transform.position, lngLine * 200.0f);
                lngRay.origin = lngRay.GetPoint(200.0f);
                lngRay.direction = -lngRay.direction;

                RaycastHit lngHit;

                if (Physics.Raycast(lngRay, out lngHit)) {
                    if (lngHit.collider.gameObject.tag == "Earth") {
                        curMeridianPoints.Add(lngHit.point);
                    }
                }
                else {
                    Debug.Log("Raycast missed Earth");
                    Debug.DrawRay(lngRay.origin, lngRay.direction * 200.0f, Color.yellow, 1000.0f);
                }
            }

            if (currParallelPoints.Count > 1) { parallelsPoints.Add(currParallelPoints); }
            if (curMeridianPoints.Count > 1) { meridianPoints.Add(curMeridianPoints); }
        }

        var allParallelsPoints = parallelsPoints.SelectMany(i => i).ToArray<Vector3>();

        var parallelsIndices = new int[(allParallelsPoints.Length * 2)];
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

        parallelsMesh.vertices = allParallelsPoints;
        parallelsMesh.SetIndices(parallelsIndices, MeshTopology.Lines, 0, true);

        var allMerdianPoints = meridianPoints.SelectMany(i => i).ToArray<Vector3>();

        var meridianIndices = new int[(allMerdianPoints.Length * 2)];
        var currMeridianIndex = 0;

        for (int i = 0; i < meridianPoints.Count; i++) {
            var currPoints = meridianPoints[i];
            for (int j = 1; j < currPoints.Count; j++) {
                int adjustedIndex = currMeridianIndex - 2 * i;
                meridianIndices[currMeridianIndex] = Mathf.CeilToInt(adjustedIndex / 2f) + i;
                meridianIndices[currMeridianIndex + 1] = Mathf.CeilToInt((adjustedIndex + 1) / 2f) + i;
                currMeridianIndex += 2;
            }
            meridianIndices[currMeridianIndex] = i * segments + currPoints.Count - 1;
            meridianIndices[currMeridianIndex + 1] = i * segments;
            currMeridianIndex += 2;
        }

        meridiansMesh.vertices = allMerdianPoints;
        meridiansMesh.SetIndices(meridianIndices, MeshTopology.Lines, 0, true);

        //mesh.vertices = points;
        //mesh.SetIndices(new int[] { 0, 1, 1, 2, 3, 4 }, MeshTopology.Lines, 0, true);
    }

    void DrawUsingCustomMesh() {

    }
}
