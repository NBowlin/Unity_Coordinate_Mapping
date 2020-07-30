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
}

public class Airplane_Realtime : MonoBehaviour
{
    [HideInInspector] public Transform planet;
    [HideInInspector] public Transform northPole;

    [SerializeField] private Transform planeModel;
    [SerializeField] private float speed;

    private float altitude = 0.1f;

    public FlightInfo info;
    // Start is called before the first frame update
    void Start()
    {
        planet = GameObject.Find("Earth").transform; //TODO: Don't like this but quick and dirty
        northPole = GameObject.Find("NorthPole").transform;

        //planeModel.rotation = Quaternion.Euler(0f, info.heading, 0f);
        planeModel.rotation = Quaternion.LookRotation(northPole.position - planeModel.position);
        planeModel.rotation = Quaternion.Euler(0f, info.heading + planeModel.rotation.eulerAngles.y, 0f);

        var planetHit = PlanetUtility.LineFromOriginToSurface(planet, transform.position, LayerMask.GetMask("Planet"));

        if (planetHit.HasValue) {
            transform.position = planetHit.Value.point + transform.position * altitude;
            transform.up = planetHit.Value.normal;

            Debug.DrawLine(transform.position, planetHit.Value.point, Color.yellow, 2f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position += planeModel.forward * speed * Time.deltaTime; 

        /*var planetHit = PlanetUtility.LineFromOriginToSurface(planet, transform.position, LayerMask.GetMask("Planet"));

        if (planetHit.HasValue) {
            transform.position = planetHit.Value.point + transform.position * altitude;
            transform.up = planetHit.Value.normal;
        }*/
    }
}
