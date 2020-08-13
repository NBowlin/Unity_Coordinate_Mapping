using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;
using System.Linq;
using UnityEngine.Events;

[System.Serializable] public class AirportsPlottedEvent : UnityEvent<List<GameObject>> { }

public class AirportLoader : MonoBehaviour, IDataLoader {
    [SerializeField] private TextAsset _dataFile;
    public TextAsset dataFile { get { return _dataFile; } set { _dataFile = value; } }

    public DataLoadedEvent loadComplete { get; set; }

    [SerializeField] private GameObject airportPrefab;

    [SerializeField] private AirportsPlottedEvent airportsPlotted;
    private List<GameObject> airports = new List<GameObject>();

    void Start() {
        ParseFile(dataFile.text);
    }

    public void ParseFile(string fileText) {
        //TODO: Update this to model parser?
        var parsed = JsonParser.Parse(fileText, new string[] { "code", "lat", "lon" });

        var codes = parsed["code"].Cast<string>().ToArray();
        var lats = parsed["lat"].Cast<float>().ToArray();
        var lngs = parsed["lon"].Cast<float>().ToArray();

        var container = new GameObject("Airports");
        container.transform.SetParent(transform, false);

        for (int i = 0; i < lats.Length; i++) {
            var lng = lngs[i];
            var lat = lats[i];

            var airport = new Airport(lat, lng);
            airport.pointPrefab = airportPrefab;

            var plotted = airport.Plot(transform, container.transform, 0); //Default layer
            plotted.name = codes[i];
            airports.Add(plotted);
        }

        if (airportsPlotted != null) { airportsPlotted.Invoke(airports); }
    }
}
