using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;
using System.Linq;

public class EarthquakeLoader : MonoBehaviour, IDataLoader {
    [SerializeField] private TextAsset _dataFile;
    public TextAsset dataFile { get { return _dataFile; } set { _dataFile = value; } }

    [SerializeField] private JsonLoadedEvent _loadComplete;
    public JsonLoadedEvent loadComplete { get { return _loadComplete; } set { _loadComplete = value; } }

    [SerializeField] private GameObject pointPrefab;

    private List<(GameObject point, EarthquakePoint data)> earthquakes = new List<(GameObject point, EarthquakePoint data)>();

    private void Start() {
        ParseFile(dataFile.text);
    }

    public void ParseFile(string fileText) {
        /*var csvParsed = CsvParser.Parse(fileText);

        var csvLats = csvParsed["earthquake.latitude"].Cast<float>().ToArray();
        var csvLngs = csvParsed["earthquake.longitude"].Cast<float>().ToArray();
        var csvMags = csvParsed["earthquake.mag"].Cast<float>().ToArray();

        if(csvLats.Length != csvLngs.Length || csvLats.Length != csvMags.Length) {
            Debug.Log("Don't have the same number of values for Latitude, Longitude, and Magnitude -- ABORTING");
            return;
        }

        List<EarthquakePoint> earthquakes = new List<EarthquakePoint>();

        for (int i = 0; i < csvLats.Length; i++) {
            var lat = csvLats[i];
            var lng = csvLngs[i];
            var mag = csvMags[i];

            var eP = new EarthquakePoint(lat, lng, mag);
            eP.pointPrefab = pointPrefab;
            var plotted = eP.Plot(transform, transform);
            plotted.name = "" + mag;
            earthquakes.Add(eP);
        }*/

        var jsonParsed = JsonParser.Parse(fileText, new string[] { "mag", "coordinates", "title" });
        var mags = jsonParsed["mag"].Cast<float>().ToArray();
        var coords = jsonParsed["coordinates"].Cast<object[]>().ToArray();
        var titles = jsonParsed["title"].Cast<string>().ToArray();

        for(int i = 0; i < coords.Length; i++) {
            var lng = (float)coords[i][0];
            var lat = (float)coords[i][1];
            var mag = mags[i];

            var eP = new EarthquakePoint(lat, lng, mag);
            
            eP.pointPrefab = pointPrefab;
            var plotted = eP.Plot(transform, transform);
            plotted.name = titles[i];
            earthquakes.Add((plotted, eP));
        }
    }

    public void filter(float minMagnitude, float maxMagnitude) {
        foreach((GameObject point, EarthquakePoint data) earthquake in earthquakes) {
            earthquake.point.SetActive(earthquake.data.magnitude >= minMagnitude && earthquake.data.magnitude <= maxMagnitude);
        }
    }
}
