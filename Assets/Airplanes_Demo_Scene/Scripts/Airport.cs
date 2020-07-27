using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;

public class Airport : MonoBehaviour, ICoordinatePoint
{
    public Location location { get; set; }
    public GameObject pointPrefab { get; set; }

    public Airport(float lat, float lng) {
        this.location = new Location(lat, lng);
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public GameObject Plot(Transform planet, Transform container, int layer) {
        var plotted = PlanetUtility.PlacePoint(planet, container, location, pointPrefab);
        return plotted;
    }
}
