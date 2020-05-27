using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGraticule : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    [HideInInspector] public int segments;
    [HideInInspector] public float angle;
    [HideInInspector] public bool isLatitude;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = 0.006f;
        lineRenderer.endWidth = 0.006f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        DrawLine();
        //DrawLongitudeLines();
        //DrawLatitudeLines();
    }

    void DrawLine() {
        int segmentIncrement = 360 / segments;
        int currSegment = 0;
        for (int j = 0; j < 360; j += segmentIncrement) {

            var line = (isLatitude ? Quaternion.Euler(0.0f, j, angle) : Quaternion.Euler(0.0f, angle, j)) * Vector3.right;

            //Need to reverse the ray direction because collisions don't work from the inside of a collider
            //So take a point some distance along the line as origin, then reverse the direction
            var ray = new Ray(Vector3.zero, line * 6.0f);
            ray.origin = ray.GetPoint(6.0f);
            ray.direction = -ray.direction;

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.gameObject.tag == "Earth") {
                    lineRenderer.SetPosition(currSegment, hit.point);
                }
            }

            currSegment += 1;
        }
    }

    void DrawLongitudeLines() {
            var longitude = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;

            int segmentIncrement = 360 / segments;
            int currentSegment = 0;
            for (int j = 0; j < 360; j += segmentIncrement) {
                var point = Quaternion.AngleAxis((float)j, longitude) * Vector3.up;
                point *= 0.502f;
                lineRenderer.SetPosition(currentSegment, point);
                currentSegment += 1;
            }
    }

    void DrawLatitudeLines() {
        var longitude = Vector3.up; //Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;

        int segmentIncrement = 360 / segments;
        int currentSegment = 0;
        for (int j = 0; j < 360; j += segmentIncrement) {
            var point = Quaternion.AngleAxis((float)j, longitude) * Vector3.right;
            point *= 0.502f;
            lineRenderer.SetPosition(currentSegment, point);
            currentSegment += 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
