using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateMapper {
    public class DefaultCoordinatePoint : MonoBehaviour, ICoordinatePoint {
        public Location location { get; set; }
        public GameObject pointPrefab { get; set; }

        public float magnitude;

        public DefaultCoordinatePoint(float latitude, float longitude, float magnitude) {
            this.location = new Location("Default Point", latitude, longitude);
            this.magnitude = magnitude;
        }

        public GameObject Plot(Transform planet, Transform container, int layer) {
            var plotted = PlanetUtility.PlacePoint(planet, container, location, pointPrefab);

            if (plotted != null) {
                plotted.layer = layer;
                var cp = plotted.AddComponent<DefaultCoordinatePoint>();
                //Copy over info from this point to the one on the gameobject
                cp.location = location;
                cp.magnitude = magnitude;

                var point = (plotted.transform.position - planet.transform.position).normalized;
                plotted.transform.localScale = new Vector3(plotted.transform.localScale.x, plotted.transform.localScale.y, plotted.transform.localScale.z * magnitude);
                plotted.transform.rotation = Quaternion.LookRotation(point);
            }

            return plotted;
        }
    }
}
