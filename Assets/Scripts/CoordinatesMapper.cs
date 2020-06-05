using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatesMapper : MonoBehaviour
{
    [SerializeField] private List<CoordinatePoint> locations;

    private void Start() {

        var locationsContainer = new GameObject("Locations");
        locationsContainer.transform.SetParent(transform, false);

        foreach (CoordinatePoint point in locations) {
            point.Plot(transform, locationsContainer.transform);
        }
    }
}
