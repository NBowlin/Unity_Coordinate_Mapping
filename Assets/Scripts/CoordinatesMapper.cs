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

        var ray = new Ray(transform.position, point * 6.0f);
        ray.origin = ray.GetPoint(6.0f);
        ray.direction = -ray.direction;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.gameObject.tag == "Earth") {
                var go = Instantiate(pointPrefab, hit.point, Quaternion.identity, parentContainer);
                go.name = loc.name;
            }
        }
        else {
            Debug.Log("Raycast missed Earth");
            Debug.DrawRay(ray.origin, ray.direction * 6.0f, Color.yellow, 1000.0f);
        }
    }
}
