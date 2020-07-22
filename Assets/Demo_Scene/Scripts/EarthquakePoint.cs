using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;

public class EarthquakePoint : ICoordinatePoint {
    public Location location { get; set; }
    public GameObject pointPrefab { get; set; }

    public float magnitude;

    public EarthquakePoint(float latitude, float longitude, float magnitude) {
        this.location = new Location(latitude, longitude);
        this.magnitude = magnitude;
    }

    public GameObject Plot(Transform planet, Transform container) {
        //TODO: Plot
        var plotted = PlanetUtility.PlacePoint(planet, container, location, pointPrefab);

        if(plotted != null) {
            var point = (plotted.transform.position - planet.transform.position).normalized;

            //Technically the Richter scale increases by a power of 10 (2 is 10x stronger than a 1, and 3 is 100x stronger than a 1) - but that makes the scales get out of control
            //So to keep a reasonable exponential increase we use the power of 2
            plotted.transform.localScale = new Vector3(plotted.transform.localScale.x, plotted.transform.localScale.y, plotted.transform.localScale.z * Mathf.Pow(2f, magnitude));
            plotted.transform.rotation = Quaternion.LookRotation(point);
        }

        return plotted;
    }
}
