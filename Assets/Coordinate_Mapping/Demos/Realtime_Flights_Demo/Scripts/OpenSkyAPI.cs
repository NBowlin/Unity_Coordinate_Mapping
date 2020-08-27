using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;

using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

public class OpenSkyAPI : MonoBehaviour {
    private HttpClient client = new HttpClient();

    private CancellationTokenSource apiCanceler = new CancellationTokenSource();
    private CancellationTokenSource apiDelayCanceler = new CancellationTokenSource();

    [SerializeField] private GameObject prefab = null;
    [SerializeField] private int planeLimit = 10;
    [SerializeField] private float apiDelay = 200f;
    [SerializeField] private float planetRadius = 6371000f;

    private void Start() {
        GetInitialFlights();
    }

    private async void GetInitialFlights() {
        var json = await FlightsApi();
        var flightInfos = ParseJson(json);
        var planes = CreatePlanes(flightInfos);

        await Task.Delay(TimeSpan.FromSeconds(apiDelay), apiDelayCanceler.Token);
        UpdateTrackedFlights(planes);
    }

    private async void UpdateTrackedFlights(List<GameObject> flights) {
        var icaos = flights.Select(f => f.GetComponent<Airplane_Realtime>().info.icao24).ToArray();
        var json = await FlightsApi(icaos);
        var flightInfos = ParseJson(json);

        foreach(FlightInfo info in flightInfos) {
            foreach(GameObject plane in flights) {
                var planeScript = plane.GetComponent<Airplane_Realtime>();
                if (planeScript.info.icao24 == info.icao24) {
                    planeScript.UpdateInfo(info);
                    break;
                }
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(apiDelay), apiDelayCanceler.Token);
        UpdateTrackedFlights(flights);
    }

    private List<GameObject> CreatePlanes(List<FlightInfo> infos) {
        var planes = GameObject.Find("Planes");
        if (planes != null) { Destroy(planes); }

        var container = new GameObject("Planes");
        container.transform.SetParent(transform, false);

        List<GameObject> plottedPlanes = new List<GameObject>();

        foreach (FlightInfo info in infos) {
            var plotted = info.Plot(transform, container.transform, LayerMask.NameToLayer("Location"));
            var planeScript = plotted.GetComponent<Airplane_Realtime>();
            planeScript.planetRadius = planetRadius;
            planeScript.planetScale = transform.localScale.x;
            planeScript.info = info;
            plotted.name = "icao24: " + info.icao24 + " - Callsign: " + info.callSign;
            plottedPlanes.Add(plotted);
        }

        return plottedPlanes;
    }

    private List<FlightInfo> ParseJson(string json) {
        var parsed = JObject.Parse(json);

        var tokens = parsed["states"];
        int count = 0;

        List<FlightInfo> infos = new List<FlightInfo>();

        foreach (JToken token in tokens) {
            var icao24T = token[0];
            var callsignT = token[1];
            var lngT = token[5];
            var latT = token[6];
            var velT = token[9];
            var headingT = token[10];
            var altT = token[7];

            var tokenArr = new JToken[] { icao24T, callsignT, lngT, latT, velT, headingT, altT };

            bool nullVal = false;
            foreach (JToken temp in tokenArr) {
                if (temp.Type == JTokenType.Null) {
                    nullVal = true;
                    break;
                }
            }

            if (nullVal) { continue; }

            var icao24 = (string)icao24T;
            var callSign = ((string)callsignT).Trim();
            var lng = (float)lngT;
            var lat = (float)latT;
            var vel = (float)velT;
            var heading = (float)headingT;
            var alt = (float)altT;

            var info = new FlightInfo(lat, lng, icao24, callSign, vel, heading, alt);
            info.pointPrefab = prefab;
            infos.Add(info);

            count++;

            if (count >= planeLimit) { break; }
        }

        return infos;
    }

    private async Task<string> FlightsApi() {
        return await FlightsApi(new string[] { });
    }

    private async Task<string> FlightsApi(string[] icao24s) {
        try {
            Debug.Log("Hitting Open Sky API");
            var icaos = icao24s.Aggregate("?icao24=", (e, x) => e + "," + x);
            var query = icaos.Length > 0 ? icaos : "";
            var url = "https://opensky-network.org/api/states/all" + query;

            var response = await client.GetAsync(url, apiCanceler.Token);
            var json = response.Content.ReadAsStringAsync().Result;
            Debug.Log("Json: " + json);
            return json;
        }
        catch (Exception e) {
            Debug.Log("Error with OpenSky: " + e.ToString());
            return null;
        }
    }

    private void OnDestroy() {
        apiCanceler.Cancel();
        apiDelayCanceler.Cancel();

        //Don't think I actually need to do this here - Should get cleaned up on it's own
        apiCanceler.Dispose();
        apiDelayCanceler.Dispose();

        apiCanceler = null;
        apiDelayCanceler = null;
    }
}
