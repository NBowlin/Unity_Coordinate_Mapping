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

public class OpenSkyAPI : MonoBehaviour {
    private HttpClient client = new HttpClient();

    [SerializeField] private GameObject prefab;

    async void Start() {

        //System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        //StartCoroutine(GetFlights());
        //sw.Stop();
        //Debug.Log("Get Flights Time: " + sw.ElapsedMilliseconds / 1000f);
        /*TestJob test = new TestJob();
        test.transform = transform;
        test.prefab = prefab;

        JobHandle handle = test.Schedule();
        handle.Complete();*/

        GetFlights();
        //Debug.Log("Done with test");
    }

    private async Task<string> FlightsApi() {
        return await FlightsApi(new string[] { });
    }

    private async Task<string> FlightsApi(string[] icao24s) {
        try {
            var icaos = icao24s.Aggregate("?icao24=", (e, x) => e + "," + x);
            var query = icaos.Length > 0 ? icaos : "";
            var url = "https://opensky-network.org/api/states/all" + query;
            var response = await client.GetAsync(url);
            var json = response.Content.ReadAsStringAsync().Result;
            return json;
        } catch (Exception e) {
            Debug.Log("Error with OpenSky: " + e.ToString());
            return null; //TODO: Should we return the error here maybe?
        }
    }

    private async Task ParseJson() {

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
    # null,              12    sensors         int[]   IDs of the receivers which contributed to this state vector.
    #       10187.94,          13    baro_altitude   float   Barometric altitude in meters.
    #       "3517",            14    squawk          string  The transponder code aka Squawk.
    #       false,             15    spi             boolean Whether flight status indicates special purpose indicator.
    #       0                  16    position_source int     Origin of this state’s position: 0 = ADS-B, 1 = ASTERIX, 2 = MLAT
    #
    */

    private async void GetFlights() {
        Debug.Log("Calling OpenSky");
        try {
            //4248f5
            var json = await FlightsApi(/*new string[] { "a65ed4", "a77ec5", "ab6fdd" }*/);
            Debug.Log("Json: " + json);

            var parsed = JObject.Parse(json);

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
                    if(temp.Type == JTokenType.Null) {
                        nullVal = true;
                        break;
                    }
                }

                if(nullVal) { continue; }

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
        catch(Exception e) {
            Debug.Log("Error with OpenSky: " + e.ToString());
        }
        
        await Task.Delay(TimeSpan.FromSeconds(10f));
        GetFlights();
    }

    /*IEnumerator GetFlights() {

        System.Diagnostics.Stopwatch requestSw = System.Diagnostics.Stopwatch.StartNew();
        //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/weather?id={0}&APPID={1}", CityId, API_KEY));
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://opensky-network.org/api/states/all");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();
        requestSw.Stop();
        Debug.Log("Request Time: " + requestSw.ElapsedMilliseconds / 1000f);

        //Debug.Log("Flight Response: " + jsonResponse);

        System.Diagnostics.Stopwatch parseSw = System.Diagnostics.Stopwatch.StartNew();

        var parsed = JObject.Parse(jsonResponse);

        parseSw.Stop();
        Debug.Log("Parse Time: " + parseSw.ElapsedMilliseconds / 1000f);

        System.Diagnostics.Stopwatch elseTime = System.Diagnostics.Stopwatch.StartNew();

        var planes = GameObject.Find("Planes");
        if (planes != null) { Destroy(planes); }

        var container = new GameObject("Planes");
        container.transform.SetParent(transform, false);

        var tokens = parsed["states"];
        int count = 0;
        foreach(JToken token in tokens) {
            var icao24T = token[0];
            var callsignT = token[1];
            var countryT = token[2];
            var lngT = token[5];
            var latT = token[6];
            
            if (icao24T.Type == JTokenType.Null || callsignT.Type == JTokenType.Null || countryT.Type == JTokenType.Null || lngT.Type == JTokenType.Null || latT.Type == JTokenType.Null) {
                continue;
            }

            var icao24 = (string)icao24T;
            var callsign = (string)callsignT;
            var country = (string)countryT;
            var lng = (float)lngT;
            var lat = (float)latT;

            var f = new Flight(lat, lng);
            f.pointPrefab = prefab;
            var plotted = f.Plot(transform, container.transform, 0); //Default layer

            count++;

            if(count > 200) { break; }
            //yield return null;
            //Debug.Log("Flight info: icao: " + icao24 + " | callsign: " + callsign + " | country: " + country + " | lat: " + lat + " | lng: " + lng);
        }

        elseTime.Stop();
        Debug.Log("Else Time: " + elseTime.ElapsedMilliseconds / 1000f);

        yield return new WaitForSeconds(5f);
        Debug.Log("Get New Flights!");
        StartCoroutine(GetFlights());
    }*/
}

/*public struct TestJob : IJob {
    public Transform transform;
    public GameObject prefab;

    public void Execute() {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://opensky-network.org/api/states/all");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();
        Debug.Log("Flight Response: " + jsonResponse);

        var parsed = JObject.Parse(jsonResponse);

        var planes = GameObject.Find("Planes");
        //if (planes != null) { Destroy(planes); }

        var container = new GameObject("Planes");
        container.transform.SetParent(transform, false);

        var tokens = parsed["states"];
        int count = 0;
        foreach (JToken token in tokens) {
            var icao24T = token[0];
            var callsignT = token[1];
            var countryT = token[2];
            var lngT = token[5];
            var latT = token[6];

            if (icao24T.Type == JTokenType.Null || callsignT.Type == JTokenType.Null || countryT.Type == JTokenType.Null || lngT.Type == JTokenType.Null || latT.Type == JTokenType.Null) {
                continue;
            }

            var icao24 = (string)icao24T;
            var callsign = (string)callsignT;
            var country = (string)countryT;
            var lng = (float)lngT;
            var lat = (float)latT;

            var f = new Flight(lat, lng);
            f.pointPrefab = prefab;
            var plotted = f.Plot(transform, container.transform, 0); //Default layer

            count++;

            if (count > 200) { break; }
            Debug.Log("Flight info: icao: " + icao24 + " | callsign: " + callsign + " | country: " + country + " | lat: " + lat + " | lng: " + lng);
        }
    }
}*/
