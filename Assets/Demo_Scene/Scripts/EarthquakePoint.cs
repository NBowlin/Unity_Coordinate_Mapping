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
        return plotted;
    }
}
