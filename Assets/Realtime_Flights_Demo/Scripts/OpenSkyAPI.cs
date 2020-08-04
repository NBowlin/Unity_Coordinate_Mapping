using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System;
using Newtonsoft.Json.Linq;
using CoordinateMapper;
using Unity.Jobs;

using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

public class OpenSkyAPI : MonoBehaviour {
    private HttpClient client = new HttpClient();

    private CancellationTokenSource apiCanceler = new CancellationTokenSource();
    private CancellationTokenSource apiDelayCanceler = new CancellationTokenSource();

    [SerializeField] private GameObject prefab;

    private void Start() {
        //GetFlights();
        GetInitialFlights();
    }

    private async void GetInitialFlights() {
        var json = await FlightsApi();
        var flightInfos = ParseJson(json);
        CreatePlanes(flightInfos);

        await Task.Delay(TimeSpan.FromSeconds(10f), apiDelayCanceler.Token);
        UpdateTrackedFlights(flightInfos);
    }

    private async void UpdateTrackedFlights(List<FlightInfo> flights) {
        var icaos = flights.Select(f => f.icao24).ToArray();
        var json = await FlightsApi(icaos);
        var flightInfos = ParseJson(json);

        //TODO: Update existing planes rather than blowing everything away
        CreatePlanes(flightInfos);
        await Task.Delay(TimeSpan.FromSeconds(10f), apiDelayCanceler.Token);
        UpdateTrackedFlights(flightInfos);
    }

    private void CreatePlanes(List<FlightInfo> infos) {
        var planes = GameObject.Find("Planes");
        if (planes != null) { Destroy(planes); }

        var container = new GameObject("Planes");
        container.transform.SetParent(transform, false);

        foreach (FlightInfo info in infos) {
            var plotted = info.Plot(transform, container.transform, 0); //Default layer
            var planeScript = plotted.GetComponent<Airplane_Realtime>();
            planeScript.info = info;
            plotted.name = info.icao24 + "_" + info.callSign + "_" + info.heading.ToString("F2");
        }
    }

    private /*async*/ List<FlightInfo> ParseJson(string json) {
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

            if (count > 200) { break; }
        }

        Debug.Log("Count: " + count);
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
            return null; //TODO: Should we return the error here maybe?
        }
    }

    /*
    # A state vector consists of:
    # Index Property        Type    Description
    #       "ac96b8",          0     icao24          string  Unique ICAO 24-bit address of the transponder in hex string representation.
    #       "AAL1042 ",        1     callsign        string  Callsign of the vehicle (8 chars). Can be null if no callsign has been received.
    #       "United States",   2     origin_country  string  Country name inferred from the ICAO 24-bit address.
    #       1526690760,        3     time_position   int     Unix timestamp (seconds) for the last position update.
    #       1526690760,        4     last_contact    int     Unix timestamp (seconds) for the last update in general.
    #       -77.226,           5     longitude       float   WGS-84 longitude in decimal degrees.
    #       35.5134,           6     latitude        float   WGS-84 latitude in decimal degrees.
    #       9685.02,           7     geo_altitude    float   Geometric altitude in meters.
    #       false,             8     on_ground       boolean Boolean value which indicates if the position was retrieved from a surface position report.
    #       219.47,            9     velocity        float   Velocity over ground in m/s.
    #       232.62,            10    heading         float   Heading in decimal degrees clockwise from north (i.e. north=0°).
    #       -4.88,             11    vertical_rate   float   Vertical rate in m/, positive for climbing, negative for descending.
    #       null,              12    sensors         int[]   IDs of the receivers which contributed to this state vector.
    #       10187.94,          13    baro_altitude   float   Barometric altitude in meters.
    #       "3517",            14    squawk          string  The transponder code aka Squawk.
    #       false,             15    spi             boolean Whether flight status indicates special purpose indicator.
    #       0                  16    position_source int     Origin of this state’s position: 0 = ADS-B, 1 = ASTERIX, 2 = MLAT
    #
    */

    /*private async void GetFlights() {
        Debug.Log("Calling OpenSky");
        try {
            //4248f5
            //new string[] { "a65ed4", "a77ec5", "ab6fdd" }
            var json = await FlightsApi();
            Debug.Log("Json: " + json);

            var parsed = JObject.Parse(json);

            //TODO: Should we destroy?
            var planes = GameObject.Find("Planes");
            if (planes != null) { Destroy(planes); }

            var container = new GameObject("Planes");
            container.transform.SetParent(transform, false);

            var tokens = parsed["states"];
            int count = 0;
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
                var plotted = info.Plot(transform, container.transform, 0); //Default layer
                var planeScript = plotted.GetComponent<Airplane_Realtime>();
                planeScript.info = info;
                plotted.name = icao24 + "_" + callSign + "_" + heading.ToString("F2");
                count++;

                if (count > 200) { break; }
            }
        }
        catch (Exception e) {
            Debug.Log("Error with OpenSky: " + e.ToString());
        }

        await Task.Delay(TimeSpan.FromSeconds(10f), apiDelayCanceler.Token); //OpenSky has limits requests to 1 per 10 seconds - so wait 10 seconds minimum
        GetFlights();
    }*/

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
