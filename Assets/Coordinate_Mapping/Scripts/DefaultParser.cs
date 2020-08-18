using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace CoordinateMapper {
    public class DefaultParser {

        private string fileText;
        private DataKeyFormat keyFormat = DataKeyFormat.JsonLatAndLngKeys;

        private string latitudeKey = null;
        private string longitudeKey = null;
        private string magnitudeKey = null;

        private bool useMagnitude = false;

        public DefaultParser(string fileText, DataKeyFormat keyFormat, string latitudeKey, string longitudeKey, string magnitudeKey) {
            this.fileText = fileText;
            this.keyFormat = keyFormat;
            this.latitudeKey = latitudeKey;
            this.longitudeKey = longitudeKey;
            this.magnitudeKey = magnitudeKey;

            useMagnitude = magnitudeKey != null && magnitudeKey.Length > 0;
        }

        public async Task<List<DefaultCoordinatePointInfo>> HandleDefaultParsing() {
            switch (keyFormat) {
                case DataKeyFormat.JsonSingleLatLngArray:
                    if (latitudeKey == null || latitudeKey.Length == 0) {
                        Debug.Log("WARNING -- Must supply Coordinate Array key with this DataKeyFormat -- ABORTING");
                        return new List<DefaultCoordinatePointInfo>();
                    }
                    break;
                case DataKeyFormat.JsonLatLngArrays:
                case DataKeyFormat.JsonLatAndLngKeys:
                case DataKeyFormat.Csv:
                    if (latitudeKey == null || latitudeKey.Length == 0 || longitudeKey == null || latitudeKey.Length == 0) {
                        Debug.Log("WARNING -- Must supply Latitude / Longitude keys with this DataKeyFormat -- ABORTING");
                        return new List<DefaultCoordinatePointInfo>();
                    }
                    break;
            }

            //Switches are scoped stupidly - so define vars outside
            string[] keys;
            Dictionary<string, object[]> parsedData = null;

            switch (keyFormat) {
                case DataKeyFormat.JsonSingleLatLngArray:
                    keys = !useMagnitude ? new string[] { latitudeKey } : new string[] { latitudeKey, magnitudeKey };
                    parsedData = await JsonParser.ParseAsync(fileText, keys);
                    return await ParseSingleLatLngArray(parsedData);
                case DataKeyFormat.JsonLatLngArrays:
                    keys = !useMagnitude ? new string[] { latitudeKey, longitudeKey } : new string[] { latitudeKey, longitudeKey, magnitudeKey };
                    parsedData = await JsonParser.ParseAsync(fileText, keys);
                    return await ParseLatLngArrays(parsedData);
                case DataKeyFormat.JsonLatAndLngKeys:
                    keys = !useMagnitude ? new string[] { latitudeKey, longitudeKey } : new string[] { latitudeKey, longitudeKey, magnitudeKey };
                    parsedData = await JsonParser.ParseAsync(fileText, keys);
                    return await ParseLatAndLngKeys(parsedData);
                case DataKeyFormat.Csv:
                    keys = !useMagnitude ? new string[] { latitudeKey, longitudeKey } : new string[] { latitudeKey, longitudeKey, magnitudeKey };
                    parsedData = await CsvParser.ParseAsync(fileText);
                    return await ParseLatAndLngKeys(parsedData);
            }

            //Shouldn't get here
            return new List<DefaultCoordinatePointInfo>();
        }

        private async Task<List<DefaultCoordinatePointInfo>> ParseLatAndLngKeys(Dictionary<string, object[]> data) {
            var latTask = Task.Run(() => data[latitudeKey].Select(v => Convert.ToSingle(v)).ToArray());
            var lngTask = Task.Run(() => data[longitudeKey].Select(v => Convert.ToSingle(v)).ToArray());
            var magTask = Task.Run(() => useMagnitude ? data[magnitudeKey].Select(v => Convert.ToSingle(v)).ToArray() : new float[0]);

            await Task.WhenAll(latTask, lngTask, magTask);

            return SerializePoints(latTask.Result, lngTask.Result, magTask.Result, useMagnitude);
        }

        private async Task<List<DefaultCoordinatePointInfo>> ParseLatLngArrays(Dictionary<string, object[]> data) {
            var latTask = Task.Run(() => data[latitudeKey].Cast<object[]>().ToArray());
            var lngTask = Task.Run(() => data[longitudeKey].Cast<object[]>().ToArray());
            var magTask = Task.Run(() => useMagnitude ? data[magnitudeKey].Select(v => Convert.ToSingle(v)).ToArray() : new float[0]);

            await Task.WhenAll(latTask, lngTask, magTask);

            var latsArr = latTask.Result;
            var lngsArr = lngTask.Result;

            var latsTask = Task.Run(() => {
                List<float> temp = new List<float>();
                foreach (object[] latArr in latsArr) {
                    foreach (object lat in latArr) {
                        temp.Add(Convert.ToSingle(lat));
                    }
                }
                return temp;
            });

            var lngsTask = Task.Run(() => {
                List<float> temp = new List<float>();
                foreach (object[] lngArr in lngsArr) {
                    foreach (object lng in lngArr) {
                        temp.Add(Convert.ToSingle(lng));
                    }
                }
                return temp;
            });

            await Task.WhenAll(latsTask, lngsTask);
            return SerializePoints(latsTask.Result.ToArray(), lngsTask.Result.ToArray(), magTask.Result, useMagnitude);
        }

        private async Task<List<DefaultCoordinatePointInfo>> ParseSingleLatLngArray(Dictionary<string, object[]> data) {
            var coordsArrs = await Task.Run(() => data[latitudeKey].Cast<object[]>().ToArray());

            var coordsTask = Task.Run(() => {
                var lats = new List<float>();
                var lngs = new List<float>();
                foreach (object[] coordArr in coordsArrs) {
                    for (int i = 0; i < coordArr.Length; i++) {
                        if (i % 2 == 0) { lats.Add(Convert.ToSingle(coordArr[i])); }
                        else { lngs.Add(Convert.ToSingle(coordArr[i])); }
                    }
                }
                return (latitudes: lats, longitudes: lngs);
            });

            var magsTask = Task.Run(() => useMagnitude ? data[magnitudeKey].Select(v => Convert.ToSingle(v)).ToArray() : new float[0]);
            await Task.WhenAll(coordsTask, magsTask);

            return SerializePoints(coordsTask.Result.latitudes.ToArray(), coordsTask.Result.longitudes.ToArray(), magsTask.Result, useMagnitude);
        }

        private List<DefaultCoordinatePointInfo> SerializePoints(float[] lats, float[] lngs, float[] mags, bool useMagnitude) {
            var points = new List<DefaultCoordinatePointInfo>();
            if (lats.Length != lngs.Length) {
                Debug.Log("WARNING -- Parsed a different number of latitude and longitudes -- ABORTING");
                return points;
            }

            if (useMagnitude && lats.Length != mags.Length) {
                Debug.Log("WARNING -- Parsed a different number of latitudes/longitudes than magnitudes -- ABORTING");
                return points;
            }

            for (int i = 0; i < lats.Length; i++) {
                var lat = lats[i];
                var lng = lngs[i];
                var mag = useMagnitude ? mags[i] : 1f;

                var cp = new DefaultCoordinatePointInfo(lat, lng, mag);
                points.Add(cp);
            }

            return points;
        }
    }
}
