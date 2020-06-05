using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatesMapper : MonoBehaviour
{
    [SerializeField] private List<CoordinatePoint_Basic> locations;
    [SerializeField] private List<CoordinatePoint_Magnitude> magLocations;

    private void Start() {

        var locationsContainer = new GameObject("Locations");
        locationsContainer.transform.SetParent(transform, false);

        foreach (CoordinatePoint point in locations) {
            point.Plot(transform, locationsContainer.transform);
        }

        var magLocationsContainer = new GameObject("Magnitude Locations");
        magLocationsContainer.transform.SetParent(transform, false);

        foreach (CoordinatePoint_Magnitude point in magLocations) {
            point.Plot(transform, magLocationsContainer.transform);
        }
    }
}
