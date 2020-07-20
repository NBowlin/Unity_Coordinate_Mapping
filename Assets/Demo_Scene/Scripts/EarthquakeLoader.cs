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

    private void Start() {
        ParseFile(dataFile.text);
    }

    public void ParseFile(string fileText) {
        var csvParsed = CsvParser.Parse(fileText);

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
            eP.Plot(transform, transform);
            earthquakes.Add(eP);
        }
    }
}
