using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace CoordinateMapper {
    public enum DataKeyFormat {
        //JsonDefaultModel, //JSON matches format outlined by my models
        JsonLatAndLngKeys, //JSON has seperate Latitude and Longitude keys in objects for each location
        JsonSingleLatLngArray, //JSON has a single array with alternating Latitude and Longitude numbers
        JsonLatLngArrays, //JSON has two arrays, one for Latitude and one for Longitude
        Csv //CSV parsing
    };

    public class DefaultVisualizer : MonoBehaviour, IDataLoader {

        [SerializeField] private TextAsset _dataFile;
        public TextAsset dataFile { get { return _dataFile; } set { _dataFile = value; } }

        [SerializeField] private GameObject pointPrefab; //TODO: Keep this?

        [SerializeField] private DataKeyFormat keyFormat;

        [SerializeField] private string latitudeKey;
        [SerializeField] private string longitudeKey;
        [SerializeField] private string magnitudeKey;

        [SerializeField] private DataLoadedEvent _loadComplete;
        public DataLoadedEvent loadComplete { get { return _loadComplete; } set { _loadComplete = value; } }

        // Start is called before the first frame update
        void Start() {
            switch(keyFormat) {
                case DataKeyFormat.JsonSingleLatLngArray:
                    if (latitudeKey == null || latitudeKey.Length == 0) {
                        Debug.Log("WARNING -- Must supply Coordinate Array key with this DataKeyFormat -- ABORTING");
                        return;
                    }
                    break;
                case DataKeyFormat.JsonLatLngArrays:
                case DataKeyFormat.JsonLatAndLngKeys:
                case DataKeyFormat.Csv:
                    if (latitudeKey == null || latitudeKey.Length == 0 || longitudeKey == null || latitudeKey.Length == 0) {
                        Debug.Log("WARNING -- Must supply Latitude / Longitude keys with this DataKeyFormat -- ABORTING");
                        return;
                    }
                    break;
            }

            ParseFile(dataFile.text);
        }

        public async void ParseLatAndLngKeys(Dictionary<string, object[]> data, bool useMagnitude) {
            var latTask = Task.Run(() => data[latitudeKey].Select(v => Convert.ToSingle(v)).ToArray());
            var lngTask = Task.Run(() => data[longitudeKey].Select(v => Convert.ToSingle(v)).ToArray());
            var magTask = Task.Run(() => useMagnitude ? data[magnitudeKey].Select(v => Convert.ToSingle(v)).ToArray() : new float[0]);

            await Task.WhenAll(latTask, lngTask, magTask);

            var lats = latTask.Result;
            var lngs = lngTask.Result;
            var mags = magTask.Result;

            CreateCoordinatePoints(lats, lngs, mags, useMagnitude);
        }

        public void ParseLatLngArrays(Dictionary<string, object[]> data, bool useMagnitude) {

            var latsArr = data[latitudeKey].Cast<object[]>().ToArray();
            var lngsArr = data[longitudeKey].Cast<object[]>().ToArray();

            List<float> lats = new List<float>();
            List<float> lngs = new List<float>();
            foreach (object[] latArr in latsArr) {
                foreach(object lat in latArr) {
                    lats.Add(Convert.ToSingle(lat));
                }
            }

            foreach (object[] lngArr in lngsArr) {
                foreach (object lng in lngArr) {
                    lngs.Add(Convert.ToSingle(lng));
                }
            }

            var mags = useMagnitude ? data[magnitudeKey].Select(v => Convert.ToSingle(v)).ToArray() : new float[0];

            CreateCoordinatePoints(lats.ToArray(), lngs.ToArray(), mags, useMagnitude);
        }

        public void ParseSingleLatLngArray(Dictionary<string, object[]> data, bool useMagnitude) {
            var coordsArrs = data[latitudeKey].Cast<object[]>().ToArray();

            var lats = new List<float>();
            var lngs = new List<float>();
            var mags = useMagnitude ? data[magnitudeKey].Select(v => Convert.ToSingle(v)).ToArray() : new float[0];

            foreach(object[] coordArr in coordsArrs) {
                for(int i = 0; i < coordArr.Length; i++) {
                    if(i % 2 == 0) { lats.Add(Convert.ToSingle(coordArr[i])); }
                    else { lngs.Add(Convert.ToSingle(coordArr[i])); }
                }
            }

            CreateCoordinatePoints(lats.ToArray(), lngs.ToArray(), mags, useMagnitude);
        }

        private void CreateCoordinatePoints(float[] lats, float[] lngs, float[] mags, bool useMagnitude) {
            if (lats.Length != lngs.Length) {
                Debug.Log("WARNING -- Parsed a different number of latitude and longitudes -- ABORTING");
                return;
            }

            if (useMagnitude && lats.Length != mags.Length) {
                Debug.Log("WARNING -- Parsed a different number of latitudes/longitudes than magnitudes -- ABORTING");
                return;
            }

            var pointsContainer = new GameObject("Points Container");
            pointsContainer.transform.SetParent(transform, false);

            for (int i = 0; i < lats.Length; i++) {
                var lat = lats[i];
                var lng = lngs[i];
                var mag = useMagnitude ? mags[i] : 1f;

                var cp = new DefaultCoordinatePoint(lat, lng, mag);
                cp.pointPrefab = pointPrefab;
                var plotted = cp.Plot(transform, pointsContainer.transform, 0);
                plotted.name = "Default Point " + i;
            }
        }

        public async void ParseFile(string fileText) {
            var hasMagnitude = magnitudeKey != null && magnitudeKey.Length > 0;

            //Switches are scoped stupidly - so define vars outside
            string[] keys;
            Dictionary<string, object[]> parsedData = null;

            switch (keyFormat) {
                case DataKeyFormat.JsonSingleLatLngArray:
                    keys = !hasMagnitude ? new string[] { latitudeKey } : new string[] { latitudeKey, magnitudeKey };
                    parsedData = await JsonParser.ParseAsync(fileText, keys);
                    ParseSingleLatLngArray(parsedData, hasMagnitude);
                    break;
                case DataKeyFormat.JsonLatLngArrays:
                    keys = !hasMagnitude ? new string[] { latitudeKey, longitudeKey } : new string[] { latitudeKey, longitudeKey, magnitudeKey };
                    parsedData = await JsonParser.ParseAsync(fileText, keys);
                    ParseLatLngArrays(parsedData, hasMagnitude);
                    break;
                case DataKeyFormat.JsonLatAndLngKeys:
                    keys = !hasMagnitude ? new string[] { latitudeKey, longitudeKey } : new string[] { latitudeKey, longitudeKey, magnitudeKey };
                    parsedData = await JsonParser.ParseAsync(fileText, keys);
                    ParseLatAndLngKeys(parsedData, hasMagnitude);
                    break;
                case DataKeyFormat.Csv:
                    keys = !hasMagnitude ? new string[] { latitudeKey, longitudeKey } : new string[] { latitudeKey, longitudeKey, magnitudeKey };
                    parsedData = await CsvParser.ParseAsync(fileText);
                    ParseLatAndLngKeys(parsedData, hasMagnitude);
                    break;
            }
        }
    }
}
