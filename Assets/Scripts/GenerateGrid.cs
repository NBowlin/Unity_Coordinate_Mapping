using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    public GameObject graticulePrefab;

    [SerializeField] private int segments;
    [SerializeField] private int angleIncrement;

    // Start is called before the first frame update
    void Start() {
        DrawLongitudeLines();
    }

    void DrawLongitudeLines() {
        for (int i = 0; i < 360; i += angleIncrement) {

            var graticuleGO = Instantiate(graticulePrefab, transform);
            var graticule = graticuleGO.GetComponent<DrawGraticule>();
            graticule.segments = segments;
            graticule.angle = i;

            /*var longitude = Quaternion.AngleAxis((float)i, Vector3.up) * Vector3.right;

            int segmentIncrement = 360 / segments;
            int currentSegment = 0;
            for (int j = 0; j < 360; j += segmentIncrement) {
                var point = Quaternion.AngleAxis((float)j, longitude) * Vector3.up;
                point *= 0.5f;
                //Debug.DrawRay(transform.localPosition, point, Color.red, 100.0f);
                Debug.Log("Add point: " + point);
                //line.SetPosition(currentSegment, point);
                currentSegment += 1;
            }*/
            //var latitude = Quaternion.AngleAxis((float)i, Vector3.up) * Vector3.right;

        }
    }
}
