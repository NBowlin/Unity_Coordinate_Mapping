using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;
using System;

public class EarthquakePoint : MonoBehaviour, ICoordinatePoint {
    public Location location { get; set; }
    public GameObject pointPrefab { get; set; }

    public float magnitude;
    public float depth;

    public DateTime date;

    public EarthquakePoint(float latitude, float longitude, float magnitude, float depth, string place, long time) {
        this.location = new Location(place, latitude, longitude);
        this.magnitude = magnitude;
        this.depth = depth;
        this.date = DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime;
    }

    public GameObject Plot(Transform planet, Transform container, int layer) {
        var plotted = PlanetUtility.PlacePoint(planet, container, location, pointPrefab);

        if(plotted != null) {
            plotted.layer = layer;
            var eqPoint = plotted.AddComponent<EarthquakePoint>();
            //Copy over info from this point to the one on the gameobject
            eqPoint.location = location;
            eqPoint.magnitude = magnitude;
            eqPoint.depth = depth;
            eqPoint.date = date;

            var point = (plotted.transform.position - planet.transform.position).normalized;

            //Technically the Richter scale increases by 10^mag (2 is 10x stronger than a 1, and 3 is 100x stronger than a 1) - but that makes the scales get out of control
            //So to keep a reasonable exponential increase we use the 2^mag
            plotted.transform.localScale = new Vector3(plotted.transform.localScale.x, plotted.transform.localScale.y, plotted.transform.localScale.z * Mathf.Pow(2f, magnitude));
            plotted.transform.rotation = Quaternion.LookRotation(point);
        }

        return plotted;
    }

    public string DisplayInfo() {
        var info = "Location: " + location.name + "\nMagnitude: " + magnitude + "\nDepth: " + depth + " km\nDate: " + date.ToString() + "\nCoordinates: " + location.latitude + ", " + location.longitude;
        return info;
    }
}
