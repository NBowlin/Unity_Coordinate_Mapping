using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatesMapper : MonoBehaviour
{
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private List<Location> locations;

    private void Start() {

        var locationsContainer = new GameObject("Locations");
        locationsContainer.transform.SetParent(transform, false);

        foreach (Location loc in locations) {
            MapLocation(loc, locationsContainer.transform);
        }
    }

    private void MapLocation(Location loc, Transform parentContainer) {
        var point = Quaternion.Euler(0.0f, -loc.longitude, loc.latitude) * Vector3.right;

        var hitInfo = PlanetUtility.LineFromOriginToSurface(transform, point);
        if(hitInfo.HasValue) {
            var go = Instantiate(pointPrefab, hitInfo.Value.point, Quaternion.identity, parentContainer);
            go.name = loc.name;
        }
    }
}
