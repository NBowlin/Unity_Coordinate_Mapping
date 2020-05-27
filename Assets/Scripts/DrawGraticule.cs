using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGraticule : MonoBehaviour
{
    [SerializeField] private LineRenderer line;

    public int segments;
    public float angle;

    // Start is called before the first frame update
    void Start()
    {
        line.positionCount = segments;
        line.startWidth = 0.005f;
        line.endWidth = 0.005f;
        line.useWorldSpace = false;
        line.loop = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        DrawLongitudeLines();
        //DrawLatitudeLines();
    }

    void DrawLongitudeLines() {
            var longitude = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;

            int segmentIncrement = 360 / segments;
            int currentSegment = 0;
            for (int j = 0; j < 360; j += segmentIncrement) {
                var point = Quaternion.AngleAxis((float)j, longitude) * Vector3.up;
                point *= 0.502f;
                line.SetPosition(currentSegment, point);
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
            line.SetPosition(currentSegment, point);
            currentSegment += 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
