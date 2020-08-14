using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateMapper {
    public class CoordinatesMapper : MonoBehaviour {
        //[SerializeField] private List<CoordinatePoint> locations;
        [SerializeField] private GameObject pointPrefab = null;

        private void Start() {
            /*var locationsContainer = new GameObject("Locations");
            locationsContainer.transform.SetParent(transform, false);

            foreach (CoordinatePoint point in locations) {
                point.Plot(transform, locationsContainer.transform);
            }

            var magLocationsContainer = new GameObject("Magnitude Locations");
            magLocationsContainer.transform.SetParent(transform, false);

            foreach (CoordinatePoint_Magnitude point in magLocations) {
                point.Plot(transform, magLocationsContainer.transform);
            }*/
        }

        //TODO: Updated the class to use ICoordinatePoint - check back after rework
        public void MapPoints(IEnumerable<ICoordinatePoint> p) {
            Debug.Log("Map points: " + p);

            var container = new GameObject("Locs");
            container.transform.SetParent(transform, false);

            foreach (ICoordinatePoint point in p) {
                point.pointPrefab = pointPrefab; //TODO: Rework this
                point.Plot(transform, container.transform, 0); //Default layer
            }
        }
    }
}
