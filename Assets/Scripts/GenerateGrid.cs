using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    public GameObject graticulePrefab;

    [SerializeField] private int segments;
    [SerializeField] private int degreesBetweenLines;

    // Start is called before the first frame update
    void Start() {
        DrawLines();
    }

    void DrawLines() {
        for (int i = -90; i <= 90; i += degreesBetweenLines) {
            var latGraticuleGO = Instantiate(graticulePrefab, transform);
            var latGraticule = latGraticuleGO.GetComponent<DrawGraticule>();
            latGraticule.segments = segments;
            latGraticule.angle = i;
            latGraticule.isLatitude = true;

            var lngGraticuleGO = Instantiate(graticulePrefab, transform);
            var lngGraticule = lngGraticuleGO.GetComponent<DrawGraticule>();
            lngGraticule.segments = segments;
            lngGraticule.angle = i;
            lngGraticule.isLatitude = false;
        }
    }
}
