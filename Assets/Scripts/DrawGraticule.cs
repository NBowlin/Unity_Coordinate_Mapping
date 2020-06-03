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

            var hitInfo = PlanetUtility.LineFromOriginToSurface(transform, line);
            if(hitInfo.HasValue) {
                lineRenderer.SetPosition(currSegment, hitInfo.Value.point);
            }

            currSegment += 1;
        }
    }
}
