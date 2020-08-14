using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;

public class FlightInfo : ICoordinatePoint {
    public Location location { get; set; }
    public GameObject pointPrefab { get; set; }

    public string icao24;
    public string callSign;
    public float velocity;
    public float heading;
    public float altitude;

    public FlightInfo(float lat, float lng) {
        location = new Location(lat, lng);
    }

    public FlightInfo(float lat, float lng, string icao24, string callSign, float velocity, float heading, float altitude) {
        location = new Location(lat, lng);
        this.icao24 = icao24;
        this.callSign = callSign;
        this.velocity = velocity;
        this.heading = heading;
        this.altitude = altitude;
    }

    public GameObject Plot(Transform planet, Transform container, int layer) {
        var plotted = PlanetUtility.PlacePoint(planet, container, location, pointPrefab);
        return plotted;
    }

    public string DisplayInfo() {
        var display = "icao24: " + icao24 + "\nCallsign: " + callSign + "\nVelocity: " + velocity + " m/s\nAltitude: " + altitude + " m\nHeading: " + heading + "°s off north";
        return display;
    }
}

public class Airplane_Realtime : MonoBehaviour
{
    [HideInInspector] public Transform planet;
    [HideInInspector] public Transform northPole;

    [SerializeField] private Transform planeModel = null;

    //private float altitude = 0.1f;
    public float planetRadius;
    public float planetScale;
    private float meter;

    public FlightInfo info;
    // Start is called before the first frame update
    void Start()
    {
        meter = planetScale / (planetRadius * 2f);
        planet = GameObject.Find("Earth").transform; //TODO: Don't like this but quick and dirty
        northPole = GameObject.Find("NorthPole").transform;

        UpdateInfo(info);
    }

    public void UpdateInfo(FlightInfo info) {
        this.info = info;
        var altitude = info.altitude * meter * 100; //With the scale of the earth, you can't really see the difference in altitude height between planes, so multiply by 100 to exaggerate it.
        Debug.DrawLine(transform.position, transform.position + -transform.up * altitude, Color.yellow, 2f);

        transform.rotation = Quaternion.identity;
        planeModel.rotation = Quaternion.identity;

        //Heading is based on angle from north pole, so look at true north then rotate to heading
        planeModel.rotation = Quaternion.LookRotation(northPole.position - planeModel.position);
        planeModel.rotation = Quaternion.Euler(0f, info.heading + planeModel.rotation.eulerAngles.y, 0f);

        var line = PlanetUtility.VectorFromLatLng(info.location.latitude, info.location.longitude, Vector3.right);
        var planetHit = PlanetUtility.LineFromOriginToSurface(planet, line, LayerMask.GetMask("Planet"));

        if (planetHit.HasValue) {
            Debug.DrawLine(planetHit.Value.point, planetHit.Value.point + planetHit.Value.point * altitude, Color.red, 2f);

            transform.position = planetHit.Value.point + planetHit.Value.point * altitude;
            transform.up = planetHit.Value.normal;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += planeModel.forward * (info.velocity * meter) * Time.deltaTime;
    }
}
