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
        lineRenderer.startWidth = 0.001f;
        lineRenderer.endWidth = 0.001f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        DrawLine();
    }

    void DrawLine() {
        int segmentIncrement = 360 / segments;
        int currSegment = 0;
        for (int j = 0; j < 360; j += segmentIncrement) {

            var line = (isLatitude ? Quaternion.Euler(0.0f, j, angle) : Quaternion.Euler(0.0f, angle, j)) * Vector3.right;

            //Need to reverse the ray direction because collisions don't work from the inside of a collider
            //So take a point some distance along the line as origin, then reverse the direction
            var ray = new Ray(transform.position, line * 200.0f);
            ray.origin = ray.GetPoint(200.0f);
            ray.direction = -ray.direction;

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.gameObject.tag == "Earth") {
                    lineRenderer.SetPosition(currSegment, hit.point);
                }
            } else {
                Debug.Log("Raycast missed Earth");
                Debug.DrawRay(ray.origin, ray.direction * 200.0f, Color.yellow, 1000.0f);
            }

            currSegment += 1;
        }
    }
}
