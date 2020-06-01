using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    public GameObject graticulePrefab;

    [SerializeField] private int segments;
    [SerializeField] private int degreesBetweenLines;

    [SerializeField] private bool drawParallels = true;
    [SerializeField] private bool drawMeridians = true;

    // Start is called before the first frame update
    void Start() {
        DrawLines();
    }

    void DrawLines() {

        var graticuleContainer = new GameObject("Graticule");
        graticuleContainer.transform.SetParent(transform, false);

        var parallelsContainer = new GameObject("Parallels");
        parallelsContainer.transform.SetParent(graticuleContainer.transform, false);

        var meridiansContainer = new GameObject("Meridians");
        meridiansContainer.transform.SetParent(graticuleContainer.transform, false);

        for (int i = -90; i <= 90; i += degreesBetweenLines) {
            if (drawParallels) {
                var latGraticuleGO = Instantiate(graticulePrefab, parallelsContainer.transform);
                var latGraticule = latGraticuleGO.GetComponent<DrawGraticule>();
                latGraticule.segments = segments;
                latGraticule.angle = i;
                latGraticule.isLatitude = true;
            }

            if (drawMeridians) {
                var lngGraticuleGO = Instantiate(graticulePrefab, meridiansContainer.transform);
                var lngGraticule = lngGraticuleGO.GetComponent<DrawGraticule>();
                lngGraticule.segments = segments;
                lngGraticule.angle = i;
                lngGraticule.isLatitude = false;
            }
        }
    }
}
