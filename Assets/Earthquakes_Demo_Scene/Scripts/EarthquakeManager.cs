using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper;
using System.Linq;
using System;

public class EarthquakeManager : MonoBehaviour, IDataLoader {
    [SerializeField] private TextAsset _dataFile;
    public TextAsset dataFile { get { return _dataFile; } set { _dataFile = value; } }

    [SerializeField] private DataLoadedEvent _loadComplete;
    public DataLoadedEvent loadComplete { get { return _loadComplete; } set { _loadComplete = value; } }

    [SerializeField] private GameObject pointPrefab = null;

    private List<(GameObject point, EarthquakeInfo data)> earthquakes = new List<(GameObject point, EarthquakeInfo data)>();
    private GameObject earthquakesContainer;

    private void Start() {
        earthquakesContainer = new GameObject("Earthquakes");
        earthquakesContainer.transform.SetParent(transform, false);
        ParseFile(dataFile.text);
    }

    public async void ParseFile(string fileText) {
        var jsonParsed = await JsonParser.ParseAsync(fileText, new string[] { "mag", "coordinates", "title", "place", "time" });

        var mags = jsonParsed["mag"].Select(m => Convert.ToSingle(m)).ToArray();
        var coords = jsonParsed["coordinates"].Cast<object[]>().ToArray();
        var titles = jsonParsed["title"].Cast<string>().ToArray();
        var places = jsonParsed["place"].Cast<string>().ToArray();
        var times = jsonParsed["time"].Cast<long>().ToArray();

        for (int i = 0; i < coords.Length; i++) {
            var lng = Convert.ToSingle(coords[i][0]);
            var lat = Convert.ToSingle(coords[i][1]);
            var depth = Convert.ToSingle(coords[i][2]);
            var mag = mags[i];
            var place = places[i];
            var time = times[i];

            var eP = new EarthquakeInfo(lat, lng, mag, depth, place, time);
            
            eP.pointPrefab = pointPrefab;
            var plotted = eP.Plot(transform, earthquakesContainer.transform, LayerMask.NameToLayer("Location"));

            plotted.name = titles[i];
            earthquakes.Add((plotted, eP));
        }

        var cps = earthquakes.Select(p => p.data);
        updateHeatmap();
    }

    public void ToggleEarthquakes(bool on) {
        earthquakesContainer.SetActive(!on);
    }

    public void filter(float minMagnitude, float maxMagnitude) {
        foreach((GameObject point, EarthquakeInfo data) earthquake in earthquakes) {
            earthquake.point.SetActive(earthquake.data.magnitude >= minMagnitude && earthquake.data.magnitude <= maxMagnitude);
        }
    }

    public void updateHeatmap() {
        //Linq is moving off the main thread?
        //var cps = earthquakes.Where(p => p.point.activeSelf).Select(d => d.data);

        List<EarthquakeInfo> cps = new List<EarthquakeInfo>();
        foreach((GameObject point, EarthquakeInfo data) eq in earthquakes) {
            if(eq.point.activeSelf) { cps.Add(eq.data); }
        }

        if (loadComplete != null) { loadComplete.Invoke(cps); }
    }
}
