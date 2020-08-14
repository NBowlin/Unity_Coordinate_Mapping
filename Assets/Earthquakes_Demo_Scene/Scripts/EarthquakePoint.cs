using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;
using System;

public class EarthquakeInfo: ICoordinatePoint {
    public Location location { get; set; }
    public GameObject pointPrefab { get; set; }

    public float magnitude;
    public float depth;

    public DateTime date;

    public EarthquakeInfo(float latitude, float longitude, float magnitude, float depth, string place, long time) {
        this.location = new Location(place, latitude, longitude);
        this.magnitude = magnitude;
        this.depth = depth;
        this.date = DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime;
    }

    public GameObject Plot(Transform planet, Transform container, int layer) {
        var plotted = PlanetUtility.PlacePoint(planet, container, location, pointPrefab);

        if (plotted != null) {
            plotted.layer = layer;
            var eqPoint = plotted.AddComponent<EarthquakePoint>();
            eqPoint.info = this;

            var point = (plotted.transform.position - planet.transform.position).normalized;

            //Technically the Richter scale increases by 10^mag (2 is 10x stronger than a 1, and 3 is 100x stronger than a 1) - but that makes the scales get out of control
            //So to keep a reasonable exponential increase we use the 2^mag
            plotted.transform.localScale = new Vector3(plotted.transform.localScale.x, plotted.transform.localScale.y, plotted.transform.localScale.z * Mathf.Pow(2f, magnitude));
            plotted.transform.rotation = Quaternion.LookRotation(point);
        }

        return plotted;
    }
}

public class EarthquakePoint : MonoBehaviour {
    public EarthquakeInfo info;
    public string DisplayInfo() {
        var display = "Location: " + info.location.name + "\nMagnitude: " + info.magnitude + "\nDepth: " + info.depth + " km\nDate: " + info.date.ToString() + "\nCoordinates: " + info.location.latitude + ", " + info.location.longitude;
        return display;
    }
}
